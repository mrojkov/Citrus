using System;
using System.Collections.Generic;

namespace Lime.Graphics.Platform.Vulkan
{
	internal unsafe class BackingBuffer : IDisposable
	{
		private PlatformRenderContext context;
		private SharpVulkan.Buffer buffer;
		private MemoryAlloc memory;
		private SharpVulkan.BufferUsageFlags usage;
		private SharpVulkan.MemoryPropertyFlags memoryPropertyFlags;
		private ulong sliceAlignment;
		private ulong sliceSize;
		private ulong sliceCount;
		private ulong sliceOffset;
		private Queue<SliceEntry> sliceQueue = new Queue<SliceEntry>();

		internal SharpVulkan.Buffer Buffer => buffer;
		internal ulong SliceOffset => sliceOffset;
		internal ulong SliceSize => sliceSize;

		public BackingBuffer(PlatformRenderContext context, SharpVulkan.BufferUsageFlags usage, SharpVulkan.MemoryPropertyFlags memoryPropertyFlags, ulong size)
		{
			this.context = context;
			this.usage = usage;
			this.memoryPropertyFlags = memoryPropertyFlags;
			sliceSize = size;
			sliceAlignment = GetAlignmentForBufferUsage(context, usage);
			sliceCount = 1;
			CreateBuffer();
			sliceOffset = sliceQueue.Dequeue().Offset;
		}

		public void Dispose()
		{
			ReleaseBuffer();
		}

		private static ulong GetAlignmentForBufferUsage(PlatformRenderContext ctx, SharpVulkan.BufferUsageFlags usage)
		{
			ulong alignment = 1;
			if ((usage & SharpVulkan.BufferUsageFlags.UniformBuffer) != 0) {
				alignment = GraphicsUtility.CombineAlignment(alignment, ctx.PhysicalDeviceLimits.MinUniformBufferOffsetAlignment);
			}
			return alignment;
		}

		public void DiscardSlice(ulong fenceValue)
		{
			sliceQueue.Enqueue(new SliceEntry {
				Offset = sliceOffset,
				FenceValue = fenceValue
			});
			if (!context.IsFenceCompleted(sliceQueue.Peek().FenceValue)) {
				sliceCount *= 2;
				ReleaseBuffer();
				CreateBuffer();
			}
			sliceOffset = sliceQueue.Dequeue().Offset;
		}

		public IntPtr MapSlice()
		{
			return MapSlice(0, sliceSize);
		}

		public IntPtr MapSlice(ulong offset, ulong size)
		{
			return context.MemoryAllocator.Map(memory, sliceOffset + offset, size);
		}

		public void UnmapSlice()
		{
			context.MemoryAllocator.Unmap(memory);
		}

		private void ReleaseBuffer()
		{
			if (buffer != SharpVulkan.Buffer.Null) {
				context.Release(buffer);
				context.Release(memory);
				buffer = SharpVulkan.Buffer.Null;
			}
		}

		private void CreateBuffer()
		{
			var alignedSliceSize = GraphicsUtility.AlignUp(sliceSize, sliceAlignment);
			var bufferSize = sliceCount * alignedSliceSize;
			var createInfo = new SharpVulkan.BufferCreateInfo {
				StructureType = SharpVulkan.StructureType.BufferCreateInfo,
				Size = bufferSize,
				SharingMode = SharpVulkan.SharingMode.Exclusive,
				Usage = usage
			};
			buffer = context.Device.CreateBuffer(ref createInfo);
			memory = context.MemoryAllocator.Allocate(buffer, memoryPropertyFlags);
			sliceQueue.Clear();
			for (ulong i = 0; i < sliceCount; i++) {
				sliceQueue.Enqueue(new SliceEntry {
					Offset = i * alignedSliceSize
				});
			}
		}

		private struct SliceEntry
		{
			public ulong FenceValue;
			public ulong Offset;
		}
	}
}
