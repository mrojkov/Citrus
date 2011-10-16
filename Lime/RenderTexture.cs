using System;
using System.Diagnostics;
#if iOS
using OpenTK.Graphics.ES20;
#elif MAC
using MonoMac.OpenGL;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace Lime
{
	/// <summary>
	/// Represents 2D texture.
	/// </summary>
	public class RenderTexture : ITexture, IDisposable
	{
		int framebuffer;
		int id;
		Size size = new Size (0, 0);
		Rectangle uvRect;

		/// <summary>
		/// Constructor.
		/// </summary>
		public RenderTexture (int width, int height)
		{
#if MAC
			size.Width = width;
			size.Height = height;
			uvRect = new Rectangle (0, 0, 1, 1);
			GL.GenFramebuffers (1, out framebuffer);
			id = GL.GenTexture ();
			GL.BindTexture (TextureTarget.Texture2D, id);
			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.Hint (HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);
			GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, (IntPtr)null);
			GL.BindFramebuffer (FramebufferTarget.FramebufferExt, framebuffer);
			GL.FramebufferTexture2D (FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, id, 0);
			if (GL.CheckFramebufferStatus (FramebufferTarget.FramebufferExt) != FramebufferErrorCode.FramebufferCompleteExt)
				throw new Exception ("Failed to create render texture. Framebuffer is incomplete.");
			GL.BindFramebuffer (FramebufferTarget.FramebufferExt, 0);
#endif
		}

		/// <summary>
		/// Size of texture.
		/// </summary>
		public Size ImageSize {
			get { return size; }
		}

		public Size SurfaceSize {
			get { return size; }
		}		
		
		public Rectangle UVRect {
			get { return uvRect; }
		}
		
		private void Dispose (bool manual)
		{
#if MAC			
			if (id != 0) {
				if (manual) {
					GL.DeleteFramebuffers (1, ref framebuffer);
					GL.DeleteTexture (id);
				} else
					Debug.Print ("[Warning] {0} leaked.", this);
				id = 0;
			}
#endif			
		}

		/// <summary>
		/// Frees OpenGL texture
		/// </summary>
		public void Dispose ()
		{
			this.Dispose (true);
		}

		/// <summary>
		/// Returns native texture handle.
		/// </summary>
		/// <returns></returns>
		public uint GetHandle ()
		{
			return (uint)id;
		}

		/// <summary>
		/// Sets texture as a render target.
		/// </summary>
		public void SetAsRenderTarget ()
		{
//			GL.BindFramebuffer (FramebufferTarget.FramebufferExt, framebuffer);
		}

		/// <summary>
		/// Sets texture as a render target.
		/// </summary>
		public void RestoreRenderTarget ()
		{
//			GL.BindFramebuffer (FramebufferTarget.FramebufferExt, 0);
		}

		/// <summary>
		/// Checks pixel transparency at given coordinates.
		/// </summary>
		/// <param name="x">x-coordinate of pixel</param>
		/// <param name="y">y-coordinate of pixel</param>
		/// <returns></returns>
		public bool IsTransparentPixel (int x, int y)
		{
			return false;
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~RenderTexture ()
		{
			Dispose (false);
		}
	}
}