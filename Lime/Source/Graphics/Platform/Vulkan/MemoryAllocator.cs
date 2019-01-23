using System;
using System.Collections.Generic;
using TreeLib;

namespace Lime.Graphics.Platform.Vulkan
{
	internal unsafe class MemoryAllocator
	{
		private MemoryType[] memoryTypes;
		private List<MemoryBlock>[] memoryPoolsLinear;
		private List<MemoryBlock>[] memoryPoolsNonLinear;

		public PlatformRenderContext Context { get; }
		public bool PreferPersistentMapping { get; }

		public MemoryAllocator(PlatformRenderContext context, bool preferPersistentMapping)
		{
			Context = context;
			PreferPersistentMapping = preferPersistentMapping;
			Initialize();
		}

		private void Initialize()
		{
			Context.PhysicalDevice.GetMemoryProperties(out var physicalDeviceMemoryProperties);
			memoryTypes = new MemoryType[physicalDeviceMemoryProperties.MemoryTypeCount];
			for (var i = 0U; i < physicalDeviceMemoryProperties.MemoryTypeCount; i++) {
				var vkMemoryType = &physicalDeviceMemoryProperties.MemoryTypes.Value0 + i;
				var vkMemoryHeap = &physicalDeviceMemoryProperties.MemoryHeaps.Value0 + vkMemoryType->HeapIndex;
				var blockSize = PickBlockSize(vkMemoryHeap->Size);
				var minAlignment = GetMemoryTypeMinAlignment(vkMemoryType->PropertyFlags);
				memoryTypes[i] = new MemoryType(i, vkMemoryType->PropertyFlags, blockSize, minAlignment);
			}
			memoryPoolsLinear = new List<MemoryBlock>[memoryTypes.Length];
			memoryPoolsNonLinear = new List<MemoryBlock>[memoryTypes.Length];
			for (var i = 0; i < memoryTypes.Length; i++) {
				memoryPoolsLinear[i] = new List<MemoryBlock>();
				memoryPoolsNonLinear[i] = new List<MemoryBlock>();
			}
		}

		public MemoryAlloc Allocate(SharpVulkan.Image image, SharpVulkan.MemoryPropertyFlags propertyFlags, SharpVulkan.ImageTiling tiling)
		{
			GetImageMemoryRequirements(image,
				out var requirements,
				out var prefersDedicated,
				out bool requiresDedicated);
			var dedicatedAllocateInfo = new SharpVulkan.Ext.MemoryDedicatedAllocateInfo {
				StructureType = SharpVulkan.Ext.StructureType.MemoryDedicatedAllocateInfo,
				Image = image
			};
			var alloc = Allocate(
				requirements, &dedicatedAllocateInfo, prefersDedicated, requiresDedicated,
				propertyFlags, tiling == SharpVulkan.ImageTiling.Linear);
			Context.Device.BindImageMemory(image, alloc.Memory.Memory, alloc.Offset);
			return alloc;
		}

		public MemoryAlloc Allocate(SharpVulkan.Buffer buffer, SharpVulkan.MemoryPropertyFlags propertyFlags)
		{
			GetBufferMemoryRequirements(buffer,
				out var requirements,
				out var prefersDedicated,
				out bool requiresDedicated);
			var dedicatedAllocateInfo = new SharpVulkan.Ext.MemoryDedicatedAllocateInfo {
				StructureType = SharpVulkan.Ext.StructureType.MemoryDedicatedAllocateInfo,
				Buffer = buffer
			};
			var alloc = Allocate(
				requirements, &dedicatedAllocateInfo, prefersDedicated, requiresDedicated,
				propertyFlags, true);
			Context.Device.BindBufferMemory(buffer, alloc.Memory.Memory, alloc.Offset);
			return alloc;
		}

		private MemoryAlloc Allocate(
			SharpVulkan.MemoryRequirements requirements, SharpVulkan.Ext.MemoryDedicatedAllocateInfo* dedicatedAllocateInfo,
			bool prefersDedicated, bool requiresDedicated, SharpVulkan.MemoryPropertyFlags propertyFlags, bool linear)
		{
			var type = TryFindMemoryType(requirements.MemoryTypeBits, propertyFlags);
			if (type == null) {
				throw new InvalidOperationException();
			}
			if (prefersDedicated) {
				var memory = TryAllocateDeviceMemory(type, requirements.Size, dedicatedAllocateInfo);
				if (memory != null) {
					return new MemoryAlloc(this, memory);
				}
				if (requiresDedicated) {
					throw new OutOfMemoryException();
				}
			}
			return AllocateFromBlock(type, requirements.Size, requirements.Alignment, linear);
		}

		private DeviceMemory TryAllocateDeviceMemory(MemoryType type, ulong size, SharpVulkan.Ext.MemoryDedicatedAllocateInfo* dedicatedAllocateInfo)
		{
			var allocateInfo = new SharpVulkan.MemoryAllocateInfo {
				StructureType = SharpVulkan.StructureType.MemoryAllocateInfo,
				MemoryTypeIndex = type.Index,
				AllocationSize = size,
				Next = new IntPtr(dedicatedAllocateInfo)
			};
			DeviceMemory memory;
			try {
				memory = new DeviceMemory(Context.Device.AllocateMemory(ref allocateInfo), type, size);
			} catch (SharpVulkan.SharpVulkanException e) when (e.Result == SharpVulkan.Result.ErrorOutOfDeviceMemory) {
				return null;
			}
			if (ShouldMapPersistenly(type)) {
				MapDeviceMemory(memory);
			}
			return memory;
		}

		private void FreeDeviceMemory(DeviceMemory memory)
		{
			if (ShouldMapPersistenly(memory.Type)) {
				UnmapDeviceMemory(memory);
			}
			if (memory.MapCounter > 0) {
				throw new InvalidOperationException();
			}
			Context.Device.FreeMemory(memory.Memory);
		}

		private bool ShouldMapPersistenly(MemoryType type)
		{
			return PreferPersistentMapping && (type.PropertyFlags & SharpVulkan.MemoryPropertyFlags.HostVisible) != 0;
		}

		private MemoryAlloc AllocateFromBlock(MemoryType type, ulong size, ulong alignment, bool linear)
		{
			if (type.BlockSize < size) {
				throw new OutOfMemoryException();
			}
			alignment = GraphicsUtility.CombineAlignment(alignment, type.MinAlignment);
			var pool = linear ? memoryPoolsLinear[type.Index] : memoryPoolsNonLinear[type.Index];
			foreach (var block in pool) {
				var blockNode = block.TryAllocate(size, alignment);
				if (blockNode != null) {
					return new MemoryAlloc(this, block, blockNode);
				}
			}
			var newBlockMemory = TryAllocateDeviceMemory(type, type.BlockSize, null);
			if (newBlockMemory == null) {
				throw new OutOfMemoryException();
			}
			var newBlock = new MemoryBlock(newBlockMemory, type.BlockSize);
			var newBlockNode = newBlock.TryAllocate(size, alignment);
			if (newBlockNode == null) {
				throw new OutOfMemoryException();
			}
			pool.Add(newBlock);
			return new MemoryAlloc(this, newBlock, newBlockNode);
		}

		public void Free(MemoryAlloc alloc)
		{
			if (alloc == null || alloc.Allocator == null) {
				return;
			}
			if (alloc.Allocator != this) {
				throw new ArgumentException(nameof(alloc));
			}
			var block = alloc.MemoryBlock;
			if (block != null) {
				block.Free(alloc.MemoryBlockNode);
			} else {
				FreeDeviceMemory(alloc.Memory);
			}
			alloc.Allocator = null;
		}

		private ulong GetMemoryTypeMinAlignment(SharpVulkan.MemoryPropertyFlags propertyFlags)
		{
			var hostVisible = SharpVulkan.MemoryPropertyFlags.HostVisible;
			var hostCoherent = SharpVulkan.MemoryPropertyFlags.HostCoherent;
			if ((propertyFlags & (hostVisible | hostCoherent)) == hostVisible) {
				return Context.PhysicalDeviceLimits.NonCoherentAtomSize;
			}
			return 1;
		}

		public IntPtr Map(MemoryAlloc alloc)
		{
			if (alloc.Allocator != this) {
				throw new ArgumentException(nameof(alloc));
			}
			return new IntPtr((byte*)MapDeviceMemory(alloc.Memory) + alloc.Offset);
		}

		public void Unmap(MemoryAlloc alloc)
		{
			if (alloc.Allocator != this) {
				throw new ArgumentException(nameof(alloc));
			}
			UnmapDeviceMemory(alloc.Memory);
		}

		private IntPtr MapDeviceMemory(DeviceMemory memory)
		{
			if ((memory.Type.PropertyFlags & SharpVulkan.MemoryPropertyFlags.HostVisible) == 0) {
				throw new InvalidOperationException();
			}
			lock (memory) {
				memory.MapCounter++;
				if (memory.MapCounter == 1) {
					memory.MappedMemory = Context.Device.MapMemory(memory.Memory, 0, memory.Size, SharpVulkan.MemoryMapFlags.None);
				}
				return memory.MappedMemory;
			}
		}

		private void UnmapDeviceMemory(DeviceMemory memory)
		{
			lock (memory) {
				if (memory.MapCounter == 0) {
					throw new InvalidOperationException();
				}
				memory.MapCounter--;
				if (memory.MapCounter == 0) {
					Context.Device.UnmapMemory(memory.Memory);
				}
			}
		}

		public void FlushMappedMemoryRange(MemoryAlloc alloc, ulong offset, ulong size)
		{
			FlushOrInvalidateMappedMemoryRange(alloc, offset, size, flush: true);
		}

		public void InvalidateMappedMemoryRange(MemoryAlloc alloc, ulong offset, ulong size)
		{
			FlushOrInvalidateMappedMemoryRange(alloc, offset, size, flush: false);
		}

		private void FlushOrInvalidateMappedMemoryRange(MemoryAlloc alloc, ulong offset, ulong size, bool flush)
		{
			if (alloc.Allocator != this) {
				throw new ArgumentException(nameof(alloc));
			}
			if (offset + size < alloc.Size) {
				throw new ArgumentException();
			}
			if (size == 0) {
				return;
			}
			var memoryType = alloc.Memory.Type;
			var hostCoherent = (memoryType.PropertyFlags & SharpVulkan.MemoryPropertyFlags.HostCoherent) != 0;
			if (hostCoherent) {
				return;
			}
			var nonCoherentAtomSize = Context.PhysicalDeviceLimits.NonCoherentAtomSize;
			var rangeStart = GraphicsUtility.AlignDown(alloc.Offset + offset, nonCoherentAtomSize);
			var rangeEnd = GraphicsUtility.AlignUp(alloc.Offset + offset + size, nonCoherentAtomSize);
			if (rangeEnd > alloc.Memory.Size) {
				rangeEnd = alloc.Memory.Size;
			}
			var range = new SharpVulkan.MappedMemoryRange {
				StructureType = SharpVulkan.StructureType.MappedMemoryRange,
				Memory = alloc.Memory.Memory,
				Offset = rangeStart,
				Size = rangeEnd - rangeStart
			};
			if (flush) {
				Context.Device.FlushMappedMemoryRanges(1, &range);
			} else {
				Context.Device.InvalidateMappedMemoryRanges(1, &range);
			}
		}

		private void GetImageMemoryRequirements(
			SharpVulkan.Image image,
			out SharpVulkan.MemoryRequirements requirements,
			out bool prefersDedicatedAllocation,
			out bool requiresDedicatedAllocation)
		{
			if (Context.SupportsDedicatedAllocation) {
				var requirementsInfo = new SharpVulkan.Ext.ImageMemoryRequirementsInfo2 {
					StructureType = SharpVulkan.Ext.StructureType.ImageMemoryRequirementsInfo2,
					Image = image
				};
				var dedicatedRequirements = new SharpVulkan.Ext.MemoryDedicatedRequirements {
					StructureType = SharpVulkan.Ext.StructureType.MemoryDedicatedRequirements
				};
				var requirements2 = new SharpVulkan.Ext.MemoryRequirements2 {
					StructureType = SharpVulkan.Ext.StructureType.MemoryRequirements2,
					Next = new IntPtr(&dedicatedRequirements)
				};
				Context.VKExt.GetImageMemoryRequirements2(Context.Device, ref requirementsInfo, ref requirements2);
				requirements = requirements2.MemoryRequirements;
				prefersDedicatedAllocation = dedicatedRequirements.PrefersDedicatedAllocation;
				requiresDedicatedAllocation = dedicatedRequirements.RequiresDedicatedAllocation;
			} else {
				Context.Device.GetImageMemoryRequirements(image, out requirements);
				prefersDedicatedAllocation = false;
				requiresDedicatedAllocation = false;
			}
		}

		private void GetBufferMemoryRequirements(
			SharpVulkan.Buffer buffer,
			out SharpVulkan.MemoryRequirements requirements,
			out bool prefersDedicatedAllocation,
			out bool requiresDedicatedAllocation)
		{
			if (Context.SupportsDedicatedAllocation) {
				var requirementsInfo = new SharpVulkan.Ext.BufferMemoryRequirementsInfo2 {
					StructureType = SharpVulkan.Ext.StructureType.BufferMemoryRequirementsInfo2,
					Buffer = buffer
				};
				var dedicatedRequirements = new SharpVulkan.Ext.MemoryDedicatedRequirements {
					StructureType = SharpVulkan.Ext.StructureType.MemoryDedicatedRequirements
				};
				var requirements2 = new SharpVulkan.Ext.MemoryRequirements2 {
					StructureType = SharpVulkan.Ext.StructureType.MemoryRequirements2,
					Next = new IntPtr(&dedicatedRequirements)
				};
				Context.VKExt.GetBufferMemoryRequirements2(Context.Device, ref requirementsInfo, ref requirements2);
				requirements = requirements2.MemoryRequirements;
				prefersDedicatedAllocation = dedicatedRequirements.PrefersDedicatedAllocation;
				requiresDedicatedAllocation = dedicatedRequirements.RequiresDedicatedAllocation;
			} else {
				Context.Device.GetBufferMemoryRequirements(buffer, out requirements);
				prefersDedicatedAllocation = false;
				requiresDedicatedAllocation = false;
			}
		}

		private MemoryType TryFindMemoryType(uint typeBits, SharpVulkan.MemoryPropertyFlags propertyFlags)
		{
			foreach (var type in memoryTypes) {
				var mask = 1 << (int)type.Index;
				if ((typeBits & mask) != 0 && (type.PropertyFlags & propertyFlags) == propertyFlags) {
					return type;
				}
			}
			return null;
		}

		private static ulong PickBlockSize(ulong heapSize)
		{
			const ulong MaxBlockSize = 64 * 1024 * 1024;
			const ulong MinBlockCount = 16;
			return Math.Min(heapSize / MinBlockCount, MaxBlockSize);
		}
	}

	internal class MemoryType
	{
		public readonly uint Index;
		public readonly SharpVulkan.MemoryPropertyFlags PropertyFlags;
		public readonly ulong BlockSize;
		public readonly ulong MinAlignment;

		public MemoryType(uint index, SharpVulkan.MemoryPropertyFlags propertyFlags, ulong blockSize, ulong minAlignment)
		{
			Index = index;
			PropertyFlags = propertyFlags;
			BlockSize = blockSize;
			MinAlignment = minAlignment;
		}
	}

	internal class MemoryBlock
	{
		private RedBlackTreeMap<FreeTreeKey, LinkedListNode<MemoryBlockSlice>> freeTree = new RedBlackTreeMap<FreeTreeKey, LinkedListNode<MemoryBlockSlice>>();
		private LinkedList<MemoryBlockSlice> list = new LinkedList<MemoryBlockSlice>();

		public readonly DeviceMemory Memory;

		public MemoryBlock(DeviceMemory memory, ulong size)
		{
			Memory = memory;
			AddSliceBefore(null, new MemoryBlockSlice { Size = size, Free = true });
		}

		public LinkedListNode<MemoryBlockSlice> TryAllocate(ulong size, ulong alignment)
		{
			if (freeTree.NearestGreaterOrEqual(new FreeTreeKey(size + alignment - 1), out _, out var node)) {
				var slice = node.Value;
				var offset = GraphicsUtility.AlignUp(slice.Offset, alignment);
				if (offset > slice.Offset) {
					AddSliceBefore(node, new MemoryBlockSlice {
						Offset = slice.Offset,
						Size = offset - slice.Offset,
						Free = true
					});
				}
				var allocNode = AddSliceBefore(node, new MemoryBlockSlice {
					Offset = offset,
					Size = size,
					Free = false
				});
				if (offset + size < slice.Offset + slice.Size) {
					AddSliceBefore(node, new MemoryBlockSlice {
						Offset = offset + size,
						Size = slice.Offset + slice.Size - offset - size,
						Free = true
					});
				}
				RemoveSliceNode(node);
				return allocNode;
			}
			return null;
		}

		public void Free(LinkedListNode<MemoryBlockSlice> node)
		{
			var offset = node.Value.Offset;
			var size = node.Value.Size;
			var prev = node.Previous;
			if (prev != null && prev.Value.Free) {
				offset = prev.Value.Offset;
				size += prev.Value.Size;
				RemoveSliceNode(prev);
			}
			var next = node.Next;
			if (next != null && next.Value.Free) {
				size += next.Value.Size;
				RemoveSliceNode(next);
			}
			AddSliceBefore(node, new MemoryBlockSlice {
				Offset = offset,
				Size = size,
				Free = true
			});
			RemoveSliceNode(node);
		}

		private LinkedListNode<MemoryBlockSlice> AddSliceBefore(LinkedListNode<MemoryBlockSlice> beforeNode, MemoryBlockSlice slice)
		{
			var node = beforeNode != null
				? list.AddBefore(beforeNode, slice)
				: list.AddLast(slice);
			if (slice.Free) {
				freeTree.Add(new FreeTreeKey(slice.Offset, slice.Size), node);
			}
			return node;
		}

		private void RemoveSliceNode(LinkedListNode<MemoryBlockSlice> node)
		{
			var slice = node.Value;
			if (slice.Free) {
				freeTree.Remove(new FreeTreeKey(slice.Offset, slice.Size));
			}
			list.Remove(node);
		}

		private struct FreeTreeKey : IComparable<FreeTreeKey>
		{
			public ulong Offset;
			public ulong Size;

			public FreeTreeKey(ulong size)
			{
				Offset = 0;
				Size = size;
			}

			public FreeTreeKey(ulong offset, ulong size)
			{
				Offset = offset;
				Size = size;
			}

			public int CompareTo(FreeTreeKey key)
			{
				var order = Size.CompareTo(key.Size);
				if (order == 0) {
					order = Offset.CompareTo(key.Offset);
				}
				return order;
			}
		}
	}

	internal struct MemoryBlockSlice
	{
		public ulong Offset;
		public ulong Size;
		public bool Free;
	}

	internal class MemoryAlloc
	{
		public MemoryAllocator Allocator;
		public MemoryBlock MemoryBlock;
		public LinkedListNode<MemoryBlockSlice> MemoryBlockNode;
		public DeviceMemory Memory;
		public ulong Offset;
		public ulong Size;

		public MemoryAlloc(MemoryAllocator allocator, DeviceMemory memory)
		{
			Allocator = allocator;
			Memory = memory;
			Size = memory.Size;
		}

		public MemoryAlloc(MemoryAllocator allocator, MemoryBlock memoryBlock, LinkedListNode<MemoryBlockSlice> memoryBlockNode)
		{
			Allocator = allocator;
			MemoryBlock = memoryBlock;
			MemoryBlockNode = memoryBlockNode;
			Memory = memoryBlock.Memory;
			Offset = memoryBlockNode.Value.Offset;
			Size = memoryBlockNode.Value.Size;
		}
	}

	internal class DeviceMemory
	{
		public SharpVulkan.DeviceMemory Memory;
		public MemoryType Type;
		public ulong Size;
		public int MapCounter;
		public IntPtr MappedMemory;

		public DeviceMemory(SharpVulkan.DeviceMemory memory, MemoryType type, ulong size)
		{
			Memory = memory;
			Type = type;
			Size = size;
		}
	}
}
