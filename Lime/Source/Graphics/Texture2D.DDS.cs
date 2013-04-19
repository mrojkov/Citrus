using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lime
{
	public partial class Texture2D : ITexture
	{
#if WIN
		enum DDSFourCC
		{
			DXT1 = ('D' | ('X' << 8) | ('T' << 16) | ('1' << 24)),
			DXT3 = ('D' | ('X' << 8) | ('T' << 16) | ('3' << 24)),
			DXT5 = ('D' | ('X' << 8) | ('T' << 16) | ('5' << 24)),
		}

		enum DDSPFFlags
		{
			Apha = 0x01,
			FourCC = 0x04,
			RGB = 0x40
		}

		const UInt32 DDSMagic = 0x20534444;

		private void InitWithDDSBitmap(BinaryReader reader)
		{
			UInt32 magic = reader.ReadUInt32();
			UInt32 size = reader.ReadUInt32();
			if (magic != DDSMagic || size != 124) {
				throw new Lime.Exception("Invalid DDS file header");
			}
			// UInt32 flags =
			reader.ReadUInt32();
			int height = reader.ReadInt32();
			int width = reader.ReadInt32();
			UInt32 pitchOrLinearSize = reader.ReadUInt32();
			// UInt32 depth =
			reader.ReadUInt32();
			UInt32 mipMapCount = reader.ReadUInt32();
			reader.ReadBytes(11 * 4);
			// Read pixel format
			// UInt32 pfSize =
			reader.ReadUInt32();
			UInt32 pfFlags = reader.ReadUInt32();
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

			if ((pfFlags & (UInt32)DDSPFFlags.FourCC) == 0) {
				throw new Lime.Exception("Only compressed DDS textures are supported");
			}

			SurfaceSize = ImageSize = new Size(width, height);
			mipMapCount = 1;
			for (int i = 0; i < mipMapCount; i++) {
				if (width < 8 || height < 8) {
					break;
				}
				PixelInternalFormat pf;
				switch ((DDSFourCC)pfFourCC) {
					case DDSFourCC.DXT1:
						pf = PixelInternalFormat.CompressedRgbS3tcDxt1Ext;
						break;
					case DDSFourCC.DXT3:
						pf = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
						break;
					case DDSFourCC.DXT5:
						pf = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
						break;
					default:
						throw new Lime.Exception("Unsupported texture format");
				}
				byte[] buffer = new byte[pitchOrLinearSize];
				reader.Read(buffer, 0, buffer.Length);
				OGL.CompressedTexImage2D(TextureTarget.Texture2D, 0, pf, width, height, 0, buffer.Length, buffer);
				pitchOrLinearSize /= 4;
				width /= 2;
				height /= 2;
				Renderer.CheckErrors();
			}
		}
#endif
	}
}
