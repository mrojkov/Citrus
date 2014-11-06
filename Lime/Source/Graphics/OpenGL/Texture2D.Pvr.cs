#if (iOS || ANDROID) && OPENGL
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using OpenTK.Graphics.ES20;

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

		private UInt32 LegacyPVRMagic = 52;
		private UInt32 PVRMagic = 0x03525650;

		private void InitWithLegacyPVRTexture(BinaryReader reader)
		{
			UInt32 headerLength = reader.ReadUInt32();
			if (headerLength != LegacyPVRMagic) {
				throw new Exception("Invalid PVR header");
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
			Action glCommands = PrepareOpenGLTexture;
			MemoryUsed = 0;
			for (int i = 0; i <= numMipmaps; i++) {
				if (i > 0 && (width < 8 || height < 8)) {
					continue;
				}
				// Cloning variables to prevent wrong capturing
				int mipLevel = i;
				int width2 = width;
				int height2 = height;
				var format = (LegacyPVRFormat)(flags & 0xFF);
				switch(format)	{
				case LegacyPVRFormat.PVRTC_4: {
					var buffer = ReadTextureData(reader, width * height * 4 / 8);
					glCommands += () => {
						GL.CompressedTexImage2D(All.Texture2D, mipLevel, All.CompressedRgbaPvrtc4Bppv1Img, width2, height2, 0, buffer.Length, buffer);
						PlatformRenderer.CheckErrors();
					};
					break;
				}
				case LegacyPVRFormat.PVRTC_2: {
					var buffer = ReadTextureData(reader, width * height * 2 / 8);
					glCommands += () => {
						GL.CompressedTexImage2D(All.Texture2D, mipLevel, All.CompressedRgbaPvrtc2Bppv1Img, width2, height2, 0, buffer.Length, buffer);
						PlatformRenderer.CheckErrors();
					};
					break;
				}
				case LegacyPVRFormat.GLARGB_4444: {
					var buffer = ReadTextureData(reader, width * height * 2);
					glCommands += () => {
						GL.TexImage2D(All.Texture2D, mipLevel, (int)All.Rgba, width2, height2, 0, All.Rgba, All.UnsignedShort4444, buffer);
						PlatformRenderer.CheckErrors();
					};
					break;
				}
				case LegacyPVRFormat.GLRGB_565: {
					var buffer = ReadTextureData(reader, width * height * 2);
					glCommands += () => {
						GL.TexImage2D(All.Texture2D, mipLevel, (int)All.Rgb, width2, height2, 0, All.Rgb, All.UnsignedShort565, buffer);
						PlatformRenderer.CheckErrors();
					};
					break;
				}
				case LegacyPVRFormat.GLARGB_8888: {
					var buffer = ReadTextureData(reader, width * height * 4);
					glCommands += () => {
						GL.TexImage2D(All.Texture2D, mipLevel, (int)All.Rgba, width2, height2, 0, All.Rgba, All.UnsignedByte, buffer);
						PlatformRenderer.CheckErrors();
					};
					break;
				}
				default:
					throw new NotImplementedException();
				}
				width /= 2;
				height /= 2;
			}
			Application.InvokeOnMainThread(glCommands);
		}

		private void InitWithPVRTexture(BinaryReader reader)
		{
			var version = reader.ReadUInt32();
			if (version != PVRMagic) {
				throw new Exception("Invalid PVR header");
			}
			var flags = reader.ReadUInt32();
			var pixelFormat = (PVRFormat)reader.ReadUInt64();
			var colorSpace = reader.ReadUInt32();
			var channelType = reader.ReadUInt32();
			var height = reader.ReadInt32();
			var width = reader.ReadInt32();
			var depth = reader.ReadUInt32();
			var numSurfaces = reader.ReadUInt32();
			var numFaces = reader.ReadUInt32();
			var numMipmaps = reader.ReadInt32();
			var metaDataSize = reader.ReadInt32();
			if (metaDataSize > 0) {
				reader.ReadChars(metaDataSize);
			}
			SurfaceSize = ImageSize = new Size(width, height);
			Action glCommands = PrepareOpenGLTexture;
			MemoryUsed = 0;
			for (int i = 0; i < numMipmaps; i++) {
				if (i > 0 && (width < 4 || height < 4)) {
					continue;
				}
				// Cloning variables to prevent wrong capturing
				int mipLevel = i;
				int width2 = width;
				int height2 = height;
				switch(pixelFormat)	{
				case PVRFormat.PVRTC_4_RGBA: {
					var buffer = ReadTextureData(reader, width * height * 4 / 8);
					glCommands += () => {
						GL.CompressedTexImage2D(All.Texture2D, mipLevel, All.CompressedRgbaPvrtc4Bppv1Img, width2, height2, 0, buffer.Length, buffer);
						PlatformRenderer.CheckErrors();
					};
					break;
				}
				case PVRFormat.PVRTC_2_RGBA: {
					var buffer = ReadTextureData(reader, width * height * 2 / 8);
					glCommands += () => {
						GL.CompressedTexImage2D(All.Texture2D, mipLevel, All.CompressedRgbaPvrtc2Bppv1Img, width2, height2, 0, buffer.Length, buffer);
						PlatformRenderer.CheckErrors();
					};
					break;
				}
#if ANDROID
				case PVRFormat.ETC1: {
					var buffer = ReadTextureData(reader, width * height * 4 / 8);
					glCommands += () => {
						GL.CompressedTexImage2D(All.Texture2D, mipLevel, All.Etc1Rgb8Oes, width2, height2, 0, buffer.Length, buffer);
						PlatformRenderer.CheckErrors();
					};
					break;
				}
#endif
				case PVRFormat.RGBA4444: {
					var buffer = ReadTextureData(reader, width * height * 2);
					glCommands += () => {
						GL.TexImage2D(All.Texture2D, mipLevel, (int)All.Rgba, width2, height2, 0, All.Rgba, All.UnsignedShort4444, buffer);
						PlatformRenderer.CheckErrors();
					};
					break;
				}
				case PVRFormat.RGB565: {
					var buffer = ReadTextureData(reader, width * height * 2);
					glCommands += () => {
						GL.TexImage2D(All.Texture2D, mipLevel, (int)All.Rgb, width2, height2, 0, All.Rgb, All.UnsignedShort565, buffer);
						PlatformRenderer.CheckErrors();
					};
					break;
				}
				case PVRFormat.RGBA8888: {
					var buffer = ReadTextureData(reader, width * height * 4);
					glCommands += () => {
						GL.TexImage2D(All.Texture2D, mipLevel, (int)All.Rgba, width2, height2, 0, All.Rgba, All.UnsignedByte, buffer);
						PlatformRenderer.CheckErrors();
					};
					break;
				}
				default:
					throw new NotImplementedException();
				}
				width /= 2;
				height /= 2;
			}
			Application.InvokeOnMainThread(glCommands);
		}
	}
}
#endif