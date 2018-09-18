#if OPENGL
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif

#pragma warning disable 0618

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
			var pixelFormat = (PVRFormat)reader.ReadUInt64();
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
			SurfaceSize = ImageSize = new Size(width, height);
			Action glCommands = PrepareOpenGLTexture;
			MemoryUsed = 0;
			for (int i = 0; i < numMipmaps; i++) {
				if (i > 0 && (width < 8 || height < 8)) {
					continue;
				}
				// Cloning variables to prevent wrong capturing
				int mipLevel = i;
				int width2 = width;
				int height2 = height;
				switch(pixelFormat)	{
#if iOS
				case PVRFormat.PVRTC_4_RGBA: {
					var buffer = ReadTextureData(reader, width * height * 4 / 8);
					glCommands += () => {
						GL.ActiveTexture(TextureUnit.Texture0);
						GL.BindTexture(TextureTarget.Texture2D, handle);
						GL.CompressedTexImage2D(All.Texture2D, mipLevel, All.CompressedRgbaPvrtc4Bppv1Img, width2, height2, 0, buffer.Length, buffer);
						PlatformRenderer.MarkTextureSlotAsDirty(0);
						PlatformRenderer.CheckErrors();
					};
					break;
				}
				case PVRFormat.PVRTC_2_RGBA: {
					if (i > 0 && height < 16) {
						continue;
					}				
					var buffer = ReadTextureData(reader, width * height * 2 / 8);
					glCommands += () => {
						GL.ActiveTexture(TextureUnit.Texture0);
						GL.BindTexture(TextureTarget.Texture2D, handle);
						GL.CompressedTexImage2D(All.Texture2D, mipLevel, All.CompressedRgbaPvrtc2Bppv1Img, width2, height2, 0, buffer.Length, buffer);
						PlatformRenderer.MarkTextureSlotAsDirty(0);
						PlatformRenderer.CheckErrors();
					};
					break;
				}
#elif ANDROID
				case PVRFormat.ETC1: {
					var buffer = ReadTextureData(reader, width * height * 4 / 8);
					glCommands += () => {
						GL.ActiveTexture(TextureUnit.Texture0);
						GL.BindTexture(TextureTarget.Texture2D, handle);
						GL.CompressedTexImage2D(All.Texture2D, mipLevel, All.Etc1Rgb8Oes, width2, height2, 0, buffer.Length, buffer);
						PlatformRenderer.MarkTextureSlotAsDirty(0);
						PlatformRenderer.CheckErrors();
					};
					break;
				}
#endif
				case PVRFormat.RGBA4444: {
					var buffer = ReadTextureData(reader, width * height * 2);
					glCommands += () => {
						GL.ActiveTexture(TextureUnit.Texture0);
						GL.BindTexture(TextureTarget.Texture2D, handle);
						GL.TexImage2D(TextureTarget.Texture2D, mipLevel, PixelInternalFormat.Rgba, width2, height2, 0, PixelFormat.Rgba, PixelType.UnsignedShort4444, buffer);
						PlatformRenderer.MarkTextureSlotAsDirty(0);
						PlatformRenderer.CheckErrors();
					};
					break;
				}
				case PVRFormat.RGB565: {
					var buffer = ReadTextureData(reader, width * height * 2);
					glCommands += () => {
						GL.ActiveTexture(TextureUnit.Texture0);
						GL.BindTexture(TextureTarget.Texture2D, handle);
						GL.TexImage2D(TextureTarget.Texture2D, mipLevel, PixelInternalFormat.Rgb, width2, height2, 0, PixelFormat.Rgb, PixelType.UnsignedShort565, buffer);
						PlatformRenderer.MarkTextureSlotAsDirty(0);
						PlatformRenderer.CheckErrors();
					};
					break;
				}
				case PVRFormat.RGBA8888: {
					var buffer = ReadTextureData(reader, width * height * 4);
					glCommands += () => {
						GL.ActiveTexture(TextureUnit.Texture0);
						GL.BindTexture(TextureTarget.Texture2D, handle);
						GL.TexImage2D(TextureTarget.Texture2D, mipLevel, PixelInternalFormat.Rgba, width2, height2, 0, PixelFormat.Rgba, PixelType.UnsignedByte, buffer);
						PlatformRenderer.MarkTextureSlotAsDirty(0);
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
			Window.Current.InvokeOnRendering(glCommands);
		}
	}
}
#endif