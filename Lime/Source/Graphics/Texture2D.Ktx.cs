using System;
using System.IO;
using System.Runtime.InteropServices;
using Lime.Graphics.Platform;

namespace Lime
{
	partial class Texture2D
	{
		public const UInt32 KTXMagic = 0x58544BAB;

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
			var numberOfMipmapLevels = Math.Max(1, reader.ReadInt32());
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
			if (numberOfMipmapLevels > 1 && numberOfMipmapLevels != GraphicsUtility.CalculateMipLevelCount(pixelWidth, pixelHeight)) {
				throw new InvalidDataException();
			}
			var format = ConvertGLFormat(glInternalFormat, glBaseInternalFormat, glFormat, glType);
			var etc2Format =
				format == Format.ETC2_R8G8B8_UNorm_Block ||
				format == Format.ETC2_R8G8B8A1_UNorm_Block ||
				format == Format.ETC2_R8G8B8A8_UNorm_Block;
			SurfaceSize = ImageSize = new Size(pixelWidth, pixelHeight);
			Action deferredCommands = null;
			for (int level = 0; level < numberOfMipmapLevels; level++) {
				var levelCopy = level;
				var dataLength = reader.ReadInt32();
				var data = ReadTextureData(reader, dataLength);
				GraphicsUtility.CalculateMipLevelSize(levelCopy, pixelWidth, pixelHeight, out var levelWidth, out var levelHeight);
				MemoryUsed = 0;
				deferredCommands += () => {
					var formatFeatures = RenderContextManager.CurrentContext.GetFormatFeatures(format);
					if (etc2Format && (formatFeatures & FormatFeatures.Sample) == 0) {
						var rgba8Data = Marshal.AllocHGlobal(levelWidth * levelHeight * 4);
						try {
							Etc2Decoder.Decode(data, rgba8Data, levelWidth, levelHeight, glInternalFormat);
							EnsurePlatformTexture(Format.R8G8B8A8_UNorm, pixelWidth, pixelHeight, numberOfMipmapLevels > 1);
							platformTexture.SetData(levelCopy, rgba8Data);
						} finally {
							Marshal.FreeHGlobal(rgba8Data);
						}
					} else {
						EnsurePlatformTexture(format, pixelWidth, pixelHeight, numberOfMipmapLevels > 1);
						platformTexture.SetData(levelCopy, data);
					}
				};
			}
			Window.Current.InvokeOnRendering(deferredCommands);
		}

		private static Format ConvertGLFormat(int glInternalFormat, int glBaseInternalFormat, int glFormat, int glType)
		{
			const int GL_ETC1_RGB8_OES = 0x8D64;
			const int GL_COMPRESSED_RGB8_ETC2 = 0x9274;
			const int GL_COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2 = 0x9276;
			const int GL_COMPRESSED_RGBA8_ETC2_EAC = 0x9278;
			const int GL_RGB = 0x1907;
			const int GL_RGBA = 0x1908;
			const int GL_UNSIGNED_BYTE = 0x1401;
			const int GL_UNSIGNED_SHORT_4_4_4_4 = 0x8033;
			const int GL_UNSIGNED_SHORT_5_5_5_1 = 0x8034;
			const int GL_UNSIGNED_SHORT_5_6_5 = 0x8363;

			if (glInternalFormat == GL_ETC1_RGB8_OES) {
				return Format.ETC1_R8G8B8_UNorm_Block;
			}
			if (glInternalFormat == GL_COMPRESSED_RGB8_ETC2) {
				return Format.ETC2_R8G8B8_UNorm_Block;
			}
			if (glInternalFormat == GL_COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2) {
				return Format.ETC2_R8G8B8A1_UNorm_Block;
			}
			if (glInternalFormat == GL_COMPRESSED_RGBA8_ETC2_EAC) {
				return Format.ETC2_R8G8B8A8_UNorm_Block;
			}
			if (glBaseInternalFormat == GL_RGB && glFormat == GL_RGB && glType == GL_UNSIGNED_BYTE) {
				return Format.R8G8B8_UNorm;
			}
			if (glBaseInternalFormat == GL_RGBA && glFormat == GL_RGBA && glType == GL_UNSIGNED_BYTE) {
				return Format.R8G8B8A8_UNorm;
			}
			if (glBaseInternalFormat == GL_RGB && glFormat == GL_RGB && glType == GL_UNSIGNED_SHORT_5_6_5) {
				return Format.R5G6B5_UNorm_Pack16;
			}
			if (glBaseInternalFormat == GL_RGBA && glFormat == GL_RGBA && glType == GL_UNSIGNED_SHORT_5_5_5_1) {
				return Format.R5G5B5A1_UNorm_Pack16;
			}
			if (glBaseInternalFormat == GL_RGBA && glFormat == GL_RGBA && glType == GL_UNSIGNED_SHORT_4_4_4_4) {
				return Format.R4G4B4A4_UNorm_Pack16;
			}
			throw new NotSupportedException();
		}
	}
}
