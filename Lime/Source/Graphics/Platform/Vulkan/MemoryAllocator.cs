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
				memoryTypes[i] = new MemoryType(vkMemoryType->PropertyFlags, PickBlockSize(vkMemoryHeap->Size));
			}
			memoryPoolsLinear = new List<MemoryBlock>[memoryTypes.Length];
			memoryPoolsNonLinear = new List<MemoryBlock>[memoryTypes.Length];
			for (var i = 0; i < memoryTypes.Length; i++) {
				memoryPoolsLinear[i] = new List<MemoryBlock>();
				memoryPoolsNonLinear[i] = new List<MemoryBlock>();
			}
		}

		public MemoryAlloc Allocate(SharpVulkan.MemoryRequirements requirements, SharpVulkan.MemoryPropertyFlags propertyFlags, bool linear)
		{
			var typeIndex = FindMemoryTypeIndex(requirements.MemoryTypeBits, propertyFlags);
			if (typeIndex == uint.MaxValue) {
				throw new InvalidOperationException();
			}
			var type = memoryTypes[typeIndex];
			if (type.BlockSize < requirements.Size) {
				throw new OutOfMemoryException();
			}
			var pool = linear ? memoryPoolsLinear[typeIndex] : memoryPoolsNonLinear[typeIndex];
			foreach (var block in pool) {
				var alloc = AllocateFromBlock(type, block, requirements);
				if (alloc.MemoryType != null) {
					return alloc;
				}
			}
			var newBlock = AllocateBlock(typeIndex);
			pool.Add(newBlock);
			return AllocateFromBlock(type, newBlock, requirements);
		}

		private MemoryAlloc AllocateFromBlock(MemoryType memoryType, MemoryBlock memoryBlock, SharpVulkan.MemoryRequirements requirements)
		{
			var node = memoryBlock.Allocate(requirements.Size, requirements.Alignment);
			if (node != null) {
				return new MemoryAlloc(memoryType, memoryBlock, node, memoryBlock.Memory);
			}
			return new MemoryAlloc();
		}

		public void Free(MemoryAlloc alloc)
		{
			var block = alloc.MemoryBlock;
			if (block != null) {
				block.Free(alloc.MemoryBlockNode);
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

		private MemoryBlock AllocateBlock(uint memoryTypeIndex)
		{
			var memoryType = memoryTypes[memoryTypeIndex];
			var memoryAllocateInfo = new SharpVulkan.MemoryAllocateInfo {
				StructureType = SharpVulkan.StructureType.MemoryAllocateInfo,
				MemoryTypeIndex = memoryTypeIndex,
				AllocationSize = memoryType.BlockSize
			};
			var memory = Context.Device.AllocateMemory(ref memoryAllocateInfo);
			return new MemoryBlock(memory, memoryType.BlockSize);
		}

		private uint FindMemoryTypeIndex(uint typeBits, SharpVulkan.MemoryPropertyFlags propertyFlags)
		{
			for (var i = 0U; i < memoryTypes.Length; i++) {
				if ((typeBits & 1) == 1) {
					if ((memoryTypes[i].PropertyFlags & propertyFlags) == propertyFlags) {
						return i;
					}
				}
				typeBits >>= 1;
			}
			return uint.MaxValue;
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
		public readonly SharpVulkan.MemoryPropertyFlags PropertyFlags;
		public readonly ulong BlockSize;

		public MemoryType(SharpVulkan.MemoryPropertyFlags propertyFlags, ulong blockSize)
		{
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

		public LinkedListNode<MemoryBlockSlice> Allocate(ulong size, ulong alignment)
		{
			if (freeTree.NearestGreaterOrEqual(new FreeTreeKey(size + alignment - 1), out _, out var node)) {
				var slice = node.Value;
				var alignedOffset = GraphicsUtility.AlignUp(slice.Offset, alignment);
				var next = node.Next;
				if (alignedOffset > slice.Offset) {
					AddSliceBefore(node, new MemoryBlockSlice {
						Offset = slice.Offset,
						Size = alignedOffset - slice.Offset,
						Free = true
					});
				}
				var allocNode = AddSliceBefore(node, new MemoryBlockSlice {
					Offset = alignedOffset,
					Size = size,
					Free = false
				});
				if (alignedOffset + size < slice.Offset + slice.Size) {
					AddSliceBefore(node, new MemoryBlockSlice {
						Offset = alignedOffset + size,
						Size = slice.Offset + slice.Size - alignedOffset - size,
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

		public ulong Offset => MemoryBlockNode.Value.Offset;
		public ulong Size => MemoryBlockNode.Value.Size;

		public MemoryAlloc(
			MemoryType memoryType, MemoryBlock memoryBlock,
			LinkedListNode<MemoryBlockSlice> memoryBlockNode,
			SharpVulkan.DeviceMemory memory)
		{
			MemoryType = memoryType;
			MemoryBlock = memoryBlock;
			MemoryBlockNode = memoryBlockNode;
			Memory = memory;
		}
	}
}
