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
			var newBlock = new MemoryBlock(newBlockMemory);
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
			if (alloc.Size < offset + size) {
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
		private static readonly Node treeSentinel;

		private Node treeRoot = treeSentinel;
		private Node first;
		private Node last;

		public readonly DeviceMemory Memory;

		static MemoryBlock()
		{
			treeSentinel = new Node();
			treeSentinel.Left = treeSentinel;
			treeSentinel.Right = treeSentinel;
			treeSentinel.Parent = null;
			treeSentinel.Color = NodeColor.Black;
		}

		public MemoryBlock(DeviceMemory memory)
		{
			Memory = memory;
			first = last = new Node {
				Offset = 0,
				Size = memory.Size
			};
			TreeInsert(first);
		}

		public Node TryAllocate(ulong size, ulong alignment)
		{
			var free = TreeSearch(size + alignment - 1);
			if (free == null) {
				return null;
			}
			TreeRemove(free);
			var offset = GraphicsUtility.AlignUp(free.Offset, alignment);
			if (offset > free.Offset) {
				var frag = new Node {
					Offset = free.Offset,
					Size = offset - free.Offset
				};
				TreeInsert(frag);
				ListInsertBefore(free, frag);
			}
			if (offset + size < free.Offset + free.Size) {
				var frag = new Node {
					Offset = offset + size,
					Size = free.Offset + free.Size - offset - size
				};
				TreeInsert(frag);
				ListInsertAfter(free, frag);
			}
			free.Offset = offset;
			free.Size = size;
			return free;
		}

		public void Free(Node node)
		{
			var freeOffset = node.Offset;
			var freeSize = node.Size;
			if (node.Prev != null && IsFree(node.Prev)) {
				freeOffset = node.Prev.Offset;
				freeSize += node.Prev.Size;
				TreeRemove(node.Prev);
				ListRemove(node.Prev);
			}
			if (node.Next != null && IsFree(node.Next)) {
				freeSize += node.Next.Size;
				TreeRemove(node.Next);
				ListRemove(node.Next);
			}
			node.Offset = freeOffset;
			node.Size = freeSize;
			TreeInsert(node);
		}

		private bool IsFree(Node node)
		{
			return node.Parent != null || node == treeRoot;
		}

		private void ListInsertBefore(Node beforeNode, Node node)
		{
			node.Next = beforeNode;
			node.Prev = beforeNode.Prev;
			if (node.Prev != null) {
				node.Prev.Next = node;
			} else {
				first = node;
			}
			beforeNode.Prev = node;
		}

		private void ListInsertAfter(Node afterNode, Node node)
		{
			node.Prev = afterNode;
			node.Next = afterNode.Next;
			if (node.Next != null) {
				node.Next.Prev = node;
			} else {
				last = node;
			}
			afterNode.Next = node;
		}

		private void ListRemove(Node node)
		{
			if (node.Prev != null) {
				node.Prev.Next = node.Next;
			} else {
				first = node.Next;
			}
			if (node.Next != null) {
				node.Next.Prev = node.Prev;
			} else {
				last = node.Prev;
			}
			node.Prev = null;
			node.Next = null;
		}

		private Node TreeSearch(ulong size)
		{
			return TreeSearch(treeRoot, size);
		}

		private Node TreeSearch(Node node, ulong size)
		{
			if (node == treeSentinel) {
				return null;
			}
			var cmp = size.CompareTo(node.Size);
			if (cmp == 0) {
				return node;
			}
			if (cmp > 0) {
				return TreeSearch(node.Right, size);
			}
			return TreeSearch(node.Left, size) ?? node;
		}

		private void TreeInsert(Node node)
		{
			Node parent = null;
			var current = treeRoot;
			var order = 0;
			while (current != treeSentinel) {
				order = node.Size.CompareTo(current.Size);
				if (order == 0) {
					order = node.Offset.CompareTo(current.Offset);
				}
				parent = current;
				if (order < 0) {
					current = current.Left;
				} else if (order > 0) {
					current = current.Right;
				} else {
					throw new InvalidOperationException();
				}
			}
			node.Parent = parent;
			node.Left = treeSentinel;
			node.Right = treeSentinel;
			node.Color = NodeColor.Red;
			if (parent != null) {
				if (order < 0) {
					parent.Left = node;
				} else {
					parent.Right = node;
				}
			} else {
				treeRoot = node;
			}
			TreeInsertFixup(node);
		}

		private void TreeInsertFixup(Node node)
		{
			while (node != treeRoot && node.Parent.Color == NodeColor.Red) {
				if (node.Parent == node.Parent.Parent.Left) {
					var uncle = node.Parent.Parent.Right;
					if (uncle.Color == NodeColor.Red) {
						node.Parent.Color = NodeColor.Black;
						uncle.Color = NodeColor.Black;
						node.Parent.Parent.Color = NodeColor.Red;
						node = node.Parent.Parent;
					} else {
						if (node == node.Parent.Right) {
							node = node.Parent;
							TreeRotateLeft(node);
						}
						node.Parent.Color = NodeColor.Black;
						node.Parent.Parent.Color = NodeColor.Red;
						TreeRotateRight(node.Parent.Parent);
					}
				} else {
					var uncle = node.Parent.Parent.Left;
					if (uncle.Color == NodeColor.Red) {
						node.Parent.Color = NodeColor.Black;
						uncle.Color = NodeColor.Black;
						node.Parent.Parent.Color = NodeColor.Red;
						node = node.Parent.Parent;
					} else {
						if (node == node.Parent.Left) {
							node = node.Parent;
							TreeRotateRight(node);
						}
						node.Parent.Color = NodeColor.Black;
						node.Parent.Parent.Color = NodeColor.Red;
						TreeRotateLeft(node.Parent.Parent);
					}
				}
			}
			treeRoot.Color = NodeColor.Black;
		}

		private void TreeRemove(Node node)
		{
			Node x, y;
			if (node.Left == treeSentinel || node.Right == treeSentinel) {
				y = node;
			} else {
				y = node.Right;
				while (y.Left != treeSentinel) {
					y = y.Left;
				}
			}
			if (y.Left != treeSentinel) {
				x = y.Left;
			} else {
				x = y.Right;
			}
			x.Parent = y.Parent;
			if (y.Parent != null) {
				if (y == y.Parent.Left) {
					y.Parent.Left = x;
				} else {
					y.Parent.Right = x;
				}
			} else {
				treeRoot = x;
			}
			if (y != node) {
				y.Left = node.Left;
				y.Right = node.Right;
				y.Parent = node.Parent;
				y.Color = node.Color;
				if (node.Left != null) {
					node.Left.Parent = y;
				}
				if (node.Right != null) {
					node.Right.Parent = y;
				}
				if (node.Parent != null) {
					if (node == node.Parent.Left) {
						node.Parent.Left = y;
					} else {
						node.Parent.Right = y;
					}
				} else {
					treeRoot = y;
				}
			}
			node.Left = null;
			node.Right = null;
			node.Parent = null;
			if (y.Color == NodeColor.Black) {
				TreeRemoveFixup(x);
			}
		}

		private void TreeRemoveFixup(Node node)
		{
			while (node != treeRoot && node.Color == NodeColor.Black) {
				if (node == node.Parent.Left) {
					var sibling = node.Parent.Right;
					if (sibling.Color == NodeColor.Red) {
						sibling.Color = NodeColor.Black;
						node.Parent.Color = NodeColor.Red;
						TreeRotateLeft(node.Parent);
						sibling = node.Parent.Right;
					}
					if (sibling.Left.Color == NodeColor.Black && sibling.Right.Color == NodeColor.Black) {
						sibling.Color = NodeColor.Red;
						node = node.Parent;
					} else {
						if (sibling.Right.Color == NodeColor.Black) {
							sibling.Left.Color = NodeColor.Black;
							sibling.Color = NodeColor.Red;
							TreeRotateRight(sibling);
							sibling = node.Parent.Right;
						}
						sibling.Color = node.Parent.Color;
						node.Parent.Color = NodeColor.Black;
						sibling.Right.Color = NodeColor.Black;
						TreeRotateLeft(node.Parent);
						node = treeRoot;
					}
				} else {
					var sibling = node.Parent.Left;
					if (sibling.Color == NodeColor.Red) {
						sibling.Color = NodeColor.Black;
						node.Parent.Color = NodeColor.Red;
						TreeRotateRight(node.Parent);
						sibling = node.Parent.Left;
					}
					if (sibling.Right.Color == NodeColor.Black && sibling.Left.Color == NodeColor.Black) {
						sibling.Color = NodeColor.Red;
						node = node.Parent;
					} else {
						if (sibling.Left.Color == NodeColor.Black) {
							sibling.Right.Color = NodeColor.Black;
							sibling.Color = NodeColor.Red;
							TreeRotateLeft(sibling);
							sibling = node.Parent.Left;
						}
						sibling.Color = node.Parent.Color;
						node.Parent.Color = NodeColor.Black;
						sibling.Left.Color = NodeColor.Black;
						TreeRotateRight(node.Parent);
						node = treeRoot;
					}
				}
			}
			node.Color = NodeColor.Black;
		}

		private void TreeRotateLeft(Node node)
		{
			var x = node.Right;
			node.Right = x.Left;
			if (x.Left != treeSentinel) {
				x.Left.Parent = node;
			}
			if (x != treeSentinel) {
				x.Parent = node.Parent;
			}
			if (node.Parent != null) {
				if (node == node.Parent.Left) {
					node.Parent.Left = x;
				} else {
					node.Parent.Right = x;
				}
			} else {
				treeRoot = x;
			}
			x.Left = node;
			if (node != treeSentinel) {
				node.Parent = x;
			}
		}

		private void TreeRotateRight(Node node)
		{
			var x = node.Left;
			node.Left = x.Right;
			if (x.Right != treeSentinel) {
				x.Right.Parent = node;
			}
			if (x != treeSentinel) {
				x.Parent = node.Parent;
			}
			if (node.Parent != null) {
				if (node == node.Parent.Right) {
					node.Parent.Right = x;
				} else {
					node.Parent.Left = x;
				}
			} else {
				treeRoot = x;
			}
			x.Right = node;
			if (node != treeSentinel) {
				node.Parent = x;
			}
		}

		public class Node
		{
			public Node Left;
			public Node Right;
			public Node Parent;
			public Node Prev;
			public Node Next;
			public NodeColor Color;
			public ulong Offset;
			public ulong Size;
		}

		public enum NodeColor
		{
			Red,
			Black
		}
	}

	internal class MemoryAlloc
	{
		public MemoryAllocator Allocator;
		public MemoryBlock MemoryBlock;
		public MemoryBlock.Node MemoryBlockNode;
		public DeviceMemory Memory;
		public ulong Offset;
		public ulong Size;

		public MemoryAlloc(MemoryAllocator allocator, DeviceMemory memory)
		{
			Allocator = allocator;
			Memory = memory;
			Size = memory.Size;
		}

		public MemoryAlloc(MemoryAllocator allocator, MemoryBlock memoryBlock, MemoryBlock.Node memoryBlockNode)
		{
			Allocator = allocator;
			MemoryBlock = memoryBlock;
			MemoryBlockNode = memoryBlockNode;
			Memory = memoryBlock.Memory;
			Offset = memoryBlockNode.Offset;
			Size = memoryBlockNode.Size;
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
