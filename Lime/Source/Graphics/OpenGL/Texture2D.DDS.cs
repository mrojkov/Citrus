#if OPENGL || GLES11
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#if WIN
using OpenTK.Graphics.OpenGL;
#elif MAC
using MonoMac.OpenGL;
using OGL = MonoMac.OpenGL.GL;
#endif

namespace Lime
{
	public partial class Texture2D : ITexture
	{
#if WIN || MAC
		enum DDSFourCC
		{
			DXT1 = ('D' | ('X' << 8) | ('T' << 16) | ('1' << 24)),
			DXT3 = ('D' | ('X' << 8) | ('T' << 16) | ('3' << 24)),
			DXT5 = ('D' | ('X' << 8) | ('T' << 16) | ('5' << 24)),
		}

		[Flags]
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

			SurfaceSize = ImageSize = new Size(width, height);
			mipMapCount = 1;
			for (int level = 0; level < mipMapCount; level++) {
				if (width < 8 || height < 8) {
					break;
				}
				if ((pfFlags & DDSPFFlags.RGB) != 0) {
					ReadRGBAImage(reader, level, width, height, pitchOrLinearSize);
				} else if ((pfFlags & DDSPFFlags.FourCC) != 0) {
					ReadCompressedImage(reader, level, width, height, pitchOrLinearSize, pfFourCC);
				} else {
					throw new Lime.Exception("Error reading DDS");
				}
				pitchOrLinearSize /= 4;
				width /= 2;
				height /= 2;
			}
		}

		private static void ReadRGBAImage(BinaryReader reader, int level, int width, int height, uint pitchOrLinearSize)
		{
			if (pitchOrLinearSize != width * 4) {
				throw new Lime.Exception("Error reading RGBA texture. Must be 32 bit rgba");
			}
			byte[] buffer = new byte[pitchOrLinearSize * height];
			reader.Read(buffer, 0, buffer.Length);
			Application.InvokeOnMainThread(() => {
				OGL.TexImage2D(TextureTarget.Texture2D, level, PixelInternalFormat.Rgba8, width, height, 0,
					PixelFormat.Bgra, PixelType.UnsignedByte, buffer);
				Renderer.CheckErrors();
			});
		}

		private static void ReadCompressedImage(BinaryReader reader, int level, int width, int height, UInt32 linearSize, UInt32 pfFourCC)
		{
			PixelInternalFormat pif;
			switch ((DDSFourCC)pfFourCC) {
				case DDSFourCC.DXT1:
					pif = PixelInternalFormat.CompressedRgbS3tcDxt1Ext;
					break;
				case DDSFourCC.DXT3:
					pif = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
					break;
				case DDSFourCC.DXT5:
					pif = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
					break;
				default:
					throw new Lime.Exception("Unsupported texture format");
			}
			byte[] buffer = new byte[linearSize];
			reader.Read(buffer, 0, buffer.Length);
			Application.InvokeOnMainThread(() => {
				OGL.CompressedTexImage2D(TextureTarget.Texture2D, level, pif, width, height, 0, buffer.Length, buffer);
				Renderer.CheckErrors();
			});
		}
#endif
	}
}
#endif