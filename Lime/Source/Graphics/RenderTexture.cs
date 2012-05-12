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
	public class RenderTexture : ITexture, IDisposable
	{
		int framebuffer;
		uint id;
		Size size = new Size(0, 0);
		Rectangle uvRect;

		public RenderTexture(int width, int height)
		{
#if MAC || WIN
			size.Width = width;
			size.Height = height;
			uvRect = new Rectangle(0, 0, 1, 1);
			GL.GenFramebuffers(1, out framebuffer);
			id = (uint)GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, id);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
				PixelFormat.Bgra, PixelType.UnsignedByte, (IntPtr)null);
			GL.BindFramebuffer(FramebufferTarget.FramebufferExt, framebuffer);
			GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, id, 0);
			if (GL.CheckFramebufferStatus(FramebufferTarget.FramebufferExt) != FramebufferErrorCode.FramebufferCompleteExt)
				throw new Exception("Failed to create render texture. Framebuffer is incomplete.");
			GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
#endif
		}

		public Size ImageSize {
			get { return size; }
		}

		public Size SurfaceSize {
			get { return size; }
		}		
		
		public Rectangle UVRect {
			get { return uvRect; }
		}
		
		public void Dispose()
		{
#if MAC || WIN
			if (id != 0) {
				lock (PlainTexture.TexturesToDelete) {
					PlainTexture.TexturesToDelete.Add(id);
				}
				id = 0;
			}
#endif
		}

		~RenderTexture()
		{
			Dispose();
		}

		public uint GetHandle()
		{
			return id;
		}

		public string SerializationPath {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}

		public void SetAsRenderTarget()
		{
			Renderer.FlushSpriteBatch();
#if iOS
			GL.BindFramebuffer(All.Framebuffer, framebuffer);
#else
			GL.BindFramebuffer(FramebufferTarget.FramebufferExt, framebuffer);
#endif
		}

		public void RestoreRenderTarget()
		{
			Renderer.FlushSpriteBatch();
#if iOS
			GL.BindFramebuffer(All.Framebuffer, 0);
#else
			GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
#endif
		}

		public bool IsTransparentPixel(int x, int y)
		{
			return false;
		}
	}
}