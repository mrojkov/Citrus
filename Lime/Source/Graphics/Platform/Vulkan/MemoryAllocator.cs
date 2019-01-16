using System;
using System.Collections.Generic;

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
			var offset = memoryBlock.Allocate(requirements.Size, requirements.Alignment);
			if (offset != ulong.MaxValue) {
				return new MemoryAlloc(memoryType, memoryBlock, memoryBlock.Memory, offset, requirements.Size);
			}
			return new MemoryAlloc();
		}

		public void Free(MemoryAlloc alloc)
		{
			var block = alloc.MemoryBlock;
			if (block != null) {
				block.Free(alloc.Offset, alloc.Size);
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
		// TODO: Use RB tree instead of LinkedList
		private LinkedList<Slice> freeList = new LinkedList<Slice>();

		public readonly SharpVulkan.DeviceMemory Memory;

		public MemoryBlock(SharpVulkan.DeviceMemory memory, ulong size)
		{
			Memory = memory;
			freeList.AddFirst(new Slice {
				Offset = 0,
				Size = size
			});
		}

		public ulong Allocate(ulong size, ulong alignment)
		{
			ulong bestFitness = ulong.MaxValue;
			ulong bestOffset = 0;
			LinkedListNode<Slice> bestSliceNode = null;
			for (var sliceNode = freeList.First; sliceNode != null; sliceNode = sliceNode.Next) {
				var slice = sliceNode.Value;
				var offset = GraphicsUtility.AlignUp(slice.Offset, alignment);
				if (offset + size <= slice.Offset + slice.Size) {
					var fitness = slice.Offset + slice.Size - offset - size;
					if (fitness < bestFitness) {
						bestFitness = fitness;
						bestOffset = offset;
						bestSliceNode = sliceNode;
					}
				}
			}
			if (bestSliceNode == null) {
				return ulong.MaxValue;
			}
			var bestSlice = bestSliceNode.Value;
			if (bestOffset > bestSlice.Offset) {
				freeList.AddBefore(bestSliceNode, new Slice {
					Offset = bestSlice.Offset,
					Size = bestOffset - bestSlice.Offset
				});
			}
			if (bestOffset + size < bestSlice.Offset + bestSlice.Size) {
				freeList.AddBefore(bestSliceNode, new Slice {
					Offset = bestOffset + size,
					Size = bestSlice.Offset + bestSlice.Size - bestOffset - size
				});
			}
			freeList.Remove(bestSliceNode);
			return bestOffset;
		}

		public void Free(ulong offset, ulong size)
		{
			var sliceNode = freeList.First;
			while (sliceNode != null) {
				var next = sliceNode.Next;
				var slice = sliceNode.Value;
				if (slice.Offset + slice.Size == offset) {
					offset = slice.Offset;
					size += slice.Size;
					freeList.Remove(sliceNode);
				} else if (slice.Offset == offset + size) {
					size += slice.Size;
					freeList.Remove(sliceNode);
				}
				sliceNode = next;
			}
			freeList.AddLast(new Slice {
				Offset = offset,
				Size = size
			});
		}

		private struct Slice
		{
			public ulong Offset;
			public ulong Size;
		}
	}

	internal struct MemoryAlloc
	{
		public readonly MemoryType MemoryType;
		public readonly MemoryBlock MemoryBlock;
		public readonly SharpVulkan.DeviceMemory Memory;
		public readonly ulong Offset;
		public readonly ulong Size;

		public MemoryAlloc(MemoryType memoryType, MemoryBlock memoryBlock, SharpVulkan.DeviceMemory memory, ulong offset, ulong size)
		{
			MemoryType = memoryType;
			MemoryBlock = memoryBlock;
			Memory = memory;
			Offset = offset;
			Size = size;
		}
	}
}
