using System;

namespace Lime.Graphics.Platform.Vulkan
{
	internal unsafe class PlatformTexture2D : IPlatformTexture2D
	{
		private PlatformRenderContext context;
		private Format format;
		private int width;
		private int height;
		private int levelCount;
		private SharpVulkan.Image image;
		private SharpVulkan.ImageView imageView;
		private MemoryAlloc memory;
		private SharpVulkan.Sampler sampler;

		internal SharpVulkan.Image Image => image;
		internal SharpVulkan.ImageView ImageView => imageView;
		internal SharpVulkan.Sampler Sampler => sampler;

		public PlatformRenderContext Context => context;
		public Format Format => format;
		public int Width => width;
		public int Height => height;
		public int LevelCount => levelCount;

		public PlatformTexture2D(PlatformRenderContext context, Format format, int width, int height, bool mipmaps, TextureParams textureParams)
			: this(context, format, width, height, mipmaps, false, textureParams)
		{
		}

		protected PlatformTexture2D(PlatformRenderContext context, Format format, int width, int height, bool mipmaps, bool renderTarget, TextureParams textureParams)
		{
			this.context = context;
			this.format = format;
			this.width = width;
			this.height = height;
			this.levelCount = mipmaps ? GraphicsUtility.CalculateMipLevelCount(width, height) : 1;
			Create(renderTarget);
			SetTextureParams(textureParams);
		}

		public virtual void Dispose()
		{
			if (image != SharpVulkan.Image.Null) {
				context.Release(imageView);
				context.Release(image);
				context.Release(memory);
				image = SharpVulkan.Image.Null;
			}
		}

		private void Create(bool renderTarget)
		{
			var vkFormat = VulkanHelper.GetVKFormat(format);
			var usage =
				SharpVulkan.ImageUsageFlags.TransferSource |
				SharpVulkan.ImageUsageFlags.TransferDestination |
				SharpVulkan.ImageUsageFlags.Sampled;
			if (renderTarget) {
				usage |= SharpVulkan.ImageUsageFlags.ColorAttachment;
			}
			var tiling = SharpVulkan.ImageTiling.Optimal;
			var imageCreateInfo = new SharpVulkan.ImageCreateInfo {
				StructureType = SharpVulkan.StructureType.ImageCreateInfo,
				ImageType = SharpVulkan.ImageType.Image2D,
				Usage = usage,
				Format = vkFormat,
				Extent = new SharpVulkan.Extent3D((uint)width, (uint)height, 1),
				MipLevels = (uint)levelCount,
				ArrayLayers = 1,
				Samples = SharpVulkan.SampleCountFlags.Sample1,
				SharingMode = SharpVulkan.SharingMode.Exclusive,
				InitialLayout = SharpVulkan.ImageLayout.Undefined,
				Tiling = tiling
			};
			image = context.Device.CreateImage(ref imageCreateInfo);
			memory = context.MemoryAllocator.Allocate(image, SharpVulkan.MemoryPropertyFlags.DeviceLocal, tiling);
			var viewCreateInfo = new SharpVulkan.ImageViewCreateInfo {
				StructureType = SharpVulkan.StructureType.ImageViewCreateInfo,
				ViewType = SharpVulkan.ImageViewType.Image2D,
				Image = image,
				Format = vkFormat,
				Components = SharpVulkan.ComponentMapping.Identity,
				SubresourceRange = new SharpVulkan.ImageSubresourceRange(SharpVulkan.ImageAspectFlags.Color)
			};
			imageView = context.Device.CreateImageView(ref viewCreateInfo);
			var memoryBarrier = new SharpVulkan.ImageMemoryBarrier {
				StructureType = SharpVulkan.StructureType.ImageMemoryBarrier,
				Image = image,
				OldLayout = SharpVulkan.ImageLayout.Undefined,
				NewLayout = SharpVulkan.ImageLayout.ShaderReadOnlyOptimal,
				SourceAccessMask = SharpVulkan.AccessFlags.None,
				DestinationAccessMask = SharpVulkan.AccessFlags.ShaderRead,
				SubresourceRange = new SharpVulkan.ImageSubresourceRange(SharpVulkan.ImageAspectFlags.Color)
			};
			context.EndRenderPass();
			context.EnsureCommandBuffer();
			context.CommandBuffer.PipelineBarrier(
				SharpVulkan.PipelineStageFlags.TopOfPipe, SharpVulkan.PipelineStageFlags.VertexShader | SharpVulkan.PipelineStageFlags.FragmentShader,
				SharpVulkan.DependencyFlags.None, 0, null, 0, null, 1, &memoryBarrier);
		}

		public void SetData(int level, int x, int y, int width, int height, IntPtr data)
		{
			ulong bufferOffsetAlignment = 4;
			bufferOffsetAlignment = GraphicsUtility.CombineAlignment(bufferOffsetAlignment, (ulong)Format.GetSize());
			bufferOffsetAlignment = GraphicsUtility.CombineAlignment(bufferOffsetAlignment, context.PhysicalDeviceLimits.OptimalBufferCopyOffsetAlignment);
			var dataSize = GraphicsUtility.CalculateImageDataSize(Format, width, height);
			var uploadBufferAlloc = context.AllocateUploadBuffer((ulong)dataSize, bufferOffsetAlignment);
			GraphicsUtility.CopyMemory(uploadBufferAlloc.Data, data, dataSize);
			context.EndRenderPass();
			context.EnsureCommandBuffer();
			var preMemoryBarrier = new SharpVulkan.ImageMemoryBarrier {
				StructureType = SharpVulkan.StructureType.ImageMemoryBarrier,
				Image = image,
				OldLayout = SharpVulkan.ImageLayout.ShaderReadOnlyOptimal,
				NewLayout = SharpVulkan.ImageLayout.TransferDestinationOptimal,
				SourceAccessMask = SharpVulkan.AccessFlags.ShaderRead,
				DestinationAccessMask = SharpVulkan.AccessFlags.TransferWrite,
				SubresourceRange = new SharpVulkan.ImageSubresourceRange(SharpVulkan.ImageAspectFlags.Color, 0, 1, (uint)level, 1)
			};
			context.CommandBuffer.PipelineBarrier(
				SharpVulkan.PipelineStageFlags.VertexShader | SharpVulkan.PipelineStageFlags.FragmentShader,
				SharpVulkan.PipelineStageFlags.Transfer, SharpVulkan.DependencyFlags.None,
				0, null, 0, null, 1, &preMemoryBarrier);
			var copyRegion = new SharpVulkan.BufferImageCopy {
				BufferOffset = uploadBufferAlloc.BufferOffset,
				BufferRowLength = (uint)width,
				BufferImageHeight = (uint)height,
				ImageOffset = new SharpVulkan.Offset3D(x, y, 0),
				ImageExtent = new SharpVulkan.Extent3D((uint)width, (uint)height, 1),
				ImageSubresource = new SharpVulkan.ImageSubresourceLayers(SharpVulkan.ImageAspectFlags.Color, 0, 1, (uint)level)
			};
			context.CommandBuffer.CopyBufferToImage(
				uploadBufferAlloc.Buffer, image, SharpVulkan.ImageLayout.TransferDestinationOptimal, 1, &copyRegion);
			var postMemoryBarrier = new SharpVulkan.ImageMemoryBarrier {
				StructureType = SharpVulkan.StructureType.ImageMemoryBarrier,
				Image = image,
				OldLayout = SharpVulkan.ImageLayout.TransferDestinationOptimal,
				NewLayout = SharpVulkan.ImageLayout.ShaderReadOnlyOptimal,
				SourceAccessMask = SharpVulkan.AccessFlags.TransferWrite,
				DestinationAccessMask = SharpVulkan.AccessFlags.ShaderRead,
				SubresourceRange = new SharpVulkan.ImageSubresourceRange(SharpVulkan.ImageAspectFlags.Color, 0, 1, (uint)level, 1)
			};
			context.CommandBuffer.PipelineBarrier(
				SharpVulkan.PipelineStageFlags.Transfer,
				SharpVulkan.PipelineStageFlags.VertexShader | SharpVulkan.PipelineStageFlags.FragmentShader,
				SharpVulkan.DependencyFlags.None, 0, null, 0, null, 1, &postMemoryBarrier);
		}

		public void SetTextureParams(TextureParams textureParams)
		{
			sampler = context.SamplerCache.AcquireSampler(textureParams);
		}

		internal void GetData(Format dstFormat, int level, int x, int y, int width, int height, IntPtr data)
		{
			// TODO: Are the memory barriers corrent?
			ulong bufferOffsetAlignment = 4;
			bufferOffsetAlignment = GraphicsUtility.CombineAlignment(bufferOffsetAlignment, (ulong)Format.GetSize());
			bufferOffsetAlignment = GraphicsUtility.CombineAlignment(bufferOffsetAlignment, context.PhysicalDeviceLimits.OptimalBufferCopyOffsetAlignment);
			var dataSize = GraphicsUtility.CalculateImageDataSize(Format, width, height);
			// FIXME: Implement read-back buffer allocator
			var readbackBufferAlloc = context.AllocateUploadBuffer((ulong)dataSize, bufferOffsetAlignment);
			context.EndRenderPass();
			context.EnsureCommandBuffer();
			var preMemoryBarrier = new SharpVulkan.ImageMemoryBarrier {
				StructureType = SharpVulkan.StructureType.ImageMemoryBarrier,
				Image = image,
				OldLayout = SharpVulkan.ImageLayout.ShaderReadOnlyOptimal,
				NewLayout = SharpVulkan.ImageLayout.TransferSourceOptimal,
				SourceAccessMask = SharpVulkan.AccessFlags.ShaderRead,
				DestinationAccessMask = SharpVulkan.AccessFlags.TransferRead,
				SubresourceRange = new SharpVulkan.ImageSubresourceRange(SharpVulkan.ImageAspectFlags.Color, 0, 1, (uint)level, 1)
			};
			context.CommandBuffer.PipelineBarrier(
				SharpVulkan.PipelineStageFlags.VertexShader | SharpVulkan.PipelineStageFlags.FragmentShader,
				SharpVulkan.PipelineStageFlags.Transfer, SharpVulkan.DependencyFlags.None,
				0, null, 0, null, 1, &preMemoryBarrier);
			var copyRegion = new SharpVulkan.BufferImageCopy {
				BufferOffset = readbackBufferAlloc.BufferOffset,
				BufferRowLength = (uint)width,
				BufferImageHeight = (uint)height,
				ImageOffset = new SharpVulkan.Offset3D(x, y, 0),
				ImageExtent = new SharpVulkan.Extent3D((uint)width, (uint)height, 1),
				ImageSubresource = new SharpVulkan.ImageSubresourceLayers(SharpVulkan.ImageAspectFlags.Color, 0, 1, (uint)level)
			};
			context.CommandBuffer.CopyImageToBuffer(
				image, SharpVulkan.ImageLayout.TransferSourceOptimal, readbackBufferAlloc.Buffer, 1, &copyRegion);
			var postMemoryBarrier = new SharpVulkan.ImageMemoryBarrier {
				StructureType = SharpVulkan.StructureType.ImageMemoryBarrier,
				Image = image,
				OldLayout = SharpVulkan.ImageLayout.TransferSourceOptimal,
				NewLayout = SharpVulkan.ImageLayout.ShaderReadOnlyOptimal,
				SourceAccessMask = SharpVulkan.AccessFlags.TransferRead,
				DestinationAccessMask = SharpVulkan.AccessFlags.ShaderRead,
				SubresourceRange = new SharpVulkan.ImageSubresourceRange(SharpVulkan.ImageAspectFlags.Color, 0, 1, (uint)level, 1)
			};
			context.CommandBuffer.PipelineBarrier(
				SharpVulkan.PipelineStageFlags.Transfer,
				SharpVulkan.PipelineStageFlags.VertexShader | SharpVulkan.PipelineStageFlags.FragmentShader,
				SharpVulkan.DependencyFlags.None, 0, null, 0, null, 1, &postMemoryBarrier);
			context.Finish();
			if (dstFormat == Format) {
				GraphicsUtility.CopyMemory(data, readbackBufferAlloc.Data, Format.GetSize() * width * height);
			} else {
				var decoder = FormatConverter.GetDecoder(Format);
				var encoder = FormatConverter.GetEncoder(dstFormat);
				var srcTexelSize = Format.GetSize();
				var dstTexelSize = dstFormat.GetSize();
				var srcData = readbackBufferAlloc.Data;
				var dstData = data;
				var texelCount = width * height;
				while (texelCount-- > 0) {
					encoder(dstData, decoder(srcData));
					srcData += srcTexelSize;
					dstData += dstTexelSize;
				}
			}
		}
	}
}
