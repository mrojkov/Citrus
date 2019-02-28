using System;
using System.Collections.Generic;

namespace Lime.Graphics.Platform.Vulkan
{
	internal unsafe class DescriptorAllocator
	{
		private PlatformRenderContext context;
		private DescriptorPoolLimits poolLimits;
		private int allocatedCombinedImageSamplers;
		private int allocatedUniformBuffers;
		private int allocatedSets;
		private SharpVulkan.DescriptorPool pool;
		private Queue<BusyPool> busyPools = new Queue<BusyPool>();

		public DescriptorAllocator(PlatformRenderContext context, DescriptorPoolLimits poolLimits)
		{
			this.context = context;
			this.poolLimits = poolLimits;
			pool = AcquireDescriptorPool();
		}

		public SharpVulkan.DescriptorSet AllocateDescriptorSet(PlatformShaderProgram program)
		{
			if (allocatedSets + 1 > poolLimits.MaxSets ||
				allocatedCombinedImageSamplers + program.CombinedImageSamplerCount > poolLimits.MaxCombinedImageSamplers||
				allocatedUniformBuffers + program.UniformBufferCount > poolLimits.MaxUniformBuffers
			) {
				DiscardPool();
			}
			var setLayout = program.DescriptorSetLayout;
			var allocateInfo = new SharpVulkan.DescriptorSetAllocateInfo {
				StructureType = SharpVulkan.StructureType.DescriptorSetAllocateInfo,
				DescriptorPool = pool,
				DescriptorSetCount = 1,
				SetLayouts = new IntPtr(&setLayout)
			};
			SharpVulkan.DescriptorSet ds;
			context.Device.AllocateDescriptorSets(ref allocateInfo, &ds);
			allocatedSets++;
			allocatedCombinedImageSamplers += program.CombinedImageSamplerCount;
			allocatedUniformBuffers += program.UniformBufferCount;
			return ds;
		}

		private SharpVulkan.DescriptorPool AcquireDescriptorPool()
		{
			if (busyPools.Count > 0 && context.IsFenceCompleted(busyPools.Peek().FenceValue)) {
				var pool = busyPools.Dequeue().Pool;
				context.Device.ResetDescriptorPool(pool, SharpVulkan.DescriptorPoolResetFlags.None);
				return pool;
			}
			var poolSizes = new[] {
				new SharpVulkan.DescriptorPoolSize {
					Type = SharpVulkan.DescriptorType.CombinedImageSampler,
					DescriptorCount = (uint)poolLimits.MaxCombinedImageSamplers
				},
				new SharpVulkan.DescriptorPoolSize {
					Type = SharpVulkan.DescriptorType.UniformBuffer,
					DescriptorCount = (uint)poolLimits.MaxUniformBuffers
				}
			};
			fixed (SharpVulkan.DescriptorPoolSize* poolSizesPtr = poolSizes) {
				var createInfo = new SharpVulkan.DescriptorPoolCreateInfo {
					StructureType = SharpVulkan.StructureType.DescriptorPoolCreateInfo,
					MaxSets = (uint)poolLimits.MaxSets,
					PoolSizeCount = (uint)poolSizes.Length,
					PoolSizes = new IntPtr(poolSizesPtr)
				};
				return context.Device.CreateDescriptorPool(ref createInfo);
			}
		}

		public void DiscardPool()
		{
			if (allocatedSets > 0) {
				busyPools.Enqueue(new BusyPool {
					FenceValue = context.NextFenceValue,
					Pool = pool
				});
				pool = AcquireDescriptorPool();
				allocatedSets = 0;
				allocatedCombinedImageSamplers = 0;
				allocatedUniformBuffers = 0;
			}
		}

		private struct BusyPool
		{
			public ulong FenceValue;
			public SharpVulkan.DescriptorPool Pool;
		}
	}

	internal struct DescriptorPoolLimits
	{
		internal int MaxSets;
		internal int MaxCombinedImageSamplers;
		internal int MaxUniformBuffers;
	}
}
