using System;
using System.IO;
using System.Collections.Generic;
#if iOS
using MonoTouch.UIKit;
using OpenTK.Graphics.ES11;
#elif MAC
using MonoMac.OpenGL;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace Lime
{
#if MAC
	using OGL = GL;
#endif
	/// <summary>
	/// Represents 2D texture.
	/// </summary>
	public class PlainTexture : ITexture
	{
		uint id;
		public Size ImageSize { get; protected set; }
		public Size SurfaceSize { get; protected set; }
		Rectangle uvRect;

		public static List<uint> TexturesToDelete = new List<uint>();

		public static void DeleteScheduledTextures()
		{
			lock (TexturesToDelete) {
				if (TexturesToDelete.Count > 0) {
					var ids = new uint[TexturesToDelete.Count];
					TexturesToDelete.CopyTo(ids);
#if GLES11
					GL.DeleteTextures(ids.Length, ids);
#else
					OGL.DeleteTextures(ids.Length, ids);
#endif
					TexturesToDelete.Clear();
					Renderer.CheckErrors();
				}
			}
		}

		public string SerializationPath {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}

		public Rectangle UVRect { get { return uvRect; } }

		public bool ChessTexture { get { return false; } }

		public void LoadImage(string path)
		{
			using (Stream stream = PackedAssetsBundle.Instance.OpenFileLocalized(path)) {
				LoadImage(stream);
			}
		}

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
		
		private void InitWithPVRTexture(BinaryReader reader)
		{
			UInt32 headerLength = reader.ReadUInt32();
			if (headerLength != 52) {
				throw new Exception("Invalid PVRT header");
			}
			Int32 height = reader.ReadInt32();
			Int32 width = reader.ReadInt32();
			UInt32 numMipmaps = reader.ReadUInt32();
			Int32 flags = reader.ReadInt32();
			/* UInt32 dataLength = */ reader.ReadUInt32();
			/* UInt32 bpp = */ reader.ReadUInt32();
			/* UInt32 bitmaskRed = */ reader.ReadUInt32();
			/* UInt32 bnitmaskGreen = */ reader.ReadUInt32();
			/* UInt32 bitmaskBlue = */ reader.ReadUInt32();
			/* UInt32 bitmaskAlpha = */ reader.ReadUInt32();
			/* UInt32 pvrTag = */ reader.ReadUInt32();
			/* UInt32 numSurfs = */ reader.ReadUInt32();
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
#else
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

		private void InitWithDDSBitmap(BinaryReader reader)
		{
			UInt32 magic = reader.ReadUInt32();
			UInt32 size = reader.ReadUInt32();
			if (magic != 0x20534444 || size != 124) {
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
				switch((DDSFourCC)pfFourCC) {
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

		private void InitWithPNGBitmap(BinaryReader reader)
		{
            var bitmap = new System.Drawing.Bitmap(reader.BaseStream);
			SurfaceSize = ImageSize = new Size(bitmap.Width, bitmap.Height);
			var lockRect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
			var lockMode = System.Drawing.Imaging.ImageLockMode.ReadOnly;
			if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb) {
				var data = bitmap.LockBits(lockRect, lockMode, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
				OGL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, data.Width, data.Height, 0,
					PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
				bitmap.UnlockBits(data);
			} else {
				var data = bitmap.LockBits(lockRect, lockMode, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
				OGL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
					PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
				bitmap.UnlockBits(data);
			}
			Renderer.CheckErrors();
		}
#endif

		public void LoadImage(Stream stream)
		{
			// Discards current texture.
			Dispose();
			DeleteScheduledTextures();

#if GLES11
			// Generate a new texture.
			GL.GenTextures(1, ref id);
			Renderer.SetTexture(id, 0);
			GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Linear);
			GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Linear);
			GL.TexParameter(All.Texture2D, All.TextureWrapS, (int)All.ClampToEdge);
			GL.TexParameter(All.Texture2D, All.TextureWrapT, (int)All.ClampToEdge);
			GL.Hint(All.PerspectiveCorrectionHint, All.Fastest);
#else
			// Generate a new texture.
			id = (uint)OGL.GenTexture();
			Renderer.SetTexture(id, 0);
			OGL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			OGL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			OGL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToEdge);
			OGL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToEdge);
			OGL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);
#endif

			using (BinaryReader reader = new BinaryReader(stream)) {
#if iOS
				InitWithPVRTexture(reader);
#else
				if (stream.CanSeek) {
					int sign = reader.PeekChar();
					stream.Seek(0, SeekOrigin.Begin);
					if (sign == 'D') {
						InitWithDDSBitmap(reader);
					} else if (sign == 137) {
						InitWithPNGBitmap(reader);
					} else {
						throw new Lime.Exception("Texture format must be either DDS or PNG");
					}
				} else {
					InitWithDDSBitmap(reader);
				}
#endif
			}
			Renderer.CheckErrors();
			uvRect = new Rectangle(Vector2.Zero, (Vector2)ImageSize / (Vector2)SurfaceSize);
		}

		/// <summary>
		/// Create texture from pixel array.
		/// </summary>
		public void LoadImage(Color4[] pixels, int width, int height, bool generateMips)
		{
			// Discards current texture.
			if (width != ImageSize.Width || height != ImageSize.Height) {
				Dispose();
				DeleteScheduledTextures();
				// Generate a new texture id.
#if GLES11
				GL.GenTextures(1, ref id);
#else
				id = (uint)OGL.GenTexture();
#endif
			}
#if GLES11
			Renderer.SetTexture(id, 0);
			GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Linear);
			GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Linear);
			GL.TexParameter(All.Texture2D, All.TextureWrapS, (int)All.ClampToEdge);
			GL.TexParameter(All.Texture2D, All.TextureWrapT, (int)All.ClampToEdge);
			GL.Hint(All.PerspectiveCorrectionHint, All.Fastest);
			GL.TexImage2D(All.Texture2D, 0, (int)All.Rgba, width, height, 0,
				All.Rgba, All.UnsignedByte, pixels);
#else
			Renderer.SetTexture(id, 0);
			OGL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			OGL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			OGL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToEdge);
			OGL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToEdge);
			OGL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);
			OGL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
				PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
			if (generateMips) {
#if WIN
				OGL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
#else
				Console.WriteLine("WARNING: Mipmap generation is not implemented for this platform");
#endif
			}
#endif
			Renderer.CheckErrors();

			ImageSize = new Size(width, height);
			SurfaceSize = ImageSize;
			uvRect = new Rectangle(0, 0, 1, 1);
		}

		~PlainTexture()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (id != 0) {
				lock (TexturesToDelete) {
					TexturesToDelete.Add(id);
				}
				id = 0;
			}
		}

		/// <summary>
		/// Returns native texture handle.
		/// </summary>
		/// <returns></returns>
		public uint GetHandle()
		{
			return id;
		}

		/// <summary>
		/// Sets texture as a render target.
		/// </summary>
		public void SetAsRenderTarget()
		{
		}

		/// <summary>
		/// Restores default render target(backbuffer).
		/// </summary>
		public void RestoreRenderTarget()
		{
		}

		/// <summary>
		/// Checks pixel transparency at given coordinates.
		/// </summary>
		/// <param name="x">x-coordinate of pixel</param>
		/// <param name="y">y-coordinate of pixel</param>
		/// <returns></returns>
		public bool IsTransparentPixel(int x, int y)
		{
			return false;
		}
	}
}