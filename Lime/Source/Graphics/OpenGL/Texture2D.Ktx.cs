#if OPENGL
using System;
using System.IO;
using System.Runtime.InteropServices;

#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace Lime
{
	public partial class Texture2D : CommonTexture, ITexture
	{
		const UInt32 KTXMagic = 0x58544BAB;

		static bool etc2Checked;
		static bool etc2Supported;

		private void InitWithKTXTexture(BinaryReader reader)
		{
			var identifier = reader.ReadBytes(12);
			if (identifier[1] != 'K' || identifier[2] != 'T' || identifier[3] != 'X') {
				throw new InvalidDataException("Invalid KTX header");
			}
			var endiannes = reader.ReadUInt32();
			if (endiannes != 0x04030201) {
				throw new InvalidDataException("Unsupported endiannes");
			}
			var glType = reader.ReadInt32();
			var glTypeSize = reader.ReadInt32();
			var glFormat = reader.ReadInt32();
			var glInternalFormat = reader.ReadInt32();
			var glBaseInternalFormat = reader.ReadInt32();
			var pixelWidth = reader.ReadInt32();
			var pixelHeight = reader.ReadInt32();
			var pixelDepth = reader.ReadInt32();
			var numberOfArrayElements = reader.ReadInt32();
			var numberOfFaces = reader.ReadInt32();
			var numberOfMipmapLevels = reader.ReadInt32();
			var bytesOfKeyValueData = reader.ReadInt32();
			reader.ReadBytes(bytesOfKeyValueData);
			if (numberOfArrayElements != 0) {
				throw new InvalidDataException("Array textures are not supported");
			}
			if (numberOfFaces != 1) {
				throw new InvalidDataException("Cubemap textures are not supported");
			}
			if (pixelDepth != 0) {
				throw new InvalidDataException("3D Textures are not supported");
			}
			if ((pixelWidth & 3) != 0 || (pixelHeight & 3) != 0) {
				throw new InvalidDataException("Texture dimensions should multiple of 4");
			}
			SurfaceSize = ImageSize = new Size(pixelWidth, pixelHeight);
			Action glCommands = PrepareOpenGLTexture;
			PrepareOpenGLTexture();
			for (int i = 0; i < Math.Max(1, numberOfMipmapLevels); i++) {
				var dataLength = reader.ReadInt32();
				MemoryUsed = 0;
				if (i > 0 && (pixelWidth < 8 || pixelHeight < 8)) {
					continue;
				}
				// Copy variables because they will be captured.
				int mipLevel = i;
				int width = pixelWidth;
				int height = pixelHeight;
				if (glFormat == 0) {
					var data = ReadTextureData(reader, dataLength);
					glCommands += () => {
						if (!etc2Checked) {
							etc2Checked = true;
							if (PlatformRenderer.GetGLESMajorVersion() >= 3) {
								etc2Supported = true;
							} else {
								var ext = GL.GetString(StringName.Extensions);
								etc2Supported = ext?.Contains("ETC2_RGBA8") ?? false;
							}
							Debug.Write(etc2Supported ? "ETC2 textures supported." : "ETC2 textures not supported.");
						}
						PlatformRenderer.PushTexture(handle, 0);
						if (etc2Supported || glInternalFormat == (int)All.Etc1Rgb8Oes) {
							GL.CompressedTexImage2D(
								TextureTarget.Texture2D, mipLevel, (PixelInternalFormat)glInternalFormat, 
								width, height, 0, dataLength, data);
						} else {
							var rgba8Data = Marshal.AllocHGlobal(width * height * 4);
							Etc2Decoder.Decode(data, rgba8Data, width, height, glInternalFormat);
							GL.TexImage2D(TextureTarget.Texture2D, mipLevel, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, rgba8Data);
							Marshal.FreeHGlobal(rgba8Data);
						}
						PlatformRenderer.PopTexture(0);
						PlatformRenderer.CheckErrors();
					};
				} else {
					var data = ReadTextureData(reader, dataLength);
					glCommands += () => {
						PlatformRenderer.PushTexture(handle, 0);
						GL.TexImage2D(
							TextureTarget.Texture2D, mipLevel, (PixelInternalFormat)glInternalFormat, 
							width, height, 0, (PixelFormat)glFormat, (PixelType)glType, data);
						PlatformRenderer.PopTexture(0);
						PlatformRenderer.CheckErrors();
					};
				}
				pixelWidth /= 2;
				pixelHeight /= 2;
			}
			Application.InvokeOnMainThread(glCommands);
		}
	}
}
#endif