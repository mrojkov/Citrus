#if OPENGL
using System;
using System.IO;
using System.Collections.Generic;
#if iOS
using OpenTK.Graphics.ES20;
#elif MAC
using MonoMac.OpenGL;
#elif WIN
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
#endif

namespace Lime
{
	/// <summary>
	/// Represents 2D texture
	/// </summary>
	public partial class Texture2D : CommonTexture, ITexture
	{
		uint handle;
		public OpacityMask OpacityMask { get; private set; }
		public Size ImageSize { get; protected set; }
		public Size SurfaceSize { get; protected set; }
		Rectangle uvRect;

		public static List<uint> TexturesToDelete = new List<uint>();
		public static List<uint> FramebuffersToDelete = new List<uint>();

		public static void DeleteScheduledTextures()
		{
			lock (TexturesToDelete) {
				if (TexturesToDelete.Count > 0) {
					Debug.Write("Discarded {0} texture(s). Total texture memory: {1}mb", TexturesToDelete.Count, CommonTexture.TotalMemoryUsedMb);
					var ids = TexturesToDelete.ToArray();
					GL.DeleteTextures(ids.Length, ids);
					TexturesToDelete.Clear();
					PlatformRenderer.CheckErrors();
				}
			}
			lock (FramebuffersToDelete) {
				if (FramebuffersToDelete.Count > 0) {
					var ids = FramebuffersToDelete.ToArray();
					GL.DeleteFramebuffers(ids.Length, ids);
					FramebuffersToDelete.Clear();
					PlatformRenderer.CheckErrors();
				}
			}
		}

		public virtual string SerializationPath {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}

		public Rectangle AtlasUVRect { get { return uvRect; } }

		public void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv0, ref Vector2 uv1)
		{
		}

		public bool IsStubTexture { get { return false; } }

		public void LoadImage(string path)
		{
			using (Stream stream = AssetsBundle.Instance.OpenFileLocalized(path)) {
				LoadImage(stream);
			}
			var maskPath = Path.ChangeExtension(path, ".mask");
			if (AssetsBundle.Instance.FileExists(maskPath)) {
				OpacityMask = new OpacityMask(maskPath);
			}
		}

		public void LoadImage(byte[] data)
		{
			using (var stream = new MemoryStream(data)) {
				LoadImage(stream);
			}
		}

		public void LoadImage(Stream stream)
		{
			LoadImageHelper(stream);
		}

		public void LoadImage(Bitmap bitmap)
		{
			using (var stream = new MemoryStream()) {
				bitmap.SaveToStream(stream);
				stream.Position = 0;
				LoadImageHelper(stream);
			}
		}

		private void LoadImageHelper(Stream stream)
		{
			// Discard current texture
			Dispose();
			using (var rewindableStream = new RewindableStream(stream))
			using (var reader = new BinaryReader(rewindableStream)) {
#if iOS
				int sign = reader.ReadInt32();
				rewindableStream.Rewind();
				if (sign == PVRMagic) {
					InitWithPVRTexture(reader);
				} else {
					InitWithPngOrJpgBitmap(rewindableStream);
				}
#elif OPENGL
				int sign = reader.ReadInt32();
				rewindableStream.Rewind();
				if (sign == DDSMagic) {
					InitWithDDSBitmap(reader);
				} else {
					InitWithPngOrJpgBitmap(rewindableStream);
				}
#endif
			}
			uvRect = new Rectangle(Vector2.Zero, (Vector2)ImageSize / (Vector2)SurfaceSize);
		}

		private void PrepareOpenGLTexture()
		{
			DeleteScheduledTextures();
			// Generate a new texture.
			if (handle == 0) {
				var t = new int[1];
				GL.GenTextures(1, t);
				handle = (uint)t[0];
			}
			PlatformRenderer.SetTexture(handle, 0);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
			PlatformRenderer.CheckErrors();
		}

		/// <summary>
		/// Create texture from pixel array
		/// </summary>
		public void LoadImage(Color4[] pixels, int width, int height, bool generateMips)
		{
			Application.InvokeOnMainThread(() => {
				PrepareOpenGLTexture();
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
				if (generateMips) {
#if !MAC && !iOS
					GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
#endif
				}
				PlatformRenderer.CheckErrors();
			});

			ImageSize = new Size(width, height);
			SurfaceSize = ImageSize;
			uvRect = new Rectangle(0, 0, 1, 1);
		}

		/// <summary>
		/// Load subtexture from pixel array
		/// </summary>
		public void LoadSubImage(Color4[] pixels, int x, int y, int width, int height)
		{
			Application.InvokeOnMainThread(() => {
				PrepareOpenGLTexture();
				GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, width, height,
					PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
				PlatformRenderer.CheckErrors();
			});
		}

		~Texture2D()
		{
			Dispose();
		}

		public override void Dispose()
		{
			DisposeOpenGLTexture();
			base.Dispose();
		}

		protected void DisposeOpenGLTexture()
		{
			if (handle != 0) {
				lock (TexturesToDelete) {
					TexturesToDelete.Add(handle);
				}
				handle = 0;
			}
		}

		/// <summary>
		/// Returns native texture handle
		/// </summary>
		/// <returns></returns>
		public uint GetHandle()
		{
			return handle;
		}

		/// <summary>
		/// Sets texture as a render target
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
		/// Checks pixel transparency at given coordinates
		/// </summary>
		/// <param name="x">x-coordinate of pixel</param>
		/// <param name="y">y-coordinate of pixel</param>
		/// <returns></returns>
		public bool IsTransparentPixel(int x, int y)
		{
			if (OpacityMask != null) {
				int x1 = x * OpacityMask.Width / ImageSize.Width;
				int y1 = y * OpacityMask.Height / ImageSize.Height;
				return !OpacityMask.TestPixel(x1, y1);
			}
			return false;
		}

		private byte[] ReadTextureData(BinaryReader reader, int length)
		{
			MemoryUsed += length;
			return reader.ReadBytes(length);
		}
	}
}
#endif