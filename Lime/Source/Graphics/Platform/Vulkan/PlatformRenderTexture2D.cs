using System;
using System.Linq;

namespace Lime.Graphics.Platform.Vulkan
{
	internal unsafe class PlatformRenderTexture2D : PlatformTexture2D, IPlatformRenderTexture2D
	{
		private SharpVulkan.Format colorFormat;
		private SharpVulkan.Image depthStencilBuffer;
		private SharpVulkan.ImageView depthStencilView;
		private MemoryAlloc depthStencilMemory;
		private SharpVulkan.Format depthStencilFormat;
		private SharpVulkan.RenderPass renderPass;
		private SharpVulkan.Framebuffer framebuffer;

		internal SharpVulkan.Format ColorFormat => colorFormat;
		internal SharpVulkan.Format DepthStencilFormat => depthStencilFormat;
		internal SharpVulkan.Framebuffer Framebuffer => framebuffer;
		internal SharpVulkan.RenderPass RenderPass => renderPass;

		public PlatformRenderTexture2D(PlatformRenderContext context, Format format, int width, int height, TextureParams textureParams)
			: base(context, format, width, height, false, true, textureParams)
		{
			colorFormat = VulkanHelper.GetVKFormat(format);
			CreateDepthStencilBuffer();
			CreateRenderPass();
			CreateFramebuffer();
		}

		public override void Dispose()
		{
			if (framebuffer != SharpVulkan.Framebuffer.Null) {
				Context.Release(framebuffer);
				Context.Release(renderPass);
				Context.Release(depthStencilView);
				Context.Release(depthStencilBuffer);
				Context.Release(depthStencilMemory);
				framebuffer = SharpVulkan.Framebuffer.Null;
			}
			base.Dispose();
		}

		private void CreateDepthStencilBuffer()
		{
			var formats = new[] {
				SharpVulkan.Format.D32SFloatS8UInt,
				SharpVulkan.Format.D24UNormS8UInt,
				SharpVulkan.Format.D16UNormS8UInt
			};
			depthStencilFormat = formats.First(format => {
				Context.PhysicalDevice.GetFormatProperties(format, out var formatProperties);
				return (formatProperties.OptimalTilingFeatures & SharpVulkan.FormatFeatureFlags.DepthStencilAttachment) != 0;
			});
			var createInfo = new SharpVulkan.ImageCreateInfo {
				StructureType = SharpVulkan.StructureType.ImageCreateInfo,
				ImageType = SharpVulkan.ImageType.Image2D,
				Usage = SharpVulkan.ImageUsageFlags.DepthStencilAttachment,
				Format = depthStencilFormat,
				Extent = new SharpVulkan.Extent3D((uint)Width, (uint)Height, 1),
				MipLevels = 1,
				ArrayLayers = 1,
				Samples = SharpVulkan.SampleCountFlags.Sample1,
				SharingMode = SharpVulkan.SharingMode.Exclusive,
				Tiling = SharpVulkan.ImageTiling.Optimal,
				InitialLayout = SharpVulkan.ImageLayout.Undefined
			};
			depthStencilBuffer = Context.Device.CreateImage(ref createInfo);
			Context.Device.GetImageMemoryRequirements(depthStencilBuffer, out var memoryRequirements);
			depthStencilMemory = Context.MemoryAllocator.Allocate(memoryRequirements, SharpVulkan.MemoryPropertyFlags.DeviceLocal, false);
			Context.Device.BindImageMemory(depthStencilBuffer, depthStencilMemory.Memory, depthStencilMemory.Offset);
			var viewCreateInfo = new SharpVulkan.ImageViewCreateInfo {
				StructureType = SharpVulkan.StructureType.ImageViewCreateInfo,
				ViewType = SharpVulkan.ImageViewType.Image2D,
				Image = depthStencilBuffer,
				Format = depthStencilFormat,
				Components = SharpVulkan.ComponentMapping.Identity,
				SubresourceRange = new SharpVulkan.ImageSubresourceRange(
					SharpVulkan.ImageAspectFlags.Depth |
					SharpVulkan.ImageAspectFlags.Stencil)
			};
			depthStencilView = Context.Device.CreateImageView(ref viewCreateInfo);
			var memoryBarrier = new SharpVulkan.ImageMemoryBarrier {
				StructureType = SharpVulkan.StructureType.ImageMemoryBarrier,
				Image = depthStencilBuffer,
				OldLayout = SharpVulkan.ImageLayout.Undefined,
				NewLayout = SharpVulkan.ImageLayout.DepthStencilAttachmentOptimal,
				SourceAccessMask = SharpVulkan.AccessFlags.None,
				DestinationAccessMask = SharpVulkan.AccessFlags.DepthStencilAttachmentRead | SharpVulkan.AccessFlags.DepthStencilAttachmentWrite,
				SubresourceRange = new SharpVulkan.ImageSubresourceRange(
					SharpVulkan.ImageAspectFlags.Depth |
					SharpVulkan.ImageAspectFlags.Stencil)
			};
			Context.EndRenderPass();
			Context.EnsureCommandBuffer();
			Context.CommandBuffer.PipelineBarrier(SharpVulkan.PipelineStageFlags.TopOfPipe,
				SharpVulkan.PipelineStageFlags.EarlyFragmentTests | SharpVulkan.PipelineStageFlags.LateFragmentTests,
				SharpVulkan.DependencyFlags.None, 0, null, 0, null, 1, &memoryBarrier);
		}

		private void CreateRenderPass()
		{
			var attachmentDescs = new[] {
				new SharpVulkan.AttachmentDescription {
					Format = colorFormat,
					Samples = SharpVulkan.SampleCountFlags.Sample1,
					LoadOperation = SharpVulkan.AttachmentLoadOperation.Load,
					StoreOperation = SharpVulkan.AttachmentStoreOperation.Store,
					InitialLayout = SharpVulkan.ImageLayout.ShaderReadOnlyOptimal,
					FinalLayout = SharpVulkan.ImageLayout.ShaderReadOnlyOptimal
				},
				new SharpVulkan.AttachmentDescription {
					Format = depthStencilFormat,
					Samples = SharpVulkan.SampleCountFlags.Sample1,
					LoadOperation = SharpVulkan.AttachmentLoadOperation.Load,
					StoreOperation = SharpVulkan.AttachmentStoreOperation.Store,
					StencilLoadOperation = SharpVulkan.AttachmentLoadOperation.Load,
					StencilStoreOperation = SharpVulkan.AttachmentStoreOperation.Store,
					InitialLayout = SharpVulkan.ImageLayout.DepthStencilAttachmentOptimal,
					FinalLayout = SharpVulkan.ImageLayout.DepthStencilAttachmentOptimal
				}
			};
			var colorAttachmentRef = new SharpVulkan.AttachmentReference {
				Attachment = 0,
				Layout = SharpVulkan.ImageLayout.ColorAttachmentOptimal
			};
			var depthStencilAttachmentRef = new SharpVulkan.AttachmentReference {
				Attachment = 1,
				Layout = SharpVulkan.ImageLayout.DepthStencilAttachmentOptimal
			};
			fixed (SharpVulkan.AttachmentDescription* attachmentDescsPtr = attachmentDescs) {
				var subpass = new SharpVulkan.SubpassDescription {
					PipelineBindPoint = SharpVulkan.PipelineBindPoint.Graphics,
					ColorAttachmentCount = 1,
					ColorAttachments = new IntPtr(&colorAttachmentRef),
					DepthStencilAttachment = new IntPtr(&depthStencilAttachmentRef)
				};
				var createInfo = new SharpVulkan.RenderPassCreateInfo {
					StructureType = SharpVulkan.StructureType.RenderPassCreateInfo,
					AttachmentCount = (uint)attachmentDescs.Length,
					Attachments = new IntPtr(attachmentDescsPtr),
					SubpassCount = 1,
					Subpasses = new IntPtr(&subpass)
				};
				renderPass = Context.Device.CreateRenderPass(ref createInfo);
			}
		}

		private void CreateFramebuffer()
		{
			var attachments = new[] { ImageView, depthStencilView };
			fixed (SharpVulkan.ImageView* attachmentsPtr = attachments) {
				var createInfo = new SharpVulkan.FramebufferCreateInfo {
					StructureType = SharpVulkan.StructureType.FramebufferCreateInfo,
					AttachmentCount = (uint)attachments.Length,
					Attachments = new IntPtr(attachmentsPtr),
					Width = (uint)Width,
					Height = (uint)Height,
					Layers = 1,
					RenderPass = renderPass
				};
				framebuffer = Context.Device.CreateFramebuffer(ref createInfo);
			}
		}
	}
}
