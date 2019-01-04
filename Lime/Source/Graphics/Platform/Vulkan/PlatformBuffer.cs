using System;
using System.Collections.Generic;

namespace Lime.Graphics.Platform.Vulkan
{
	internal unsafe class PlatformBuffer : IPlatformBuffer
	{
		private PlatformRenderContext context;
		private SharpVulkan.AccessFlags accessMask;
		private SharpVulkan.PipelineStageFlags pipelineStageMask;
		private BackingBuffer backingBuffer;
		private BufferType bufferType;
		private int size;
		private bool dynamic;

		internal ulong ReaderFenceValue;
		internal BackingBuffer BackingBuffer => backingBuffer;

		public BufferType BufferType => bufferType;
		public int Size => size;
		public bool Dynamic => dynamic;

		public PlatformBuffer(PlatformRenderContext context, BufferType bufferType, int size, bool dynamic)
		{
			this.context = context;
			this.bufferType = bufferType;
			this.size = size;
			this.dynamic = dynamic;
			Create();
		}

		public void Dispose()
		{
			if (backingBuffer != null) {
				backingBuffer.Dispose();
				backingBuffer = null;
			}
		}

		private void Create()
		{
			var usage = SharpVulkan.BufferUsageFlags.None;
			switch (bufferType) {
				case BufferType.Vertex:
					usage = SharpVulkan.BufferUsageFlags.VertexBuffer;
					accessMask = SharpVulkan.AccessFlags.VertexAttributeRead;
					pipelineStageMask = SharpVulkan.PipelineStageFlags.VertexInput;
					break;
				case BufferType.Index:
					usage = SharpVulkan.BufferUsageFlags.IndexBuffer;
					accessMask = SharpVulkan.AccessFlags.IndexRead;
					pipelineStageMask = SharpVulkan.PipelineStageFlags.VertexInput;
					break;
				default:
					throw new InvalidOperationException();
			}
			var memoryPropertyFlags = dynamic
				? SharpVulkan.MemoryPropertyFlags.HostVisible | SharpVulkan.MemoryPropertyFlags.HostCoherent
				: SharpVulkan.MemoryPropertyFlags.DeviceLocal;
			backingBuffer = new BackingBuffer(context, usage, memoryPropertyFlags, (ulong)size);
		}

		public void SetData(int offset, IntPtr data, int size, BufferSetDataMode mode)
		{
			if (dynamic) {
				SetDataDynamic(offset, data, size, mode);
			} else {
				SetDataStatic(offset, data, size);
			}
		}

		private void SetDataDynamic(int offset, IntPtr data, int size, BufferSetDataMode mode)
		{
			if (mode == BufferSetDataMode.Discard) {
				backingBuffer.DiscardSlice(ReaderFenceValue);
				ReaderFenceValue = 0;
			} else {
				if (context.NextFenceValue <= ReaderFenceValue) {
					context.Flush();
				}
				context.WaitForFence(ReaderFenceValue);
			}
			var dstData = backingBuffer.MapSlice((ulong)offset, (ulong)size);
			try {
				GraphicsUtility.CopyMemory(dstData, data, size);
			} finally {
				backingBuffer.UnmapSlice();
			}
		}

		private void SetDataStatic(int offset, IntPtr data, int size)
		{
			var uploadBufferAlloc = context.AllocateUploadBuffer((ulong)size, 1);
			GraphicsUtility.CopyMemory(uploadBufferAlloc.Data, data, size);
			context.EndRenderPass();
			context.EnsureCommandBuffer();
			context.CommandBuffer.PipelineBarrier(
				pipelineStageMask, SharpVulkan.PipelineStageFlags.Transfer,
				SharpVulkan.DependencyFlags.None, 0, null, 0, null, 0, null);
			var copyRegion = new SharpVulkan.BufferCopy {
				SourceOffset = uploadBufferAlloc.BufferOffset,
				DestinationOffset = (ulong)offset,
				Size = (ulong)size
			};
			context.CommandBuffer.CopyBuffer(uploadBufferAlloc.Buffer, backingBuffer.Buffer, 1, &copyRegion);
			var postMemoryBarrier = new SharpVulkan.MemoryBarrier {
				StructureType = SharpVulkan.StructureType.MemoryBarrier,
				SourceAccessMask = SharpVulkan.AccessFlags.TransferWrite,
				DestinationAccessMask = accessMask
			};
			context.CommandBuffer.PipelineBarrier(
				SharpVulkan.PipelineStageFlags.Transfer, pipelineStageMask,
				SharpVulkan.DependencyFlags.None, 1, &postMemoryBarrier, 0, null, 0, null);
		}
	}
}
