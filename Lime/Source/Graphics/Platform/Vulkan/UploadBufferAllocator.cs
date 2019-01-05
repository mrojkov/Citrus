using System;
using System.Collections.Generic;

namespace Lime.Graphics.Platform.Vulkan
{
	internal unsafe class UploadBufferAllocator : IDisposable
	{
		private PlatformRenderContext context;
		private SharpVulkan.Buffer buffer;
		private SharpVulkan.DeviceMemory memory;
		private ulong bufferSize;
		private ulong bufferOffset;
		private IntPtr mappedMemory;

		public UploadBufferAllocator(PlatformRenderContext context, ulong initialBufferSize)
		{
			this.context = context;
			CreateBuffer(initialBufferSize);
		}

		public void Dispose()
		{
			ReleaseBuffer();
		}

		public UploadBufferAlloc Allocate(ulong size, ulong alignment)
		{
			var alignedOffset = GraphicsUtility.AlignUp(bufferOffset, alignment);
			if (alignedOffset + size > bufferSize) {
				var newBufferSize = bufferSize;
				while (newBufferSize < size) {
					newBufferSize *= 2;
				}
				ReleaseBuffer();
				CreateBuffer(newBufferSize);
				alignedOffset = 0;
			}
			bufferOffset = alignedOffset + size;
			return new UploadBufferAlloc(buffer, new IntPtr((byte*)mappedMemory + alignedOffset), alignedOffset, size);
		}

		private void CreateBuffer(ulong size)
		{
			var createInfo = new SharpVulkan.BufferCreateInfo {
				StructureType = SharpVulkan.StructureType.BufferCreateInfo,
				Size = size,
				SharingMode = SharpVulkan.SharingMode.Exclusive,
				Usage = SharpVulkan.BufferUsageFlags.TransferSource
			};
			buffer = context.Device.CreateBuffer(ref createInfo);
			context.Device.GetBufferMemoryRequirements(buffer, out var memoryRequirements);
			memory = context.AllocateMemory(memoryRequirements,
				SharpVulkan.MemoryPropertyFlags.HostVisible | SharpVulkan.MemoryPropertyFlags.HostCoherent);
			context.Device.BindBufferMemory(buffer, memory, 0);
			mappedMemory = context.Device.MapMemory(memory, 0, size, SharpVulkan.MemoryMapFlags.None);
			bufferSize = size;
			bufferOffset = 0;
		}

		private void ReleaseBuffer()
		{
			if (buffer != SharpVulkan.Buffer.Null) {
				context.Device.UnmapMemory(memory);
				context.Release(buffer);
				context.Release(memory);
				buffer = SharpVulkan.Buffer.Null;
			}
		}
	}

	internal struct UploadBufferAlloc
	{
		public readonly SharpVulkan.Buffer Buffer;
		public readonly IntPtr Data;
		public readonly ulong BufferOffset;
		public readonly ulong Size;

		internal UploadBufferAlloc(SharpVulkan.Buffer buffer, IntPtr data, ulong bufferOffset, ulong size)
		{
			Buffer = buffer;
			Data = data;
			BufferOffset = bufferOffset;
			Size = size;
		}
	}
}
