using System;

namespace Lime
{
	public enum Format
	{
		Undefined,
		R8_SNorm,
		R8_SScaled,
		R8_UNorm,
		R8_UScaled,
		R8G8_SNorm,
		R8G8_SScaled,
		R8G8_UNorm,
		R8G8_UScaled,
		R8G8B8_SNorm,
		R8G8B8_SScaled,
		R8G8B8_UNorm,
		R8G8B8_UScaled,
		R8G8B8A8_SNorm,
		R8G8B8A8_SScaled,
		R8G8B8A8_UNorm,
		R8G8B8A8_UScaled,
		R16_SNorm,
		R16_SScaled,
		R16_UNorm,
		R16_UScaled,
		R16G16_SNorm,
		R16G16_SScaled,
		R16G16_UNorm,
		R16G16_UScaled,
		R16G16B16_SNorm,
		R16G16B16_SScaled,
		R16G16B16_UNorm,
		R16G16B16_UScaled,
		R16G16B16A16_SNorm,
		R16G16B16A16_SScaled,
		R16G16B16A16_UNorm,
		R16G16B16A16_UScaled,
		R32_SFloat,
		R32G32_SFloat,
		R32G32B32_SFloat,
		R32G32B32A32_SFloat,
		R5G6B5_UNorm_Pack16,
		R5G5B5A1_UNorm_Pack16,
		R4G4B4A4_UNorm_Pack16,
		BC1_RGB_UNorm_Block,
		BC1_RGBA_UNorm_Block,
		BC2_UNorm_Block,
		BC3_UNorm_Block,
		ETC1_R8G8B8_UNorm_Block,
		ETC2_R8G8B8_UNorm_Block,
		ETC2_R8G8B8A1_UNorm_Block,
		ETC2_R8G8B8A8_UNorm_Block,
		PVRTC1_2Bpp_UNorm_Block,
		PVRTC1_4Bpp_UNorm_Block,
		PVRTC2_2Bpp_UNorm_Block,
		PVRTC2_4Bpp_UNorm_Block
	}

	[Flags]
	public enum FormatFeatures
	{
		None = 0,
		Sample = 1 << 1,
		RenderTarget = 1 << 2,
		VertexBuffer = 1 << 3
	}

	public static class FormatExtensions
	{
		public static int GetSize(this Format format)
		{
			switch (format) {
				case Format.R8_SNorm:
				case Format.R8_SScaled:
				case Format.R8_UNorm:
				case Format.R8_UScaled:
					return 1;
				case Format.R8G8_SNorm:
				case Format.R8G8_SScaled:
				case Format.R8G8_UNorm:
				case Format.R8G8_UScaled:
					return 2;
				case Format.R8G8B8_SNorm:
				case Format.R8G8B8_SScaled:
				case Format.R8G8B8_UNorm:
				case Format.R8G8B8_UScaled:
					return 3;
				case Format.R8G8B8A8_SNorm:
				case Format.R8G8B8A8_SScaled:
				case Format.R8G8B8A8_UNorm:
				case Format.R8G8B8A8_UScaled:
					return 4;
				case Format.R16_SNorm:
				case Format.R16_SScaled:
				case Format.R16_UNorm:
				case Format.R16_UScaled:
					return 2;
				case Format.R16G16_SNorm:
				case Format.R16G16_SScaled:
				case Format.R16G16_UNorm:
				case Format.R16G16_UScaled:
					return 4;
				case Format.R16G16B16_SNorm:
				case Format.R16G16B16_SScaled:
				case Format.R16G16B16_UNorm:
				case Format.R16G16B16_UScaled:
					return 6;
				case Format.R16G16B16A16_SNorm:
				case Format.R16G16B16A16_SScaled:
				case Format.R16G16B16A16_UNorm:
				case Format.R16G16B16A16_UScaled:
					return 8;
				case Format.R32_SFloat:
					return 4;
				case Format.R32G32_SFloat:
					return 8;
				case Format.R32G32B32_SFloat:
					return 12;
				case Format.R32G32B32A32_SFloat:
					return 16;
				case Format.R5G6B5_UNorm_Pack16:
				case Format.R5G5B5A1_UNorm_Pack16:
				case Format.R4G4B4A4_UNorm_Pack16:
					return 2;
				case Format.BC1_RGB_UNorm_Block:
				case Format.BC1_RGBA_UNorm_Block:
					return 8;
				case Format.BC2_UNorm_Block:
				case Format.BC3_UNorm_Block:
					return 16;
				case Format.ETC1_R8G8B8_UNorm_Block:
				case Format.ETC2_R8G8B8_UNorm_Block:
				case Format.ETC2_R8G8B8A1_UNorm_Block:
					return 8;
				case Format.ETC2_R8G8B8A8_UNorm_Block:
					return 16;
				case Format.PVRTC1_2Bpp_UNorm_Block:
				case Format.PVRTC1_4Bpp_UNorm_Block:
					return 32;
				case Format.PVRTC2_2Bpp_UNorm_Block:
				case Format.PVRTC2_4Bpp_UNorm_Block:
					return 8;
				default:
					throw new ArgumentException(nameof(format));
			}
		}

		public static void GetBlockSize(this Format format, out int width, out int height)
		{
			switch (format) {
				case Format.BC1_RGB_UNorm_Block:
				case Format.BC1_RGBA_UNorm_Block:
				case Format.BC2_UNorm_Block:
				case Format.BC3_UNorm_Block:
				case Format.ETC1_R8G8B8_UNorm_Block:
				case Format.ETC2_R8G8B8_UNorm_Block:
				case Format.ETC2_R8G8B8A1_UNorm_Block:
				case Format.ETC2_R8G8B8A8_UNorm_Block:
					width = height = 4;
					break;
				case Format.PVRTC1_2Bpp_UNorm_Block:
					width = 16;
					height = 8;
					break;
				case Format.PVRTC1_4Bpp_UNorm_Block:
					width = 8;
					height = 8;
					break;
				case Format.PVRTC2_2Bpp_UNorm_Block:
					width = 8;
					height = 4;
					break;
				case Format.PVRTC2_4Bpp_UNorm_Block:
					width = 4;
					height = 4;
					break;
				default:
					width = height = 1;
					break;
			}
		}

		public static bool IsCompressed(this Format format)
		{
			switch (format) {
				case Format.BC1_RGB_UNorm_Block:
				case Format.BC1_RGBA_UNorm_Block:
				case Format.BC2_UNorm_Block:
				case Format.BC3_UNorm_Block:
				case Format.ETC1_R8G8B8_UNorm_Block:
				case Format.ETC2_R8G8B8_UNorm_Block:
				case Format.ETC2_R8G8B8A1_UNorm_Block:
				case Format.ETC2_R8G8B8A8_UNorm_Block:
				case Format.PVRTC1_2Bpp_UNorm_Block:
				case Format.PVRTC1_4Bpp_UNorm_Block:
				case Format.PVRTC2_2Bpp_UNorm_Block:
				case Format.PVRTC2_4Bpp_UNorm_Block:
					return true;
				default:
					return false;
			}
		}
	}
}
