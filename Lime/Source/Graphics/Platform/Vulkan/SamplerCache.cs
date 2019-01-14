using System;
using System.Collections.Generic;

namespace Lime.Graphics.Platform.Vulkan
{
	internal unsafe class SamplerCache
	{
		private PlatformRenderContext context;
		private Dictionary<long, SharpVulkan.Sampler> cache = new Dictionary<long, SharpVulkan.Sampler>();

		public SamplerCache(PlatformRenderContext context)
		{
			this.context = context;
		}

		public SharpVulkan.Sampler AcquireSampler(TextureParams textureParams)
		{
			textureParams = textureParams ?? TextureParams.Default;
			var hash = CalculateHash(textureParams);
			if (!cache.TryGetValue(hash, out var sampler)) {
				sampler = CreateSampler(textureParams);
				cache.Add(hash, sampler);
			}
			return sampler;
		}

		private SharpVulkan.Sampler CreateSampler(TextureParams textureParams)
		{
			var createInfo = new SharpVulkan.SamplerCreateInfo {
				StructureType = SharpVulkan.StructureType.SamplerCreateInfo,
				MinFilter = GetVKFilter(textureParams.MinFilter),
				MagFilter = GetVKFilter(textureParams.MagFilter),
				MipmapMode = GetVKSamplerMipmapMode(textureParams.MipmapMode),
				AddressModeU = GetVKSamplerAddressMode(textureParams.WrapModeU),
				AddressModeV = GetVKSamplerAddressMode(textureParams.WrapModeV)
			};
			return context.Device.CreateSampler(ref createInfo);
			// FIXME: Adjust MinLod, MaxLod to match OpenGL
		}

		private static SharpVulkan.Filter GetVKFilter(TextureFilter filter)
		{
			switch (filter) {
				case TextureFilter.Linear:
					return SharpVulkan.Filter.Linear;
				case TextureFilter.Nearest:
					return SharpVulkan.Filter.Nearest;
				default:
					throw new ArgumentException(nameof(filter));
			}
		}

		private static SharpVulkan.SamplerMipmapMode GetVKSamplerMipmapMode(TextureMipmapMode mode)
		{
			switch (mode) {
				case TextureMipmapMode.Linear:
					return SharpVulkan.SamplerMipmapMode.Linear;
				case TextureMipmapMode.Nearest:
					return SharpVulkan.SamplerMipmapMode.Nearest;
				default:
					throw new ArgumentException(nameof(mode));
			}
		}

		private static SharpVulkan.SamplerAddressMode GetVKSamplerAddressMode(TextureWrapMode mode)
		{
			switch (mode) {
				case TextureWrapMode.Clamp:
					return SharpVulkan.SamplerAddressMode.ClampToEdge;
				case TextureWrapMode.Repeat:
					return SharpVulkan.SamplerAddressMode.Repeat;
				case TextureWrapMode.MirroredRepeat:
					return SharpVulkan.SamplerAddressMode.MirroredRepeat;
				default:
					throw new ArgumentException(nameof(mode));
			}
		}

		private static long CalculateHash(TextureParams textureParams)
		{
			var hasher = new Hasher();
			hasher.Begin();
			hasher.Write(textureParams.MinFilter);
			hasher.Write(textureParams.MagFilter);
			hasher.Write(textureParams.MipmapMode);
			hasher.Write(textureParams.WrapModeU);
			hasher.Write(textureParams.WrapModeV);
			return hasher.End();
		}
	}
}
