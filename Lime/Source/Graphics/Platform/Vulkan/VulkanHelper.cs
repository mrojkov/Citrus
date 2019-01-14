using System;

namespace Lime.Graphics.Platform.Vulkan
{
	internal static class VulkanHelper
	{
		public static SharpVulkan.Format GetVKFormat(Format format)
		{
			switch (format) {
				case Format.Undefined:
					return SharpVulkan.Format.Undefined;
				case Format.R8_SNorm:
					return SharpVulkan.Format.R8SNorm;
				case Format.R8_SScaled:
					return SharpVulkan.Format.R8Sscaled;
				case Format.R8_UNorm:
					return SharpVulkan.Format.R8UNorm;
				case Format.R8_UScaled:
					return SharpVulkan.Format.R8UScaled;
				case Format.R8G8_SNorm:
					return SharpVulkan.Format.R8G8SNorm;
				case Format.R8G8_SScaled:
					return SharpVulkan.Format.R8G8Sscaled;
				case Format.R8G8_UNorm:
					return SharpVulkan.Format.R8G8UNorm;
				case Format.R8G8_UScaled:
					return SharpVulkan.Format.R8G8UScaled;
				case Format.R8G8B8_SNorm:
					return SharpVulkan.Format.R8G8B8SNorm;
				case Format.R8G8B8_SScaled:
					return SharpVulkan.Format.R8G8B8Sscaled;
				case Format.R8G8B8_UNorm:
					return SharpVulkan.Format.R8G8B8UNorm;
				case Format.R8G8B8_UScaled:
					return SharpVulkan.Format.R8G8B8UScaled;
				case Format.R8G8B8A8_SNorm:
					return SharpVulkan.Format.R8G8B8A8SNorm;
				case Format.R8G8B8A8_SScaled:
					return SharpVulkan.Format.R8G8B8A8Sscaled;
				case Format.R8G8B8A8_UNorm:
					return SharpVulkan.Format.R8G8B8A8UNorm;
				case Format.R8G8B8A8_UScaled:
					return SharpVulkan.Format.R8G8B8A8UScaled;
				case Format.R16_SNorm:
					return SharpVulkan.Format.R16SNorm;
				case Format.R16_SScaled:
					return SharpVulkan.Format.R16Sscaled;
				case Format.R16_UNorm:
					return SharpVulkan.Format.R16UNorm;
				case Format.R16_UScaled:
					return SharpVulkan.Format.R16UScaled;
				case Format.R16G16_SNorm:
					return SharpVulkan.Format.R16G16SNorm;
				case Format.R16G16_SScaled:
					return SharpVulkan.Format.R16G16Sscaled;
				case Format.R16G16_UNorm:
					return SharpVulkan.Format.R16G16UNorm;
				case Format.R16G16_UScaled:
					return SharpVulkan.Format.R16G16UScaled;
				case Format.R16G16B16_SNorm:
					return SharpVulkan.Format.R16G16B16SNorm;
				case Format.R16G16B16_SScaled:
					return SharpVulkan.Format.R16G16B16Sscaled;
				case Format.R16G16B16_UNorm:
					return SharpVulkan.Format.R16G16B16UNorm;
				case Format.R16G16B16_UScaled:
					return SharpVulkan.Format.R16G16B16UScaled;
				case Format.R16G16B16A16_SNorm:
					return SharpVulkan.Format.R16G16B16A16SNorm;
				case Format.R16G16B16A16_SScaled:
					return SharpVulkan.Format.R16G16B16A16Sscaled;
				case Format.R16G16B16A16_UNorm:
					return SharpVulkan.Format.R16G16B16A16UNorm;
				case Format.R16G16B16A16_UScaled:
					return SharpVulkan.Format.R16G16B16A16UScaled;
				case Format.R32_SFloat:
					return SharpVulkan.Format.R32SFloat;
				case Format.R32G32_SFloat:
					return SharpVulkan.Format.R32G32SFloat;
				case Format.R32G32B32_SFloat:
					return SharpVulkan.Format.R32G32B32SFloat;
				case Format.R32G32B32A32_SFloat:
					return SharpVulkan.Format.R32G32B32A32SFloat;
				case Format.R5G6B5_UNorm_Pack16:
					return SharpVulkan.Format.R5G6B5UNormPack16;
				case Format.R5G5B5A1_UNorm_Pack16:
					return SharpVulkan.Format.R5G5B5A1UNormPack16;
				case Format.R4G4B4A4_UNorm_Pack16:
					return SharpVulkan.Format.R4G4B4A4UNormPack16;
				case Format.BC1_RGB_UNorm_Block:
					return SharpVulkan.Format.Bc1RgbUNormBlock;
				case Format.BC1_RGBA_UNorm_Block:
					return SharpVulkan.Format.Bc1RgbaUNormBlock;
				case Format.BC2_UNorm_Block:
					return SharpVulkan.Format.Bc2UNormBlock;
				case Format.BC3_UNorm_Block:
					return SharpVulkan.Format.Bc3UNormBlock;
				case Format.ETC1_R8G8B8_UNorm_Block:
				case Format.ETC2_R8G8B8_UNorm_Block:
					return SharpVulkan.Format.Etc2R8G8B8UNormBlock;
				case Format.ETC2_R8G8B8A1_UNorm_Block:
					return SharpVulkan.Format.Etc2R8G8B8A1UNormBlock;
				case Format.ETC2_R8G8B8A8_UNorm_Block:
					return SharpVulkan.Format.Etc2R8G8B8A8UNormBlock;
				case Format.PVRTC1_2Bpp_UNorm_Block:
					return SharpVulkan.Format.Pvrtc12BppUNormBlock;
				case Format.PVRTC1_4Bpp_UNorm_Block:
					return SharpVulkan.Format.Pvrtc14BppUNormBlock;
				case Format.PVRTC2_2Bpp_UNorm_Block:
					return SharpVulkan.Format.Pvrtc22BppUNormBlock;
				case Format.PVRTC2_4Bpp_UNorm_Block:
					return SharpVulkan.Format.Pvrtc24BppUNormBlock;
				default:
					throw new ArgumentException(nameof(format));
			}
		}

		public static SharpVulkan.ShaderStageFlags GetVKShaderStageFlags(ShaderStageMask mask)
		{
			var result = SharpVulkan.ShaderStageFlags.None;
			if ((mask & ShaderStageMask.Vertex) != 0) {
				result |= SharpVulkan.ShaderStageFlags.Vertex;
			}
			if ((mask & ShaderStageMask.Fragment) != 0) {
				result |= SharpVulkan.ShaderStageFlags.Fragment;
			}
			return result;
		}

		public static SharpVulkan.IndexType GetVKIndexType(IndexFormat format)
		{
			switch (format) {
				case IndexFormat.Index16Bits:
					return SharpVulkan.IndexType.UInt16;
				default:
					throw new ArgumentException(nameof(format));
			}
		}

		public static SharpVulkan.PrimitiveTopology GetVKPrimitiveTopology(PrimitiveTopology topology)
		{
			switch (topology) {
				case PrimitiveTopology.TriangleList:
					return SharpVulkan.PrimitiveTopology.TriangleList;
				case PrimitiveTopology.TriangleStrip:
					return SharpVulkan.PrimitiveTopology.TriangleStrip;
				default:
					throw new ArgumentException(nameof(topology));
			}
		}

		public static SharpVulkan.ColorComponentFlags GetVKColorComponentFlags(ColorWriteMask mask)
		{
			var result = SharpVulkan.ColorComponentFlags.None;
			if ((mask & ColorWriteMask.Red) != 0) {
				result |= SharpVulkan.ColorComponentFlags.R;
			}
			if ((mask & ColorWriteMask.Green) != 0){
				result |= SharpVulkan.ColorComponentFlags.G;
			}
			if ((mask & ColorWriteMask.Blue) != 0) {
				result |= SharpVulkan.ColorComponentFlags.B;
			}
			if ((mask & ColorWriteMask.Alpha) != 0) {
				result |= SharpVulkan.ColorComponentFlags.A;
			}
			return result;
		}

		public static SharpVulkan.BlendOperation GetVKBlendOp(BlendFunc func)
		{
			switch (func) {
				case BlendFunc.Add:
					return SharpVulkan.BlendOperation.Add;
				case BlendFunc.Subtract:
					return SharpVulkan.BlendOperation.Subtract;
				case BlendFunc.ReverseSubtract:
					return SharpVulkan.BlendOperation.ReverseSubtract;
				default:
					throw new ArgumentException(nameof(func));
			}
		}

		public static SharpVulkan.BlendFactor GetVKBlendFactor(Blend blend)
		{
			switch (blend) {
				case Blend.One:
					return SharpVulkan.BlendFactor.One;
				case Blend.Zero:
					return SharpVulkan.BlendFactor.Zero;
				case Blend.Factor:
					return SharpVulkan.BlendFactor.ConstantColor;
				case Blend.SrcColor:
					return SharpVulkan.BlendFactor.SourceColor;
				case Blend.SrcAlpha:
					return SharpVulkan.BlendFactor.SourceAlpha;
				case Blend.SrcAlphaSaturation:
					return SharpVulkan.BlendFactor.SourceAlphaSaturate;
				case Blend.DstColor:
					return SharpVulkan.BlendFactor.DestinationColor;
				case Blend.DstAlpha:
					return SharpVulkan.BlendFactor.DestinationAlpha;
				case Blend.InverseFactor:
					return SharpVulkan.BlendFactor.OneMinusConstantColor;
				case Blend.InverseSrcColor:
					return SharpVulkan.BlendFactor.OneMinusSourceColor;
				case Blend.InverseSrcAlpha:
					return SharpVulkan.BlendFactor.OneMinusSourceAlpha;
				case Blend.InverseDstColor:
					return SharpVulkan.BlendFactor.OneMinusDestinationColor;
				case Blend.InverseDstAlpha:
					return SharpVulkan.BlendFactor.OneMinusDestinationAlpha;
				default:
					throw new ArgumentException(nameof(blend));
			}
		}

		public static SharpVulkan.CompareOperation GetVKCompareOp(CompareFunc func)
		{
			switch (func) {
				case CompareFunc.Always:
					return SharpVulkan.CompareOperation.Always;
				case CompareFunc.Never:
					return SharpVulkan.CompareOperation.Never;
				case CompareFunc.Less:
					return SharpVulkan.CompareOperation.Less;
				case CompareFunc.LessEqual:
					return SharpVulkan.CompareOperation.LessOrEqual;
				case CompareFunc.Greater:
					return SharpVulkan.CompareOperation.Greater;
				case CompareFunc.GreaterEqual:
					return SharpVulkan.CompareOperation.GreaterOrEqual;
				case CompareFunc.Equal:
					return SharpVulkan.CompareOperation.Equal;
				case CompareFunc.NotEqual:
					return SharpVulkan.CompareOperation.NotEqual;
				default:
					throw new ArgumentException(nameof(func));
			}
		}

		public static SharpVulkan.StencilOperation GetVKStencilOp(StencilOp op)
		{
			switch (op) {
				case StencilOp.Keep:
					return SharpVulkan.StencilOperation.Keep;
				case StencilOp.Zero:
					return SharpVulkan.StencilOperation.Zero;
				case StencilOp.Replace:
					return SharpVulkan.StencilOperation.Replace;
				case StencilOp.Invert:
					return SharpVulkan.StencilOperation.Invert;
				case StencilOp.Increment:
					return SharpVulkan.StencilOperation.IncrementAndWrap;
				case StencilOp.IncrementSaturation:
					return SharpVulkan.StencilOperation.IncrementAndClamp;
				case StencilOp.Decrement:
					return SharpVulkan.StencilOperation.DecrementAndWrap;
				case StencilOp.DecrementSaturation:
					return SharpVulkan.StencilOperation.DecrementAndClamp;
				default:
					throw new ArgumentException(nameof(op));
			}
		}

		public static SharpVulkan.CullModeFlags GetVKCullModeFlags(CullMode mode)
		{
			switch (mode) {
				case CullMode.None:
					return SharpVulkan.CullModeFlags.None;
				case CullMode.Back:
					return SharpVulkan.CullModeFlags.Back;
				case CullMode.Front:
					return SharpVulkan.CullModeFlags.Front;
				default:
					throw new ArgumentException(nameof(mode));
			}
		}

		public static SharpVulkan.FrontFace GetVKFrontFace(FrontFace face)
		{
			switch (face) {
				case FrontFace.CW:
					return SharpVulkan.FrontFace.Clockwise;
				case FrontFace.CCW:
					return SharpVulkan.FrontFace.CounterClockwise;
				default:
					throw new ArgumentException(nameof(face));
			}
		}
	}
}
