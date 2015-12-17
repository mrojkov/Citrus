#if OPENGL
using System;
using System.IO;
#if MAC
using OpenTK.Graphics.OpenGL;
#elif WIN || iOS || ANDROID
using OpenTK.Graphics.ES20;
#endif
#pragma warning disable 0618

namespace Lime
{
	public partial class Texture2D : CommonTexture, ITexture
	{
		private void InitWithLimeBitmap(Bitmap bitmap)
		{
			if (!Application.CurrentThread.IsMain()) {
				throw new NotSupportedException ("Calling from non-main thread currently is not supported");
			}
			SurfaceSize = ImageSize = (Size)bitmap.Size;
			PrepareOpenGLTexture();
			var pixels = bitmap.GetPixels();
			PlatformRenderer.PushTexture(handle, 0);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0,
				PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
			MemoryUsed = pixels.Length * 4;
			PlatformRenderer.PopTexture(0);
			PlatformRenderer.CheckErrors();
		}

		private void InitWithPngOrJpgBitmap(Stream stream)
		{
			using (var bitmap = new Bitmap(stream)) {
				InitWithLimeBitmap(bitmap);
			}
		}
	}
}
#endif