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

		public MemoryAllocator(PlatformRenderContext context)
		{
			Context = context;
			Initialize();
		}

		private void Initialize()
		{
			Context.PhysicalDevice.GetMemoryProperties(out var physicalDeviceMemoryProperties);
			memoryTypes = new MemoryType[physicalDeviceMemoryProperties.MemoryTypeCount];
			for (var i = 0U; i < physicalDeviceMemoryProperties.MemoryTypeCount; i++) {
				var vkMemoryType = &physicalDeviceMemoryProperties.MemoryTypes.Value0 + i;
				var vkMemoryHeap = &physicalDeviceMemoryProperties.MemoryHeaps.Value0 + vkMemoryType->HeapIndex;
				memoryTypes[i] = new MemoryType(i, vkMemoryType->PropertyFlags, PickBlockSize(vkMemoryHeap->Size));
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
			Context.Device.BindImageMemory(image, alloc.Memory, alloc.Offset);
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
			Context.Device.BindBufferMemory(buffer, alloc.Memory, alloc.Offset);
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
				if (memory != SharpVulkan.DeviceMemory.Null) {
					return new MemoryAlloc(type, memory, requirements.Size);
				}
				if (requiresDedicated) {
					throw new OutOfMemoryException();
				}
			}
			return AllocateFromBlock(type, requirements.Size, requirements.Alignment, linear);
		}

		private SharpVulkan.DeviceMemory TryAllocateDeviceMemory(MemoryType type, ulong size, SharpVulkan.Ext.MemoryDedicatedAllocateInfo* dedicatedAllocateInfo)
		{
			var allocateInfo = new SharpVulkan.MemoryAllocateInfo {
				StructureType = SharpVulkan.StructureType.MemoryAllocateInfo,
				MemoryTypeIndex = type.Index,
				AllocationSize = size,
				Next = new IntPtr(dedicatedAllocateInfo)
			};
			try {
				return Context.Device.AllocateMemory(ref allocateInfo);
			} catch (SharpVulkan.SharpVulkanException e) when (e.Result == SharpVulkan.Result.ErrorOutOfDeviceMemory) {
				return SharpVulkan.DeviceMemory.Null;
			}
		}

		private MemoryAlloc AllocateFromBlock(MemoryType type, ulong size, ulong alignment, bool linear)
		{
			if (type.BlockSize < size) {
				throw new OutOfMemoryException();
			}
			var pool = linear ? memoryPoolsLinear[type.Index] : memoryPoolsNonLinear[type.Index];
			foreach (var block in pool) {
				var blockNode = block.TryAllocate(size, alignment);
				if (blockNode != null) {
					return new MemoryAlloc(type, block, blockNode);
				}
			}
			var newBlockMemory = TryAllocateDeviceMemory(type, type.BlockSize, null);
			if (newBlockMemory == SharpVulkan.DeviceMemory.Null) {
				throw new OutOfMemoryException();
			}
			var newBlock = new MemoryBlock(newBlockMemory, type.BlockSize);
			var newBlockNode = newBlock.TryAllocate(size, alignment);
			if (newBlockNode == null) {
				throw new OutOfMemoryException();
			}
			pool.Add(newBlock);
			return new MemoryAlloc(type, newBlock, newBlockNode);
		}

		public void Free(MemoryAlloc alloc)
		{
			var block = alloc.MemoryBlock;
			if (block != null) {
				block.Free(alloc.MemoryBlockNode);
			} else {
				Context.Device.FreeMemory(alloc.Memory);
			}
		}

		public IntPtr Map(MemoryAlloc alloc, ulong offset, ulong size)
		{
			if (size == SharpVulkan.Vulkan.WholeSize) {
				size = alloc.Size;
			}
			if (offset + size > alloc.Size) {
				throw new InvalidOperationException();
			}
			var memory = alloc.MemoryBlock.Memory;
			return Context.Device.MapMemory(memory, alloc.Offset + offset, size, SharpVulkan.MemoryMapFlags.None);
		}

		public void Unmap(MemoryAlloc alloc)
		{
			var memory = alloc.MemoryBlock.Memory;
			Context.Device.UnmapMemory(memory);
		}

		internal void GetImageMemoryRequirements(
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

		public MemoryType(uint index, SharpVulkan.MemoryPropertyFlags propertyFlags, ulong blockSize)
		{
			Index = index;
			PropertyFlags = propertyFlags;
			BlockSize = blockSize;
		}
	}

	internal class MemoryBlock
	{
		private RedBlackTreeMap<FreeTreeKey, LinkedListNode<MemoryBlockSlice>> freeTree = new RedBlackTreeMap<FreeTreeKey, LinkedListNode<MemoryBlockSlice>>();
		private LinkedList<MemoryBlockSlice> list = new LinkedList<MemoryBlockSlice>();

		public readonly SharpVulkan.DeviceMemory Memory;

		public MemoryBlock(SharpVulkan.DeviceMemory memory, ulong size)
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

	internal struct MemoryAlloc
	{
		public readonly MemoryType MemoryType;
		public readonly MemoryBlock MemoryBlock;
		public readonly LinkedListNode<MemoryBlockSlice> MemoryBlockNode;
		public readonly SharpVulkan.DeviceMemory Memory;
		public readonly ulong Offset;
		public readonly ulong Size;

		public MemoryAlloc(MemoryType memoryType, SharpVulkan.DeviceMemory memory, ulong size)
		{
			MemoryType = memoryType;
			MemoryBlock = null;
			MemoryBlockNode = null;
			Memory = memory;
			Offset = 0;
			Size = size;
		}

		public MemoryAlloc(MemoryType memoryType, MemoryBlock memoryBlock, LinkedListNode<MemoryBlockSlice> memoryBlockNode)
		{
			MemoryType = memoryType;
			MemoryBlock = memoryBlock;
			MemoryBlockNode = memoryBlockNode;
			Memory = memoryBlock.Memory;
			Offset = memoryBlockNode.Value.Offset;
			Size = memoryBlockNode.Value.Size;
		}
	}
}
