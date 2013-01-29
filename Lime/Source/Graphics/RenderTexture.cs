using System;
using System.Diagnostics;
#if iOS
using OpenTK.Graphics.ES20;
#elif MAC
using MonoMac.OpenGL;
using OGL = MonoMac.OpenGL.GL;
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

#if GLES11
		static int defaultFramebuffer = -1;
#endif

		public RenderTexture(int width, int height)
		{
			size.Width = width;
			size.Height = height;
			uvRect = new Rectangle(0, 0, 1, 1);
#if GLES11
			if (defaultFramebuffer < 0) {
				GL.GetInteger(All.FramebufferBinding, ref defaultFramebuffer);
			}
			GL.GenFramebuffers(1, ref framebuffer);
			GL.GenTextures(1, ref id);
			GL.BindTexture(All.Texture2D, id);
			GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Linear);
			GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Linear);
			GL.TexImage2D(All.Texture2D, 0, (int)All.Rgba, width, height, 0,
				All.Bgra, All.UnsignedByte, (IntPtr)null);
			GL.BindFramebuffer(All.Framebuffer, framebuffer);
			GL.FramebufferTexture2D(All.Framebuffer, All.ColorAttachment0, All.Texture2D, id, 0);
			if (GL.CheckFramebufferStatus(All.Framebuffer) != All.FramebufferComplete)
				throw new Exception("Failed to create render texture. Framebuffer is incomplete.");
			GL.BindFramebuffer(All.Framebuffer, defaultFramebuffer);
#else
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
			Renderer.CheckErrors();
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
			if (id != 0) {
				lock (PlainTexture.TexturesToDelete) {
					PlainTexture.TexturesToDelete.Add(id);
				}
				id = 0;
			}
		}

		~RenderTexture()
		{
			Dispose();
		}

		public uint GetHandle()
		{
			return id;
		}

		public bool ChessTexture { get { return false; } }

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
			GL.BindFramebuffer(All.Framebuffer, defaultFramebuffer);
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