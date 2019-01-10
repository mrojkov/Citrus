using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using static SharpVulkan.ResultExtensions;

namespace Lime.Graphics.Platform.Vulkan
{
	public unsafe class PlatformRenderContext : IPlatformRenderContext
	{
		private const bool validation = false;

		private SharpVulkan.Instance instance;
		private SharpVulkan.PhysicalDevice physicalDevice;
		private SharpVulkan.DebugReportCallback debugReportCallback;
		private DebugReportCallbackDelegate debugReport = DebugReport;
		private SharpVulkan.Device device;
		private SharpVulkan.Queue queue;
		private uint queueFamilyIndex;
		private SharpVulkan.CommandPool commandPool;
		private SharpVulkan.CommandBuffer commandBuffer;
		private SharpVulkan.PhysicalDeviceLimits physicalDeviceLimits;
		private Queue<SubmitInfo> submitInfos = new Queue<SubmitInfo>();
		private Stack<SharpVulkan.CommandBuffer> freeCommandBuffers = new Stack<SharpVulkan.CommandBuffer>();
		private Stack<SharpVulkan.Fence> freeFences = new Stack<SharpVulkan.Fence>();
		private ulong nextFenceValue = 1;
		private ulong lastCompletedFenceValue;
		private Swapchain swapchain;
		private Scheduler scheduler;
		private SharpVulkan.RenderPass activeRenderPass;
		private SharpVulkan.Format activeColorFormat;
		private SharpVulkan.Format activeDepthStencilFormat;
		private Viewport viewport;
		private BlendState blendState;
		private DepthState depthState;
		private StencilState stencilState;
		private ScissorState scissorState;
		private PlatformShaderProgram shaderProgram;
		private ColorWriteMask colorWriteMask;
		private CullMode cullMode;
		private FrontFace frontFace;
		private PrimitiveTopology primitiveTopology;
		private PlatformVertexInputLayout vertexInputLayout;
		private LruCache<long, SharpVulkan.Pipeline> pipelineLruCache = new LruCache<long, SharpVulkan.Pipeline>();
		private SharpVulkan.PipelineCache pipelineCache;
		private UploadBufferAllocator uploadBufferSuballocator;
		private DescriptorAllocator descriptorAllocator;
		private const int MaxVertexBufferSlots = 32;
		private PlatformBuffer[] vertexBuffers = new PlatformBuffer[MaxVertexBufferSlots];
		private int[] vertexOffsets = new int[MaxVertexBufferSlots];
		private PlatformBuffer indexBuffer;
		private int indexOffset;
		private IndexFormat indexFormat;
		private const int MaxTextureSlots = 32;
		private PlatformTexture2D[] textures = new PlatformTexture2D[MaxTextureSlots];
		private PlatformRenderTexture2D renderTarget;
		private SamplerCache samplerCache;
		private Dictionary<Format, FormatFeatures> formatFeaturesCache = new Dictionary<Format, FormatFeatures>();

		internal SharpVulkan.Instance Instance => instance;
		internal SharpVulkan.PhysicalDevice PhysicalDevice => physicalDevice;
		internal SharpVulkan.Device Device => device;
		internal SharpVulkan.Queue Queue => queue;
		internal uint QueueFamilyIndex => queueFamilyIndex;
		internal SharpVulkan.CommandBuffer CommandBuffer => commandBuffer;
		internal ulong NextFenceValue => nextFenceValue;
		internal SharpVulkan.PhysicalDeviceLimits PhysicalDeviceLimits => physicalDeviceLimits;
		internal SamplerCache SamplerCache => samplerCache;

		public PlatformRenderContext()
		{
			CreateInstance();
			if (validation) {
				CreateDebugReportCallback();
			}
			CreateDevice();
			CheckFeatures();
			CreateCommandPool();
			CreatePipelineCache();
			scheduler = new Scheduler(this);
			uploadBufferSuballocator = new UploadBufferAllocator(this, 4 * 1024 * 1024);
			descriptorAllocator = new DescriptorAllocator(this, new DescriptorPoolLimits {
				MaxSets = 512,
				MaxCombinedImageSamplers = 1024,
				MaxUniformBuffers = 1024
			});
			samplerCache = new SamplerCache(this);
			ResetState();
		}

		public void Dispose()
		{
			// FIXME: Implement PlatformRenderContext.Dispose
		}

		private void CheckFeatures()
		{
			physicalDevice.GetProperties(out var physicalDeviceProperties);
			physicalDeviceLimits = physicalDeviceProperties.Limits;
		}

		private void CreateInstance()
		{
			var applicationInfo = new SharpVulkan.ApplicationInfo {
				StructureType = SharpVulkan.StructureType.ApplicationInfo,
				ApiVersion = new SharpVulkan.Version(1, 0, 0)
			};

			var enabledLayerNames = new List<IntPtr>();
			if (validation) {
				enabledLayerNames.Add(Marshal.StringToHGlobalAnsi("VK_LAYER_LUNARG_standard_validation"));
			}
			var enabledExtensionNames = new List<IntPtr>();
			enabledExtensionNames.Add(Marshal.StringToHGlobalAnsi("VK_KHR_surface"));
			enabledExtensionNames.Add(Marshal.StringToHGlobalAnsi("VK_KHR_win32_surface"));
			enabledExtensionNames.Add(Marshal.StringToHGlobalAnsi("VK_EXT_debug_report"));
			try {
				fixed (IntPtr* enabledLayerNamesPtr = enabledLayerNames.ToArray())
				fixed (IntPtr* enabledExtensionNamesPtr = enabledExtensionNames.ToArray()) {
					var createInfo = new SharpVulkan.InstanceCreateInfo {
						StructureType = SharpVulkan.StructureType.InstanceCreateInfo,
						EnabledLayerCount = (uint)enabledLayerNames.Count,
						EnabledLayerNames = new IntPtr(enabledLayerNamesPtr),
						EnabledExtensionCount = (uint)enabledExtensionNames.Count,
						EnabledExtensionNames = new IntPtr(enabledExtensionNamesPtr)
					};
					instance = SharpVulkan.Vulkan.CreateInstance(ref createInfo);
				}
			} finally {
				foreach (var i in enabledLayerNames) {
					Marshal.FreeHGlobal(i);
				}
				foreach (var i in enabledExtensionNames) {
					Marshal.FreeHGlobal(i);
				}
			}
			physicalDevice = instance.PhysicalDevices[0];
		}

		private void CreateDebugReportCallback()
		{
			var createDebugReportCallback = GetInstanceEntryPoint<CreateDebugReportCallbackDelegate>("vkCreateDebugReportCallbackEXT");
			var createInfo = new SharpVulkan.DebugReportCallbackCreateInfo {
				StructureType = SharpVulkan.StructureType.DebugReportCallbackCreateInfo,
				Callback = Marshal.GetFunctionPointerForDelegate(debugReport),
				//Flags = (uint)(
				//	SharpVulkan.DebugReportFlags.Error |
				//	SharpVulkan.DebugReportFlags.Warning |
				//	SharpVulkan.DebugReportFlags.PerformanceWarning)
				Flags = (uint)(
					SharpVulkan.DebugReportFlags.Error |
					SharpVulkan.DebugReportFlags.Warning)
			};
			createDebugReportCallback(instance, ref createInfo, null, out debugReportCallback).CheckError();
		}

		private void CreateDevice()
		{
			queueFamilyIndex = physicalDevice.QueueFamilyProperties
				.Where((properties, index) => (properties.QueueFlags & SharpVulkan.QueueFlags.Graphics) != 0)
				.Select((properties, index) => (uint)index)
				.First();
			var queuePriority = 1.0f;
			var queueCreateInfo = new SharpVulkan.DeviceQueueCreateInfo {
				StructureType = SharpVulkan.StructureType.DeviceQueueCreateInfo,
				QueueCount = 1,
				QueuePriorities = new IntPtr(&queuePriority),
				QueueFamilyIndex = queueFamilyIndex
			};
			var enabledExtensionNames = new[] {
				Marshal.StringToHGlobalAnsi("VK_KHR_swapchain")
			};
			try {
				fixed (IntPtr* enabledExtensionNamesPtr = enabledExtensionNames) {
					var createInfo = new SharpVulkan.DeviceCreateInfo {
						StructureType = SharpVulkan.StructureType.DeviceCreateInfo,
						EnabledExtensionCount = (uint)enabledExtensionNames.Length,
						EnabledExtensionNames = new IntPtr(enabledExtensionNamesPtr),
						QueueCreateInfoCount = 1,
						QueueCreateInfos = new IntPtr(&queueCreateInfo)
					};
					device = physicalDevice.CreateDevice(ref createInfo);
				}
			} finally {
				foreach (var i in enabledExtensionNames) {
					Marshal.FreeHGlobal(i);
				}
			}
			queue = device.GetQueue(queueFamilyIndex, 0);
		}

		private void CreateCommandPool()
		{
			var createInfo = new SharpVulkan.CommandPoolCreateInfo {
				StructureType = SharpVulkan.StructureType.CommandPoolCreateInfo,
				Flags = SharpVulkan.CommandPoolCreateFlags.ResetCommandBuffer,
				QueueFamilyIndex = queueFamilyIndex
			};
			commandPool = device.CreateCommandPool(ref createInfo);
		}

		private void CreatePipelineCache()
		{
			var createInfo = new SharpVulkan.PipelineCacheCreateInfo {
				StructureType = SharpVulkan.StructureType.PipelineCacheCreateInfo,
			};
			pipelineCache = device.CreatePipelineCache(ref createInfo);
		}


		public void Begin(Swapchain swapchain)
		{
			if (this.swapchain != null) {
				throw new InvalidOperationException();
			}
			this.swapchain = swapchain;
			EnsureCommandBuffer();
			var memoryBarrier = new SharpVulkan.ImageMemoryBarrier {
				StructureType = SharpVulkan.StructureType.ImageMemoryBarrier,
				Image = swapchain.Backbuffer,
				OldLayout = SharpVulkan.ImageLayout.Undefined,
				NewLayout = SharpVulkan.ImageLayout.ColorAttachmentOptimal,
				SourceAccessMask = SharpVulkan.AccessFlags.None,
				DestinationAccessMask = SharpVulkan.AccessFlags.ColorAttachmentRead | SharpVulkan.AccessFlags.ColorAttachmentWrite,
				SubresourceRange = new SharpVulkan.ImageSubresourceRange(SharpVulkan.ImageAspectFlags.Color)
			};
			commandBuffer.PipelineBarrier(
				SharpVulkan.PipelineStageFlags.TopOfPipe, SharpVulkan.PipelineStageFlags.ColorAttachmentOutput,
				SharpVulkan.DependencyFlags.None, 0, null, 0, null, 1, &memoryBarrier);
		}

		public void Present()
		{
			if (swapchain == null) {
				throw new NotSupportedException();
			}
			EndRenderPass();
			EnsureCommandBuffer();
			var memoryBarrier = new SharpVulkan.ImageMemoryBarrier {
				StructureType = SharpVulkan.StructureType.ImageMemoryBarrier,
				Image = swapchain.Backbuffer,
				OldLayout = SharpVulkan.ImageLayout.ColorAttachmentOptimal,
				NewLayout = SharpVulkan.ImageLayout.PresentSource,
				SourceAccessMask = SharpVulkan.AccessFlags.ColorAttachmentRead | SharpVulkan.AccessFlags.ColorAttachmentWrite,
				DestinationAccessMask = SharpVulkan.AccessFlags.MemoryRead,
				SubresourceRange = new SharpVulkan.ImageSubresourceRange(SharpVulkan.ImageAspectFlags.Color)
			};
			commandBuffer.PipelineBarrier(
				SharpVulkan.PipelineStageFlags.ColorAttachmentOutput, SharpVulkan.PipelineStageFlags.BottomOfPipe,
				SharpVulkan.DependencyFlags.None, 0, null, 0, null, 1, &memoryBarrier);
			Flush();
			swapchain.Present();
			swapchain = null;
		}

		public void SetViewport(Viewport vp)
		{
			viewport = vp;
		}

		public void SetBlendState(BlendState state)
		{
			blendState = state;
		}

		public void SetDepthState(DepthState state)
		{
			depthState = state;
		}

		public void SetStencilState(StencilState state)
		{
			stencilState = state;
		}

		public void SetScissorState(ScissorState state)
		{
			scissorState = state;
		}

		public void SetColorWriteMask(ColorWriteMask mask)
		{
			colorWriteMask = mask;
		}

		public void SetCullMode(CullMode mode)
		{
			cullMode = mode;
		}

		public void SetFrontFace(FrontFace face)
		{
			frontFace = face;
		}

		public void SetPrimitiveTopology(PrimitiveTopology topology)
		{
			primitiveTopology = topology;
		}

		public void SetShaderProgram(IPlatformShaderProgram program)
		{
			shaderProgram = (PlatformShaderProgram)program;
		}

		public void SetVertexInputLayout(IPlatformVertexInputLayout layout)
		{
			vertexInputLayout = (PlatformVertexInputLayout)layout;
		}

		public void SetTexture(int slot, IPlatformTexture2D texture)
		{
			textures[slot] = (PlatformTexture2D)texture;
		}

		public void SetVertexBuffer(int slot, IPlatformBuffer buffer, int offset)
		{
			vertexBuffers[slot] = (PlatformBuffer)buffer;
			vertexOffsets[slot] = offset;
		}

		public void SetIndexBuffer(IPlatformBuffer buffer, int offset, IndexFormat format)
		{
			indexBuffer = (PlatformBuffer)buffer;
			indexOffset = offset;
			indexFormat = format;
		}

		public void Draw(int startVertex, int vertexCount)
		{
			PreDraw();
			commandBuffer.Draw((uint)vertexCount, 1, (uint)startVertex, 0);
		}

		public void DrawIndexed(int startIndex, int indexCount, int baseVertex)
		{
			PreDraw();
			commandBuffer.DrawIndexed((uint)indexCount, 1, (uint)startIndex, baseVertex, 0);
		}

		private void PreDraw()
		{
			EnsureRenderPass();
			// FIXME: Update blend constants

			var vkViewport = new SharpVulkan.Viewport(
				viewport.X, viewport.Y, viewport.Width, viewport.Height,
				viewport.MinDepth, viewport.MaxDepth);
			commandBuffer.SetViewport(0, 1, &vkViewport);

			var scissorBounds = scissorState.Enable ? scissorState.Bounds : viewport.Bounds;
			var vkScissor = new SharpVulkan.Rect2D(
				scissorBounds.X, scissorBounds.Y, (uint)scissorBounds.Width, (uint)scissorBounds.Height);
			commandBuffer.SetScissor(0, 1, &vkScissor);

			var pipeline = GetOrCreatePipeline();
			commandBuffer.BindPipeline(SharpVulkan.PipelineBindPoint.Graphics, pipeline);
			commandBuffer.SetStencilReference(SharpVulkan.StencilFaceFlags.FrontAndBack, stencilState.ReferenceValue);

			shaderProgram.UpdateUniformBuffers(nextFenceValue);
			BindDescriptorSets();
			BindVertexBuffers();
			BindIndexBuffer();
		}

		private void BindVertexBuffers()
		{
			for (var slot = 0; slot < vertexBuffers.Length; slot++) {
				var buffer = vertexBuffers[slot];
				if (buffer != null) {
					buffer.ReaderFenceValue = nextFenceValue;
					var vkBuffer = buffer.BackingBuffer.Buffer;
					var vkOffset = buffer.BackingBuffer.SliceOffset + (ulong)vertexOffsets[slot];
					commandBuffer.BindVertexBuffers((uint)slot, 1, &vkBuffer, &vkOffset);
				}
			}
		}

		private void BindIndexBuffer()
		{
			if (indexBuffer != null) {
				indexBuffer.ReaderFenceValue = nextFenceValue;
				var offset = indexBuffer.BackingBuffer.SliceOffset + (ulong)indexOffset;
				commandBuffer.BindIndexBuffer(indexBuffer.BackingBuffer.Buffer, offset, VulkanHelper.GetVKIndexType(indexFormat));
			}
		}

		private void BindDescriptorSets()
		{
			var descriptorSet = descriptorAllocator.AllocateDescriptorSet(shaderProgram);
			var updateTemplate = shaderProgram.DescriptorSetUpdateTemplate;
			var bufferInfos = stackalloc SharpVulkan.DescriptorBufferInfo[updateTemplate.Length];
			var imageInfos = stackalloc SharpVulkan.DescriptorImageInfo[updateTemplate.Length];
			var writes = stackalloc SharpVulkan.WriteDescriptorSet[updateTemplate.Length];
			var writeCount = 0;
			foreach (var templateEntry in updateTemplate) {
				writes[writeCount] = new SharpVulkan.WriteDescriptorSet {
					StructureType = SharpVulkan.StructureType.WriteDescriptorSet,
					DestinationSet = descriptorSet,
					DestinationBinding = (uint)templateEntry.Binding,
					DescriptorType = templateEntry.DescriptorType,
					DescriptorCount = 1,
					ImageInfo = new IntPtr(&imageInfos[writeCount]),
					BufferInfo = new IntPtr(&bufferInfos[writeCount])
				};
				switch (templateEntry.DescriptorType) {
					case SharpVulkan.DescriptorType.CombinedImageSampler:
						var texture = textures[templateEntry.TextureSlot];
						imageInfos[writeCount].ImageLayout = SharpVulkan.ImageLayout.ShaderReadOnlyOptimal;
						imageInfos[writeCount].ImageView = texture.ImageView;
						imageInfos[writeCount].Sampler = texture.Sampler;
						break;
					case SharpVulkan.DescriptorType.UniformBuffer:
						var buffer = shaderProgram.UniformBuffers[templateEntry.BufferSlot];
						bufferInfos[writeCount].Buffer = buffer.Buffer;
						bufferInfos[writeCount].Offset = buffer.SliceOffset;
						bufferInfos[writeCount].Range = buffer.SliceSize;
						break;
					default:
						throw new InvalidOperationException();
				}
				writeCount++;
			}
			device.UpdateDescriptorSets((uint)writeCount, writes, 0, null);
			EnsureCommandBuffer();
			commandBuffer.BindDescriptorSets(SharpVulkan.PipelineBindPoint.Graphics, shaderProgram.PipelineLayout, 0, 1, &descriptorSet, 0, null);
		}

		private SharpVulkan.Pipeline GetOrCreatePipeline()
		{
			const int lruCacheEvictThreshold = 4096;
			while (pipelineLruCache.Count > lruCacheEvictThreshold) {
				Release(pipelineLruCache.Evict());
			}
			var hash = ComputePipelineHash();
			if (!pipelineLruCache.TryGetValue(hash, out var pipeline)) {
				pipeline = CreatePipeline();
				pipelineLruCache.Add(hash, pipeline);
			}
			return pipeline;
		}

		private long ComputePipelineHash()
		{
			var hasher = new Hasher();
			hasher.Begin();
			hasher.Write(blendState.Enable);
			hasher.Write(blendState.ColorBlendFunc);
			hasher.Write(blendState.ColorSrcBlend);
			hasher.Write(blendState.ColorDstBlend);
			hasher.Write(blendState.AlphaBlendFunc);
			hasher.Write(blendState.AlphaSrcBlend);
			hasher.Write(blendState.AlphaDstBlend);
			hasher.Write(depthState.Enable);
			hasher.Write(depthState.WriteEnable);
			hasher.Write(depthState.Comparison);
			hasher.Write(stencilState.ReadMask);
			hasher.Write(stencilState.WriteMask);
			hasher.Write(stencilState.FrontFaceComparison);
			hasher.Write(stencilState.FrontFaceDepthFail);
			hasher.Write(stencilState.FrontFaceFail);
			hasher.Write(stencilState.FrontFacePass);
			hasher.Write(stencilState.BackFaceComparison);
			hasher.Write(stencilState.BackFaceDepthFail);
			hasher.Write(stencilState.BackFaceFail);
			hasher.Write(stencilState.BackFacePass);
			hasher.Write(colorWriteMask);
			hasher.Write(cullMode);
			hasher.Write(frontFace);
			hasher.Write(primitiveTopology);
			hasher.Write(vertexInputLayout.ReferenceHash);
			hasher.Write(shaderProgram.ReferenceHash);
			hasher.Write(activeColorFormat);
			hasher.Write(activeDepthStencilFormat);
			return hasher.End();
		}

		private SharpVulkan.Pipeline CreatePipeline()
		{
			var shaderEntryPointNamePtr = Marshal.StringToHGlobalAnsi("main");
			try {
				var vertexBindings = vertexInputLayout.Bindings
					.Select(binding => new SharpVulkan.VertexInputBindingDescription {
						Binding = (uint)binding.Slot,
						Stride = (uint)binding.Stride
					}).ToArray();

				var vertexAttributes = vertexInputLayout.Attributes
					.Select(attrib => new SharpVulkan.VertexInputAttributeDescription {
						Binding = (uint)attrib.Slot,
						Location = (uint)attrib.Location,
						Offset = (uint)attrib.Offset,
						Format = VulkanHelper.GetVKFormat(attrib.Format)
					}).ToArray();

				var inputAssemblyState = new SharpVulkan.PipelineInputAssemblyStateCreateInfo {
					StructureType = SharpVulkan.StructureType.PipelineInputAssemblyStateCreateInfo,
					Topology = VulkanHelper.GetVKPrimitiveTopology(primitiveTopology)
				};
				var rasterState = new SharpVulkan.PipelineRasterizationStateCreateInfo {
					StructureType = SharpVulkan.StructureType.PipelineRasterizationStateCreateInfo,
					CullMode = VulkanHelper.GetVKCullModeFlags(cullMode),
					FrontFace = VulkanHelper.GetVKFrontFace(frontFace),
					PolygonMode = SharpVulkan.PolygonMode.Fill,
					DepthBiasEnable = false,
					DepthClampEnable = false,
					RasterizerDiscardEnable = false,
					LineWidth = 1.0f
				};
				var depthStencilState = new SharpVulkan.PipelineDepthStencilStateCreateInfo {
					StructureType = SharpVulkan.StructureType.PipelineDepthStencilStateCreateInfo,
					DepthTestEnable = depthState.Enable,
					DepthWriteEnable = depthState.WriteEnable,
					DepthCompareOperation = VulkanHelper.GetVKCompareOp(depthState.Comparison),
					DepthBoundsTestEnable = false,
					StencilTestEnable = stencilState.Enable,
					Front = new SharpVulkan.StencilOperationState {
						CompareOperation = VulkanHelper.GetVKCompareOp(stencilState.FrontFaceComparison),
						DepthFailOperation = VulkanHelper.GetVKStencilOp(stencilState.FrontFaceDepthFail),
						FailOperation = VulkanHelper.GetVKStencilOp(stencilState.FrontFaceFail),
						PassOperation = VulkanHelper.GetVKStencilOp(stencilState.FrontFacePass),
						CompareMask = stencilState.ReadMask,
						WriteMask = stencilState.WriteMask
					},
					Back = new SharpVulkan.StencilOperationState {
						CompareOperation = VulkanHelper.GetVKCompareOp(stencilState.BackFaceComparison),
						DepthFailOperation = VulkanHelper.GetVKStencilOp(stencilState.BackFaceDepthFail),
						FailOperation = VulkanHelper.GetVKStencilOp(stencilState.BackFaceFail),
						PassOperation = VulkanHelper.GetVKStencilOp(stencilState.BackFacePass),
						CompareMask = stencilState.ReadMask,
						WriteMask = stencilState.WriteMask
					}
				};
				var colorBlendAttachment = new SharpVulkan.PipelineColorBlendAttachmentState {
					BlendEnable = blendState.Enable,
					ColorBlendOperation = VulkanHelper.GetVKBlendOp(blendState.ColorBlendFunc),
					AlphaBlendOperation = VulkanHelper.GetVKBlendOp(blendState.AlphaBlendFunc),
					SourceColorBlendFactor = VulkanHelper.GetVKBlendFactor(blendState.ColorSrcBlend),
					SourceAlphaBlendFactor = VulkanHelper.GetVKBlendFactor(blendState.AlphaSrcBlend),
					DestinationColorBlendFactor = VulkanHelper.GetVKBlendFactor(blendState.ColorDstBlend),
					DestinationAlphaBlendFactor = VulkanHelper.GetVKBlendFactor(blendState.AlphaDstBlend),
					ColorWriteMask = VulkanHelper.GetVKColorComponentFlags(colorWriteMask)
				};
				var colorBlendState = new SharpVulkan.PipelineColorBlendStateCreateInfo {
					StructureType = SharpVulkan.StructureType.PipelineColorBlendStateCreateInfo,
					AttachmentCount = 1,
					Attachments = new IntPtr(&colorBlendAttachment)
				};
				var viewportState = new SharpVulkan.PipelineViewportStateCreateInfo {
					StructureType = SharpVulkan.StructureType.PipelineViewportStateCreateInfo,
					ViewportCount = 1,
					ScissorCount = 1
				};
				var stages = new[] {
					new SharpVulkan.PipelineShaderStageCreateInfo {
						StructureType = SharpVulkan.StructureType.PipelineShaderStageCreateInfo,
						Stage = SharpVulkan.ShaderStageFlags.Vertex,
						Module = shaderProgram.VSModule,
						Name = shaderEntryPointNamePtr
					},
					new SharpVulkan.PipelineShaderStageCreateInfo {
						StructureType = SharpVulkan.StructureType.PipelineShaderStageCreateInfo,
						Stage = SharpVulkan.ShaderStageFlags.Fragment,
						Module = shaderProgram.FSModule,
						Name = shaderEntryPointNamePtr
					}
				};
				var dynamicStates = new[] {
					SharpVulkan.DynamicState.Viewport,
					SharpVulkan.DynamicState.StencilReference,
					SharpVulkan.DynamicState.BlendConstants,
					SharpVulkan.DynamicState.Scissor
				};
				var multisampleState = new SharpVulkan.PipelineMultisampleStateCreateInfo {
					StructureType = SharpVulkan.StructureType.PipelineMultisampleStateCreateInfo,
					RasterizationSamples = SharpVulkan.SampleCountFlags.Sample1,
				};
				var tessellationState = new SharpVulkan.PipelineTessellationStateCreateInfo {
					StructureType = SharpVulkan.StructureType.PipelineTessellationStateCreateInfo
				};
				fixed (SharpVulkan.VertexInputBindingDescription* vertexBindingsPtr = vertexBindings)
				fixed (SharpVulkan.VertexInputAttributeDescription* vertexAttributesPtr = vertexAttributes)
				fixed (SharpVulkan.PipelineShaderStageCreateInfo* stagesPtr = stages)
				fixed (SharpVulkan.DynamicState* dynamicStatesPtr = dynamicStates) {
					var vertexInputState = new SharpVulkan.PipelineVertexInputStateCreateInfo {
						StructureType = SharpVulkan.StructureType.PipelineVertexInputStateCreateInfo,
						VertexBindingDescriptionCount = (uint)vertexBindings.Length,
						VertexBindingDescriptions = new IntPtr(vertexBindingsPtr),
						VertexAttributeDescriptionCount = (uint)vertexAttributes.Length,
						VertexAttributeDescriptions = new IntPtr(vertexAttributesPtr)
					};
					var dynamicState = new SharpVulkan.PipelineDynamicStateCreateInfo {
						StructureType = SharpVulkan.StructureType.PipelineDynamicStateCreateInfo,
						DynamicStateCount = (uint)dynamicStates.Length,
						DynamicStates = new IntPtr(dynamicStatesPtr)
					};
					var createInfo = new SharpVulkan.GraphicsPipelineCreateInfo {
						StructureType = SharpVulkan.StructureType.GraphicsPipelineCreateInfo,
						StageCount = (uint)stages.Length,
						Stages = new IntPtr(stagesPtr),
						ViewportState = new IntPtr(&viewportState),
						InputAssemblyState = new IntPtr(&inputAssemblyState),
						VertexInputState = new IntPtr(&vertexInputState),
						RasterizationState = new IntPtr(&rasterState),
						DepthStencilState = new IntPtr(&depthStencilState),
						ColorBlendState = new IntPtr(&colorBlendState),
						MultisampleState = new IntPtr(&multisampleState),
						TessellationState = new IntPtr(&tessellationState),
						DynamicState = new IntPtr(&dynamicState),
						RenderPass = activeRenderPass,
						Layout = shaderProgram.PipelineLayout,
						Subpass = 0
					};
					return device.CreateGraphicsPipelines(pipelineCache, 1, &createInfo);
				}
			} finally {
				Marshal.FreeHGlobal(shaderEntryPointNamePtr);
			}
		}

		public void Clear(ClearOptions options, float r, float g, float b, float a, float depth, byte stencil)
		{
			EnsureRenderPass();
			var attachments = stackalloc SharpVulkan.ClearAttachment[2];
			var attachmentCount = 0U;
			if ((options & ClearOptions.ColorBuffer) != 0) {
				// FIXME: Handle SInt, UInt formats
				attachments[attachmentCount++] = new SharpVulkan.ClearAttachment {
					ColorAttachment = 0,
					AspectMask = SharpVulkan.ImageAspectFlags.Color,
					ClearValue = new SharpVulkan.ClearValue {
						Color = new SharpVulkan.RawColor4(r, g, b, a)
					}
				};
			}
			if ((options & (ClearOptions.DepthBuffer | ClearOptions.StencilBuffer)) != 0) {
				var aspect = SharpVulkan.ImageAspectFlags.None;
				if ((options & ClearOptions.DepthBuffer) != 0) {
					aspect |= SharpVulkan.ImageAspectFlags.Depth;
				}
				if ((options & ClearOptions.StencilBuffer) != 0) {
					aspect |= SharpVulkan.ImageAspectFlags.Stencil;
				}
				attachments[attachmentCount++] = new SharpVulkan.ClearAttachment {
					AspectMask = aspect,
					ClearValue = new SharpVulkan.ClearValue {
						DepthStencil = new SharpVulkan.ClearDepthStencilValue(depth, stencil)
					}
				};
			}
			var clearRect = new SharpVulkan.ClearRect {
				BaseArrayLayer = 0,
				LayerCount = 1,
				Rect = new SharpVulkan.Rect2D(0, 0, (uint)viewport.Width, (uint)viewport.Height)
			};
			commandBuffer.ClearAttachments(attachmentCount, ref attachments[0], 1, &clearRect);
		}

		public void SetRenderTarget(IPlatformRenderTexture2D texture)
		{
			if (renderTarget != texture) {
				EndRenderPass();
				renderTarget = (PlatformRenderTexture2D)texture;
			}
		}

		private void EnsureRenderPass()
		{
			if (activeRenderPass == SharpVulkan.RenderPass.Null) {
				SharpVulkan.Framebuffer fb;
				int width, height;
				if (renderTarget != null) {
					activeRenderPass = renderTarget.RenderPass;
					activeColorFormat = renderTarget.ColorFormat;
					activeDepthStencilFormat = renderTarget.DepthStencilFormat;
					fb = renderTarget.Framebuffer;
					width = renderTarget.Width;
					height = renderTarget.Height;
				} else {
					activeRenderPass = swapchain.RenderPass;
					activeColorFormat = swapchain.BackbufferFormat;
					activeDepthStencilFormat = swapchain.DepthStencilFormat;
					fb = swapchain.Framebuffer;
					width = swapchain.Width;
					height = swapchain.Height;
				}
				EnsureCommandBuffer();
				var rpBeginInfo = new SharpVulkan.RenderPassBeginInfo {
					StructureType = SharpVulkan.StructureType.RenderPassBeginInfo,
					RenderPass = activeRenderPass,
					Framebuffer = fb,
					RenderArea = new SharpVulkan.Rect2D(0, 0, (uint)width, (uint)height)
				};
				commandBuffer.BeginRenderPass(ref rpBeginInfo, SharpVulkan.SubpassContents.Inline);
			}
		}

		internal void EndRenderPass()
		{
			if (activeRenderPass != SharpVulkan.RenderPass.Null) {
				commandBuffer.EndRenderPass();
				activeRenderPass = SharpVulkan.RenderPass.Null;
			}
		}

		public void Finish()
		{
			Flush();
			device.WaitIdle();
			scheduler.Perform();
		}

		public void Flush()
		{
			scheduler.Perform();
			if (commandBuffer != SharpVulkan.CommandBuffer.Null) {
				EndRenderPass();
				commandBuffer.End();
				descriptorAllocator.DiscardPool();
				var waitSemaphore = swapchain?.ReleaseAcquirementSemaphore() ?? SharpVulkan.Semaphore.Null;
				Submit(commandBuffer, waitSemaphore, SharpVulkan.PipelineStageFlags.AllCommands);
				commandBuffer = SharpVulkan.CommandBuffer.Null;
				InvalidateState();
			}
		}

		private void ResetState()
		{
			renderTarget = null;
			viewport = Viewport.Default;
			blendState = BlendState.Default;
			depthState = DepthState.Default;
			stencilState = StencilState.Default;
			scissorState = ScissorState.Default;
			colorWriteMask = ColorWriteMask.All;
			cullMode = CullMode.None;
			frontFace = FrontFace.CW;
			primitiveTopology = PrimitiveTopology.TriangleList;
			vertexInputLayout = null;
			shaderProgram = null;
			for (var i = 0; i < textures.Length; i++) {
				textures[i] = null;
			}
			for (var i = 0; i < vertexBuffers.Length; i++) {
				vertexBuffers[i] = null;
			}
			indexBuffer = null;
			InvalidateState();
		}

		private void InvalidateState()
		{
		}

		private void Submit(SharpVulkan.CommandBuffer cb, SharpVulkan.Semaphore waitSemaphore, SharpVulkan.PipelineStageFlags waitDstStageMask)
		{
			var fence = AcquireFence();
			var submitInfo = new SharpVulkan.SubmitInfo {
				StructureType = SharpVulkan.StructureType.SubmitInfo,
				CommandBufferCount = 1,
				CommandBuffers = new IntPtr(&cb),
				WaitSemaphoreCount = waitSemaphore != SharpVulkan.Semaphore.Null ? 1U : 0U,
				WaitSemaphores = new IntPtr(&waitSemaphore),
				WaitDstStageMask = new IntPtr(&waitDstStageMask)
			};
			queue.Submit(1, &submitInfo, fence);
			submitInfos.Enqueue(new SubmitInfo {
				CommandBuffer = cb,
				Fence = fence,
				FenceValue = nextFenceValue++
			});
		}

		private SharpVulkan.CommandBuffer GetCurrentCommandBuffer()
		{
			if (commandBuffer == SharpVulkan.CommandBuffer.Null) {
				commandBuffer = AcquireCommandBuffer();
				var beginInfo = new SharpVulkan.CommandBufferBeginInfo {
					StructureType = SharpVulkan.StructureType.CommandBufferBeginInfo,
					Flags = SharpVulkan.CommandBufferUsageFlags.OneTimeSubmit
				};
				commandBuffer.Begin(ref beginInfo);
			}
			return commandBuffer;
		}

		internal void EnsureCommandBuffer()
		{
			if (commandBuffer == SharpVulkan.CommandBuffer.Null) {
				commandBuffer = AcquireCommandBuffer();
				var beginInfo = new SharpVulkan.CommandBufferBeginInfo {
					StructureType = SharpVulkan.StructureType.CommandBufferBeginInfo,
					Flags = SharpVulkan.CommandBufferUsageFlags.OneTimeSubmit
				};
				commandBuffer.Begin(ref beginInfo);
			}
		}

		private SharpVulkan.CommandBuffer AcquireCommandBuffer()
		{
			CheckCompletedCommandBuffers();
			if (freeCommandBuffers.Count > 0) {
				return freeCommandBuffers.Pop();
			}
			var allocateInfo = new SharpVulkan.CommandBufferAllocateInfo {
				StructureType = SharpVulkan.StructureType.CommandBufferAllocateInfo,
				CommandPool = commandPool,
				CommandBufferCount = 1
			};
			SharpVulkan.CommandBuffer cb;
			device.AllocateCommandBuffers(ref allocateInfo, &cb);
			return cb;
		}

		private SharpVulkan.Fence AcquireFence()
		{
			CheckCompletedCommandBuffers();
			if (freeFences.Count > 0) {
				var fence = freeFences.Pop();
				device.ResetFences(1, &fence);
				return fence;
			}
			var createInfo = new SharpVulkan.FenceCreateInfo {
				StructureType = SharpVulkan.StructureType.FenceCreateInfo
			};
			return device.CreateFence(ref createInfo);
		}

		internal bool IsFenceCompleted(ulong fenceValue)
		{
			if (fenceValue > lastCompletedFenceValue) {
				CheckCompletedCommandBuffers();
			}
			return fenceValue <= lastCompletedFenceValue;
		}

		internal void WaitForFence(ulong fenceValue)
		{
			if (fenceValue > lastCompletedFenceValue) {
				CheckCompletedCommandBuffers();
				while (fenceValue > lastCompletedFenceValue) {
					// FIXME: Find another solution
					System.Threading.Thread.Yield();
					CheckCompletedCommandBuffers();
				}
			}
		}

		private void CheckCompletedCommandBuffers()
		{
			while (submitInfos.Count > 0 && device.GetFenceStatus(submitInfos.Peek().Fence) == SharpVulkan.Result.Success) {
				var s = submitInfos.Dequeue();
				freeCommandBuffers.Push(s.CommandBuffer);
				freeFences.Push(s.Fence);
				lastCompletedFenceValue = s.FenceValue;
			}
		}

		internal void Release(SharpVulkan.Buffer obj)
		{
			scheduler.Add(nextFenceValue, () => {
				device.DestroyBuffer(obj);
			});
		}

		internal void Release(SharpVulkan.Image obj)
		{
			scheduler.Add(nextFenceValue, () => {
				device.DestroyImage(obj);
			});
		}

		internal void Release(SharpVulkan.ImageView obj)
		{
			scheduler.Add(nextFenceValue, () => {
				device.DestroyImageView(obj);
			});
		}

		internal void Release(SharpVulkan.DeviceMemory obj)
		{
			scheduler.Add(nextFenceValue, () => {
				device.FreeMemory(obj);
			});
		}

		internal void Release(SharpVulkan.Framebuffer obj)
		{
			scheduler.Add(nextFenceValue, () => {
				device.DestroyFramebuffer(obj);
			});
		}

		internal void Release(SharpVulkan.RenderPass obj)
		{
			scheduler.Add(nextFenceValue, () => {
				device.DestroyRenderPass(obj);
			});
		}

		internal void Release(SharpVulkan.DescriptorSetLayout obj)
		{
			scheduler.Add(nextFenceValue, () => {
				device.DestroyDescriptorSetLayout(obj);
			});
		}

		internal void Release(SharpVulkan.PipelineLayout obj)
		{
			scheduler.Add(nextFenceValue, () => {
				device.DestroyPipelineLayout(obj);
			});
		}

		internal void Release(SharpVulkan.ShaderModule obj)
		{
			scheduler.Add(nextFenceValue, () => {
				device.DestroyShaderModule(obj);
			});
		}

		internal void Release(SharpVulkan.Pipeline obj)
		{
			scheduler.Add(nextFenceValue, () => {
				device.DestroyPipeline(obj);
			});
		}

		internal void Release(SharpVulkan.Sampler obj)
		{
			scheduler.Add(nextFenceValue, () => {
				device.DestroySampler(obj);
			});
		}

		public FormatFeatures GetFormatFeatures(Format format)
		{
			if (!formatFeaturesCache.TryGetValue(format, out var features)) {
				physicalDevice.GetFormatProperties(VulkanHelper.GetVKFormat(format), out var formatProperties);
				features = FormatFeatures.None;
				if ((formatProperties.OptimalTilingFeatures & SharpVulkan.FormatFeatureFlags.SampledImage) != 0) {
					features |= FormatFeatures.Sample;
				}
				if ((formatProperties.OptimalTilingFeatures & SharpVulkan.FormatFeatureFlags.ColorAttachmentBlend) != 0) {
					features |= FormatFeatures.RenderTarget;
				}
				if ((formatProperties.BufferFeatures & SharpVulkan.FormatFeatureFlags.VertexBuffer) != 0) {
					features |= FormatFeatures.VertexBuffer;
				}
				formatFeaturesCache.Add(format, features);
			}
			return features;
		}

		internal UploadBufferAlloc AllocateUploadBuffer(ulong size, ulong alignment)
		{
			return uploadBufferSuballocator.Allocate(size, alignment);
		}

		internal SharpVulkan.DeviceMemory AllocateMemory(SharpVulkan.MemoryRequirements requirements, SharpVulkan.MemoryPropertyFlags propertyFlags)
		{
			var typeIndex = FindMemoryTypeIndex(requirements.MemoryTypeBits, propertyFlags);
			if (typeIndex == uint.MaxValue) {
				throw new InvalidOperationException();
			}
			var allocateInfo = new SharpVulkan.MemoryAllocateInfo {
				StructureType = SharpVulkan.StructureType.MemoryAllocateInfo,
				MemoryTypeIndex = typeIndex,
				AllocationSize = requirements.Size
			};
			return device.AllocateMemory(ref allocateInfo);
		}

		private uint FindMemoryTypeIndex(uint typeBits, SharpVulkan.MemoryPropertyFlags propertyFlags)
		{
			physicalDevice.GetMemoryProperties(out var memoryProperties);
			for (var i = 0U; i < memoryProperties.MemoryTypeCount; i++) {
				if ((typeBits & 1) == 1) {
					var memoryType = &memoryProperties.MemoryTypes.Value0 + i;
					if ((memoryType->PropertyFlags & propertyFlags) == propertyFlags) {
						return i;
					}
				}
				typeBits >>= 1;
			}
			return uint.MaxValue;
		}

		private T GetInstanceEntryPoint<T>(string name) where T : Delegate
		{
			var namePtr = Marshal.StringToHGlobalAnsi(name);
			try {
				var address = instance.GetProcAddress((byte*)namePtr);
				return Marshal.GetDelegateForFunctionPointer<T>(address);
			} finally {
				Marshal.FreeHGlobal(namePtr);
			}
		}

		public Swapchain CreateSwapchain(IntPtr windowHandle, int width, int height)
		{
			return new Swapchain(this, windowHandle, width, height);
		}

		public IPlatformTexture2D CreateTexture2D(Format format, int width, int height, bool mipmaps, TextureParams textureParams)
		{
			return new PlatformTexture2D(this, format, width, height, mipmaps, textureParams);
		}

		public IPlatformRenderTexture2D CreateRenderTexture2D(Format format, int width, int height, TextureParams textureParams)
		{
			return new PlatformRenderTexture2D(this, format, width, height, textureParams);
		}

		public IPlatformShader CreateShader(ShaderStageMask stage, string source)
		{
			return new PlatformShader(this, stage, source);
		}

		public IPlatformShaderProgram CreateShaderProgram(
			IPlatformShader[] shaders,
			ShaderProgram.AttribLocation[] attribLocation,
			ShaderProgram.Sampler[] samplers)
		{
			return new PlatformShaderProgram(this, shaders, attribLocation, samplers);
		}

		public IPlatformVertexInputLayout CreateVertexInputLayout(
			VertexInputLayoutBinding[] bindings,
			VertexInputLayoutAttribute[] attributes)
		{
			return new PlatformVertexInputLayout(this, bindings, attributes);
		}

		public IPlatformBuffer CreateBuffer(BufferType bufferType, int size, bool dynamic)
		{
			return new PlatformBuffer(this, bufferType, size, dynamic);
		}

		private struct SubmitInfo
		{
			public SharpVulkan.CommandBuffer CommandBuffer;
			public SharpVulkan.Fence Fence;
			public ulong FenceValue;
		}

		private static SharpVulkan.RawBool DebugReport(
			SharpVulkan.DebugReportFlags flags, SharpVulkan.DebugReportObjectType objectType, ulong @object,
			SharpVulkan.PointerSize location, int messageCode, string layerPrefix, string message, IntPtr userData)
		{
			Logger.Write($"{flags}: {message} ([{messageCode}] {layerPrefix})");
			return false;
		}

		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		private delegate SharpVulkan.RawBool DebugReportCallbackDelegate(
			SharpVulkan.DebugReportFlags flags, SharpVulkan.DebugReportObjectType objectType, ulong @object,
			SharpVulkan.PointerSize location, int messageCode, string layerPrefix, string message, IntPtr userData);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate SharpVulkan.Result CreateDebugReportCallbackDelegate(
			SharpVulkan.Instance instance, ref SharpVulkan.DebugReportCallbackCreateInfo createInfo,
			SharpVulkan.AllocationCallbacks* allocator, out SharpVulkan.DebugReportCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate SharpVulkan.Result DestroyDebugReportCallbackDelegate(
			SharpVulkan.Instance instance, SharpVulkan.DebugReportCallback debugReportCallback,
			SharpVulkan.AllocationCallbacks* allocator);
	}
}
