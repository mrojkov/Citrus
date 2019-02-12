using System;

#if !iOS && !MAC && !ANDROID
using OpenTK.Graphics.ES20;
#endif

namespace Lime.Graphics.Platform.OpenGL
{
	internal class PlatformTexture2D : IPlatformTexture2D
	{
		private bool compressed;

		internal int GLTexture;
		internal All GLInternalFormat;
		internal All GLFormat;
		internal All GLPixelType;

		public PlatformRenderContext Context { get; }
		public Format Format { get; }
		public int Width { get; }
		public int Height { get; }
		public int LevelCount { get; }
		public bool Disposed { get; private set; }

		internal PlatformTexture2D(PlatformRenderContext context, Format format, int width, int height, bool mipmaps, TextureParams textureParams)
		{
			Context = context;
			Format = format;
			Width = width;
			Height = height;
			LevelCount = mipmaps ? GraphicsUtility.CalculateMipLevelCount(width, height) : 1;
			Initialize(textureParams);
		}

		public virtual void Dispose()
		{
			if (GLTexture != 0) {
				GL.DeleteTexture(GLTexture);
				GLHelper.CheckGLErrors();
				GLTexture = 0;
			}
			Disposed = true;
		}

		private void Initialize(TextureParams textureParams)
		{
			compressed = Format.IsCompressed();
			GLHelper.GetGLTextureFormat(Context, Format, out GLInternalFormat, out GLFormat, out GLPixelType);
			GLTexture = GL.GenTexture();
			GLHelper.CheckGLErrors();
			GL.ActiveTexture(TextureUnit.Texture0);
			GLHelper.CheckGLErrors();
			GL.BindTexture(TextureTarget.Texture2D, GLTexture);
			GLHelper.CheckGLErrors();
			for (var level = 0; level < LevelCount; level++) {
				GraphicsUtility.CalculateMipLevelSize(level, Width, Height, out var levelWidth, out var levelHeight);
				if (compressed) {
					var imageSize = GraphicsUtility.CalculateImageDataSize(Format, levelWidth, levelHeight);
					GL.CompressedTexImage2D(TextureTarget.Texture2D, level, (PixelInternalFormat)GLInternalFormat,
						levelWidth, levelHeight, 0, imageSize, IntPtr.Zero);
					GLHelper.CheckGLErrors();
				} else {
					GL.TexImage2D(TextureTarget.Texture2D, level, (PixelInternalFormat)GLInternalFormat,
						levelWidth, levelHeight, 0, (PixelFormat)GLFormat, (PixelType)GLPixelType, IntPtr.Zero);
					GLHelper.CheckGLErrors();
				}
			}
			UpdateTextureParams(textureParams);
			Context.InvalidateTextureBinding(0);
		}

		public void SetData(int level, int x, int y, int width, int height, IntPtr data)
		{
			GL.ActiveTexture(TextureUnit.Texture0);
			GLHelper.CheckGLErrors();
			GL.BindTexture(TextureTarget.Texture2D, GLTexture);
			GLHelper.CheckGLErrors();
			if (compressed) {
				var imageSize = GraphicsUtility.CalculateImageDataSize(Format, width, height);
				GL.CompressedTexSubImage2D(TextureTarget.Texture2D, level, x, y, width, height,
					(PixelFormat)GLInternalFormat, imageSize, data);
				GLHelper.CheckGLErrors();
			} else {
				GL.TexSubImage2D(TextureTarget.Texture2D, level, x, y, width, height,
					(PixelFormat)GLFormat, (PixelType)GLPixelType, data);
				GLHelper.CheckGLErrors();
			}
			Context.InvalidateTextureBinding(0);
		}

		public void SetTextureParams(TextureParams textureParams)
		{
			GL.ActiveTexture(TextureUnit.Texture0);
			GLHelper.CheckGLErrors();
			GL.BindTexture(TextureTarget.Texture2D, GLTexture);
			GLHelper.CheckGLErrors();
			UpdateTextureParams(textureParams);
			Context.InvalidateTextureBinding(0);
		}

		private void UpdateTextureParams(TextureParams textureParams)
		{
			textureParams = textureParams ?? TextureParams.Default;
			var glMinFilter = LevelCount > 1
				? GLHelper.GetGLTextureFilter(textureParams.MinFilter, textureParams.MipmapMode)
				: GLHelper.GetGLTextureFilter(textureParams.MinFilter);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)glMinFilter);
			GLHelper.CheckGLErrors();
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
				(int)GLHelper.GetGLTextureFilter(textureParams.MagFilter));
			GLHelper.CheckGLErrors();
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
				(int)GLHelper.GetGLTextureWrapMode(textureParams.WrapModeU));
			GLHelper.CheckGLErrors();
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
				(int)GLHelper.GetGLTextureWrapMode(textureParams.WrapModeV));
			GLHelper.CheckGLErrors();
		}
	}
}
