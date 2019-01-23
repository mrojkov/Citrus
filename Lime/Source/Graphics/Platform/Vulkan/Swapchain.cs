using System;
using System.Linq;
using System.Diagnostics;

namespace Lime.Graphics.Platform.Vulkan
{
	public unsafe class Swapchain : IDisposable
	{
		private PlatformRenderContext context;
		private IntPtr windowHandle;
		private SharpVulkan.Surface surface;
		private SharpVulkan.Swapchain swapchain;
		private SharpVulkan.Format backbufferFormat;
		private SharpVulkan.Image[] backbuffers;
		private SharpVulkan.ImageView[] backbufferViews;
		private SharpVulkan.Image depthStencilBuffer;
		private SharpVulkan.ImageView depthStencilView;
		private MemoryAlloc depthStencilMemory;
		private SharpVulkan.Format depthStencilFormat;
		private SharpVulkan.RenderPass renderPass;
		private SharpVulkan.Framebuffer[] framebuffers;
		private uint backbufferIndex;
		private SharpVulkan.Semaphore[] acquirementSemaphores;
		private SharpVulkan.Semaphore acquirementSemaphore;
		private int nextAcquirementSemaphoreIndex;
		private int width;
		private int height;
		private ulong swapchainDestroyFenceValue;

		internal SharpVulkan.Format BackbufferFormat => backbufferFormat;
		internal SharpVulkan.Format DepthStencilFormat => depthStencilFormat;
		internal SharpVulkan.Image Backbuffer => backbuffers[backbufferIndex];
		internal SharpVulkan.Framebuffer Framebuffer => framebuffers[backbufferIndex];
		internal SharpVulkan.RenderPass RenderPass => renderPass;

		public int Width => width;
		public int Height => height;

		public Swapchain(PlatformRenderContext context, IntPtr windowHandle, int width, int height)
		{
			this.context = context;
			this.windowHandle = windowHandle;
			this.width = width;
			this.height = height;
			CreateSurface();
			CreateSwapchain();
		}

		public void Dispose()
		{
			DestroySwapchain();
			DestroySurface();
		}

		private void DestroySwapchain()
		{
			if (swapchain != SharpVulkan.Swapchain.Null) {
				context.WaitForFence(swapchainDestroyFenceValue);
				swapchainDestroyFenceValue = context.NextFenceValue;
				foreach (var i in framebuffers) {
					context.Release(i);
				}
				foreach (var i in backbufferViews) {
					context.Release(i);
				}
				foreach (var i in acquirementSemaphores) {
					context.Release(i);
				}
				context.Release(depthStencilView);
				context.Release(depthStencilBuffer);
				context.Release(depthStencilMemory);
				context.Release(renderPass);
				context.Release(swapchain);
				context.Flush();
				swapchain = SharpVulkan.Swapchain.Null;
			}
		}

		private void DestroySurface()
		{
			if (surface != SharpVulkan.Surface.Null) {
				context.Finish();
				context.Instance.DestroySurface(surface);
				surface = SharpVulkan.Surface.Null;
			}
		}

		internal void Resize(int width, int height)
		{
			if (this.width == width && this.height == height) {
				return;
			}
			this.width = width;
			this.height = height;
			CreateSwapchain();
		}

		private void CreateSurface()
		{
			var createInfo = new SharpVulkan.Win32SurfaceCreateInfo {
				StructureType = SharpVulkan.StructureType.Win32SurfaceCreateInfo,
				InstanceHandle = Process.GetCurrentProcess().Handle,
				WindowHandle = windowHandle
			};
			surface = context.Instance.CreateWin32Surface(createInfo);
			if (!context.PhysicalDevice.GetSurfaceSupport(context.QueueFamilyIndex, surface)) {
				throw new NotSupportedException();
			}
		}

		private void CreateSwapchain()
		{
			var surfaceFormats = context.PhysicalDevice.GetSurfaceFormats(surface);
			if (surfaceFormats.Length == 1 && surfaceFormats[0].Format == SharpVulkan.Format.Undefined) {
				backbufferFormat = SharpVulkan.Format.B8G8R8A8UNorm;
			} else {
				backbufferFormat = surfaceFormats[0].Format;
			}
			context.PhysicalDevice.GetSurfaceCapabilities(surface, out var surfaceCapabilities);
			if (width < surfaceCapabilities.MinImageExtent.Width || height < surfaceCapabilities.MinImageExtent.Height ||
				width > surfaceCapabilities.MaxImageExtent.Width || height > surfaceCapabilities.MaxImageExtent.Height
			) {
				throw new InvalidOperationException();
			}
			var desiredBufferCount = 2U;
			if (desiredBufferCount < surfaceCapabilities.MinImageCount) {
				desiredBufferCount = surfaceCapabilities.MinImageCount;
			} else if (surfaceCapabilities.MaxImageCount > 0 && desiredBufferCount > surfaceCapabilities.MaxImageCount) {
				desiredBufferCount = surfaceCapabilities.MaxImageCount;
			}
			var preTransform = SharpVulkan.SurfaceTransformFlags.Identity;
			if ((surfaceCapabilities.SupportedTransforms & preTransform) == 0) {
				throw new NotSupportedException();
			}
			var compositeAlpha = SharpVulkan.CompositeAlphaFlags.Opaque;
			if ((surfaceCapabilities.SupportedCompositeAlpha & compositeAlpha) == 0) {
				throw new NotSupportedException();
			}
			var backbufferUsage = SharpVulkan.ImageUsageFlags.ColorAttachment;
			if ((surfaceCapabilities.SupportedUsageFlags & backbufferUsage) == 0) {
				throw new NotSupportedException();
			}
			var oldSwapchain = swapchain;
			DestroySwapchain();
			var createInfo = new SharpVulkan.SwapchainCreateInfo {
				StructureType = SharpVulkan.StructureType.SwapchainCreateInfo,
				Surface = surface,
				PresentMode = SharpVulkan.PresentMode.Fifo,
				PreTransform = preTransform,
				CompositeAlpha = compositeAlpha,
				ImageUsage = backbufferUsage,
				ImageFormat = backbufferFormat,
				ImageColorSpace = SharpVulkan.ColorSpace.SRgbNonlinear,
				ImageExtent = new SharpVulkan.Extent2D((uint)width, (uint)height),
				ImageArrayLayers = 1,
				ImageSharingMode = SharpVulkan.SharingMode.Exclusive,
				MinImageCount = desiredBufferCount,
				Clipped = true,
				OldSwapchain = oldSwapchain
			};
			swapchain = context.Device.CreateSwapchain(ref createInfo);
			CreateBackbuffers();
			CreateDepthStencilBuffer();
			CreateRenderPass();
			CreateFramebuffers();
			AcquireNextImage();
		}

		private void CreateBackbuffers()
		{
			backbuffers = context.Device.GetSwapchainImages(swapchain);
			backbufferViews = new SharpVulkan.ImageView[backbuffers.Length];
			for (var i = 0; i < backbuffers.Length; i++) {
				var viewCreateInfo = new SharpVulkan.ImageViewCreateInfo {
					StructureType = SharpVulkan.StructureType.ImageViewCreateInfo,
					ViewType = SharpVulkan.ImageViewType.Image2D,
					Image = backbuffers[i],
					Format = backbufferFormat,
					Components = SharpVulkan.ComponentMapping.Identity,
					SubresourceRange = new SharpVulkan.ImageSubresourceRange(SharpVulkan.ImageAspectFlags.Color)
				};
				backbufferViews[i] = context.Device.CreateImageView(ref viewCreateInfo);
			}
			acquirementSemaphores = new SharpVulkan.Semaphore[backbuffers.Length + 1];
			for (var i = 0; i < backbuffers.Length + 1; i++) {
				var semaphoreCreateInfo = new SharpVulkan.SemaphoreCreateInfo {
					StructureType = SharpVulkan.StructureType.SemaphoreCreateInfo
				};
				acquirementSemaphores[i] = context.Device.CreateSemaphore(ref semaphoreCreateInfo);
			}
		}

		private void CreateDepthStencilBuffer()
		{
			var formats = new[] {
				SharpVulkan.Format.D32SFloatS8UInt,
				SharpVulkan.Format.D24UNormS8UInt,
				SharpVulkan.Format.D16UNormS8UInt
			};
			depthStencilFormat = formats.First(format => {
				context.PhysicalDevice.GetFormatProperties(format, out var formatProperties);
				return (formatProperties.OptimalTilingFeatures & SharpVulkan.FormatFeatureFlags.DepthStencilAttachment) != 0;
			});
			var tiling = SharpVulkan.ImageTiling.Optimal;
			var createInfo = new SharpVulkan.ImageCreateInfo {
				StructureType = SharpVulkan.StructureType.ImageCreateInfo,
				ImageType = SharpVulkan.ImageType.Image2D,
				Usage = SharpVulkan.ImageUsageFlags.DepthStencilAttachment,
				Format = depthStencilFormat,
				Extent = new SharpVulkan.Extent3D((uint)width, (uint)height, 1),
				MipLevels = 1,
				ArrayLayers = 1,
				Samples = SharpVulkan.SampleCountFlags.Sample1,
				SharingMode = SharpVulkan.SharingMode.Exclusive,
				Tiling = tiling,
				InitialLayout = SharpVulkan.ImageLayout.Undefined
			};
			depthStencilBuffer = context.Device.CreateImage(ref createInfo);
			depthStencilMemory = context.MemoryAllocator.Allocate(depthStencilBuffer, SharpVulkan.MemoryPropertyFlags.DeviceLocal, tiling);
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
			depthStencilView = context.Device.CreateImageView(ref viewCreateInfo);
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
			context.EndRenderPass();
			context.EnsureCommandBuffer();
			context.CommandBuffer.PipelineBarrier(SharpVulkan.PipelineStageFlags.TopOfPipe,
				SharpVulkan.PipelineStageFlags.EarlyFragmentTests | SharpVulkan.PipelineStageFlags.LateFragmentTests,
				SharpVulkan.DependencyFlags.None, 0, null, 0, null, 1, &memoryBarrier);
		}

		private void CreateRenderPass()
		{
			var attachmentDescs = new[] {
				new SharpVulkan.AttachmentDescription {
					Format = backbufferFormat,
					Samples = SharpVulkan.SampleCountFlags.Sample1,
					LoadOperation = SharpVulkan.AttachmentLoadOperation.Load,
					StoreOperation = SharpVulkan.AttachmentStoreOperation.Store,
					InitialLayout = SharpVulkan.ImageLayout.ColorAttachmentOptimal,
					FinalLayout = SharpVulkan.ImageLayout.ColorAttachmentOptimal
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
				renderPass = context.Device.CreateRenderPass(ref createInfo);
			}
		}

		private void CreateFramebuffers()
		{
			framebuffers = new SharpVulkan.Framebuffer[backbuffers.Length];
			for (var i = 0; i < backbuffers.Length; i++) {
				var attachments = new[] { backbufferViews[i], depthStencilView };
				fixed (SharpVulkan.ImageView* attachmentsPtr = attachments) {
					var createInfo = new SharpVulkan.FramebufferCreateInfo {
						StructureType = SharpVulkan.StructureType.FramebufferCreateInfo,
						AttachmentCount = (uint)attachments.Length,
						Attachments = new IntPtr(attachmentsPtr),
						Width = (uint)width,
						Height = (uint)height,
						Layers = 1,
						RenderPass = renderPass
					};
					framebuffers[i] = context.Device.CreateFramebuffer(ref createInfo);
				}
			}
		}

		private void AcquireNextImage()
		{
			acquirementSemaphore = acquirementSemaphores[nextAcquirementSemaphoreIndex];
			nextAcquirementSemaphoreIndex += 1;
			nextAcquirementSemaphoreIndex %= acquirementSemaphores.Length;
			backbufferIndex = context.Device.AcquireNextImage(swapchain, ulong.MaxValue, acquirementSemaphore, SharpVulkan.Fence.Null);
		}

		internal SharpVulkan.Semaphore ReleaseAcquirementSemaphore()
		{
			var semaphore = acquirementSemaphore;
			acquirementSemaphore = SharpVulkan.Semaphore.Null;
			return semaphore;
		}

		internal void Present()
		{
			var swapchainCopy = swapchain;
			var backbufferIndexCopy = backbufferIndex;
			var presentInfo = new SharpVulkan.PresentInfo {
				StructureType = SharpVulkan.StructureType.PresentInfo,
				SwapchainCount = 1,
				Swapchains = new IntPtr(&swapchainCopy),
				ImageIndices = new IntPtr(&backbufferIndexCopy)
			};
			context.Queue.Present(ref presentInfo);
			AcquireNextImage();
		}
	}
}
