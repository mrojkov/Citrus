#if (MAC || WIN)
using System;
using System.IO;
using Lime.Graphics.Platform;

namespace Lime
{
	partial class Texture2D
	{
		enum DDSFourCC
		{
			DXT1 = ('D' | ('X' << 8) | ('T' << 16) | ('1' << 24)),
			DXT3 = ('D' | ('X' << 8) | ('T' << 16) | ('3' << 24)),
			DXT5 = ('D' | ('X' << 8) | ('T' << 16) | ('5' << 24)),
		}

		[Flags]
		enum DDSPFFlags
		{
			Alpha = 0x01,
			FourCC = 0x04,
			RGB = 0x40
		}

		const UInt32 DDSMagic = 0x20534444;

		private void InitWithDDSBitmap(BinaryReader reader)
		{
			UInt32 magic = reader.ReadUInt32();
			UInt32 size = reader.ReadUInt32();
			if (magic != DDSMagic || size != 124) {
				throw new InvalidDataException("Invalid DDS file header");
			}
			// UInt32 flags =
			reader.ReadUInt32();
			int height = reader.ReadInt32();
			int width = reader.ReadInt32();
			int pitchOrLinearSize = (int)reader.ReadUInt32();
			// UInt32 depth =
			reader.ReadUInt32();
			UInt32 mipMapCount = reader.ReadUInt32();
			reader.ReadBytes(11 * 4);
			// Read pixel format
			reader.ReadUInt32(); // Structure size (32 bytes)
			DDSPFFlags pfFlags = (DDSPFFlags)reader.ReadUInt32();
			UInt32 pfFourCC = reader.ReadUInt32();
			// UInt32 pfRGBBitCount =
			reader.ReadUInt32();
			// UInt32 pfRBitMask =
			reader.ReadUInt32();
			// UInt32 pfGBitMask =
			reader.ReadUInt32();
			// UInt32 pfBBitMask =
			reader.ReadUInt32();
			// UInt32 pfABitMask =
			reader.ReadUInt32();
			// read the rest of header
			// UInt32 caps =
			reader.ReadUInt32();
			// UInt32 caps2 =
			reader.ReadUInt32();
			// UInt32 caps3 =
			reader.ReadUInt32();
			// UInt32 caps4 =
			reader.ReadUInt32();
			// UInt32 reserved2 =
			reader.ReadUInt32();

			Format format;
			if ((pfFlags & DDSPFFlags.RGB) != 0) {
				format = Format.R8G8B8A8_UNorm;
			} else {
				switch ((DDSFourCC)pfFourCC) {
					case DDSFourCC.DXT1:
						format = Format.BC1_RGB_UNorm_Block;
						break;
					case DDSFourCC.DXT3:
						format = Format.BC2_UNorm_Block;
						break;
					case DDSFourCC.DXT5:
						format = Format.BC3_UNorm_Block;
						break;
					default:
						throw new InvalidDataException("Unsupported texture format");
				}
			}
			SurfaceSize = ImageSize = new Size(width, height);
			mipMapCount = 1;
			if (mipMapCount > 1 && mipMapCount != GraphicsUtility.CalculateMipLevelCount(width, height)) {
				throw new NotSupportedException();
			}
			Action deferredCommands = () => EnsurePlatformTexture(format, width, height, mipMapCount > 1);
			MemoryUsed = 0;
			for (int level = 0; level < mipMapCount; level++) {
				var levelCopy = level;
				var buffer = ReadTextureData(reader, GraphicsUtility.CalculateMipLevelDataSize(levelCopy, format, width, height));
				deferredCommands += () => platformTexture.SetData(levelCopy, buffer);
			}
			Window.Current.InvokeOnRendering(deferredCommands);
		}
	}
}
#endif
