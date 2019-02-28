using System;
using System.IO;
using Lime.Graphics.Platform;

namespace Lime
{
	public partial class Texture2D : CommonTexture, ITexture
	{
		// Values taken from PVRTexture.h from http://www.imgtec.com
		enum PVRTextureFlag {
			Mipmap			= (1<<8),		// has mip map levels
			Twiddle			= (1<<9),		// is twiddled
			FlagBumpmap		= (1<<10),		// has normals encoded for a bump map
			Tiling			= (1<<11),		// is bordered for tiled pvr
			Cubemap			= (1<<12),		// is a cubemap/skybox
			FalseMipCol		= (1<<13),		// are there false coloured MIP levels
			FlagVolume		= (1<<14),		// is this a volume texture
			FlagAlpha		= (1<<15),		// v2.1 is there transparency info in the texture
			VerticalFlip	= (1<<16),		// v2.1 is the texture vertically flipped
		}

		enum LegacyPVRFormat
		{
			RGBA_4444 = 0x0,
			RGBA_1555 = 0x1,
			RGB_565 = 0x2,
			RGB_555 = 0x3,
			RGB_888 = 0x4,
			ARGB_8888 = 0x5,
			PVRTC_2 = 0xC,
			PVRTC_4 = 0xD,	
			GLARGB_4444 = 0x10,
			GLARGB_8888 = 0x12,
			GLRGB_565 = 0x13,
		}

		enum PVRFormat : ulong
		{
			RGBA8888 = 'r' | ('g' << 8) | ('b' << 16) | ('a' << 24) | (8L << 32) | (8L << 40) | (8L << 48) | (8L << 56),
			RGBA4444 = 'r' | ('g' << 8) | ('b' << 16) | ('a' << 24) | (4L << 32) | (4L << 40) | (4L << 48) | (4L << 56),
			RGB565 = 'r' | ('g' << 8) | ('b' << 16) | (5L << 32) | (6L << 40) | (5L << 48),
			PVRTC_2_RGB = 0,
			PVRTC_2_RGBA = 1 << 32,
			PVRTC_4_RGB = 2 << 32,
			PVRTC_4_RGBA = 3 << 32,
			ETC1 = 6 << 32,
		}

		private UInt32 PVRMagic = 0x03525650;

		private void InitWithPVRTexture(BinaryReader reader)
		{
			var version = reader.ReadUInt32();
			if (version != PVRMagic) {
				throw new Exception("Invalid PVR header");
			}
			reader.ReadUInt32(); // flags
			var pvrFormat = (PVRFormat)reader.ReadUInt64();
			reader.ReadUInt32(); // color space
			reader.ReadUInt32(); // channel type
			var height = reader.ReadInt32();
			var width = reader.ReadInt32();
			reader.ReadUInt32(); // depth
			reader.ReadUInt32(); // num surfaces
			reader.ReadUInt32(); // num faces
			var numMipmaps = reader.ReadInt32();
			var metaDataSize = reader.ReadInt32();
			if (metaDataSize > 0) {
				reader.ReadChars(metaDataSize);
			}
			Format format;
			switch (pvrFormat) {
				case PVRFormat.ETC1:
					format = Format.ETC1_R8G8B8_UNorm_Block;
					break;
				case PVRFormat.PVRTC_2_RGBA:
					format = Format.PVRTC1_2Bpp_UNorm_Block;
					break;
				case PVRFormat.PVRTC_4_RGBA:
					format = Format.PVRTC1_4Bpp_UNorm_Block;
					break;
				case PVRFormat.RGB565:
					format = Format.R5G6B5_UNorm_Pack16;
					break;
				case PVRFormat.RGBA4444:
					format = Format.R4G4B4A4_UNorm_Pack16;
					break;
				case PVRFormat.RGBA8888:
					format = Format.R8G8B8A8_UNorm;
					break;
				default:
					throw new NotSupportedException();
			}
			SurfaceSize = ImageSize = new Size(width, height);
			if (numMipmaps > 1 && numMipmaps != GraphicsUtility.CalculateMipLevelCount(width, height)) {
				throw new NotSupportedException();
			}
			Action deferredCommands = () => EnsurePlatformTexture(format, width, height, numMipmaps > 1);
			MemoryUsed = 0;
			for (int level = 0; level < numMipmaps; level++) {
				var levelCopy = level;
				var buffer = ReadTextureData(reader, GraphicsUtility.CalculateMipLevelDataSize(levelCopy, format, width, height));
				deferredCommands += () => platformTexture.SetData(levelCopy, buffer);
			}
			Window.Current.InvokeOnRendering(deferredCommands);
		}
	}
}
