#if OPENGL || GLES11
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
#if iOS
using OpenTK.Graphics.ES11;
#endif

namespace Lime
{
	public partial class Texture2D : ITexture
	{
#if iOS
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

		enum PVRFormat
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

		private UInt32 PVRMagic = 52;

		private void InitWithPVRTexture(BinaryReader reader)
		{
			UInt32 headerLength = reader.ReadUInt32();
			if (headerLength != PVRMagic) {
				throw new Exception("Invalid PVRT header");
			}
			Int32 height = reader.ReadInt32();
			Int32 width = reader.ReadInt32();
			UInt32 numMipmaps = reader.ReadUInt32();
			Int32 flags = reader.ReadInt32();
			reader.ReadUInt32(); // dataLength
			reader.ReadUInt32(); // bpp
			reader.ReadUInt32(); // bitmaskRed
			reader.ReadUInt32(); // bnitmaskGreen
			reader.ReadUInt32(); // bitmaskBlue
			reader.ReadUInt32(); // bitmaskAlpha
			reader.ReadUInt32(); // UInt32 pvrTag
			reader.ReadUInt32(); // UInt32 numSurfs
			SurfaceSize = ImageSize = new Size(width, height);
			for (int i = 0; i <= numMipmaps; i++) {
				if (i > 0 && (width < 8 || height < 8)) {
					continue;
				}
				PVRFormat format = (PVRFormat)(flags & 0xFF);
				switch(format)	{
				case PVRFormat.PVRTC_4: {
					byte[] buffer = new byte[width * height * 4 / 8];
					reader.Read(buffer, 0, buffer.Length);
					GL.CompressedTexImage2D(All.Texture2D, i, All.CompressedRgbaPvrtc4Bppv1Img, width, height, 0, buffer.Length, buffer);
					break;
				}
				case PVRFormat.PVRTC_2: {
					byte[] buffer = new byte[width * height * 2 / 8];
					reader.Read(buffer, 0, buffer.Length);
					GL.CompressedTexImage2D(All.Texture2D, i, All.CompressedRgbaPvrtc2Bppv1Img, width, height, 0, buffer.Length, buffer);
					break;
				}
				case PVRFormat.GLARGB_4444: {
					byte[] buffer = new byte[width * height * 2];
					reader.Read(buffer, 0, buffer.Length);
					GL.TexImage2D(All.Texture2D, i, (int)All.Rgba, width, height, 0, All.Rgba, All.UnsignedShort4444, buffer);
					break;
				}
				case PVRFormat.GLRGB_565: {
					byte[] buffer = new byte[width * height * 2];
					reader.Read(buffer, 0, buffer.Length);
					GL.TexImage2D(All.Texture2D, i, (int)All.Rgb, width, height, 0, All.Rgb, All.UnsignedShort565, buffer);
					break;
				}
				case PVRFormat.GLARGB_8888: {
					byte[] buffer = new byte[width * height * 4];
					reader.Read(buffer, 0, buffer.Length);
					GL.TexImage2D(All.Texture2D, i, (int)All.Rgba, width, height, 0, All.Rgba, All.UnsignedByte, buffer);
					break;
				}
				default:
					throw new NotImplementedException();
				}
				width /= 2;
				height /= 2;
			}
		}
#endif
	}
}
#endif