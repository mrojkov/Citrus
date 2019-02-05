using System;

#if !iOS && !MAC && !ANDROID
using OpenTK.Graphics.ES20;
#endif

namespace Lime.Graphics.Platform.OpenGL
{
	internal class PlatformRenderTexture2D : PlatformTexture2D, IPlatformRenderTexture2D
	{
		internal int GLFramebuffer;
		internal int GLDepthBuffer;
		internal int GLStencilBuffer;

		internal PlatformRenderTexture2D(PlatformRenderContext context, Format format, int width, int height, TextureParams textureParams)
			: base(context, format, width, height, false, textureParams)
		{
			Initialize();
		}

		public override void Dispose()
		{
			if (GLFramebuffer != 0) {
				GL.DeleteFramebuffer(GLFramebuffer);
				GLHelper.CheckGLErrors();
				GLFramebuffer = 0;
			}
			if (GLStencilBuffer != 0) {
				if (GLStencilBuffer != GLDepthBuffer) {
					GL.DeleteRenderbuffer(GLStencilBuffer);
					GLHelper.CheckGLErrors();
				}
				GLStencilBuffer = 0;
			}
			if (GLDepthBuffer != 0) {
				GL.DeleteRenderbuffer(GLDepthBuffer);
				GLHelper.CheckGLErrors();
				GLDepthBuffer = 0;
			}
			base.Dispose();
		}

		private void Initialize()
		{
			GetGLDepthStencilFormat(out var glDepthFormat, out var glStencilFormat);
			GLFramebuffer = GL.GenFramebuffer();
			GLHelper.CheckGLErrors();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, GLFramebuffer);
			GLHelper.CheckGLErrors();
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, GLTexture, 0);
			GLHelper.CheckGLErrors();
			GLDepthBuffer = GLStencilBuffer = 0;
			if (glDepthFormat != 0) {
				GLDepthBuffer = GL.GenRenderbuffer();
				GLHelper.CheckGLErrors();
				GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, GLDepthBuffer);
				GLHelper.CheckGLErrors();
				GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, (RenderbufferInternalFormat)glDepthFormat, Width, Height);
				GLHelper.CheckGLErrors();
				GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.DepthAttachment,
					RenderbufferTarget.Renderbuffer, GLDepthBuffer);
				GLHelper.CheckGLErrors();
			}
			if (glStencilFormat != 0) {
				GLStencilBuffer = GLDepthBuffer;
				if (glStencilFormat != glDepthFormat) {
					GLStencilBuffer = GL.GenRenderbuffer();
					GLHelper.CheckGLErrors();
					GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, GLStencilBuffer);
					GLHelper.CheckGLErrors();
					GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, (RenderbufferInternalFormat)glStencilFormat, Width, Height);
					GLHelper.CheckGLErrors();
				}
				GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.StencilAttachment,
					RenderbufferTarget.Renderbuffer, GLStencilBuffer);
				GLHelper.CheckGLErrors();
			}
			var fbStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
			GLHelper.CheckGLErrors();
			if (fbStatus != FramebufferErrorCode.FramebufferComplete) {
				throw new InvalidOperationException();
			}
			Context.InvalidateRenderTargetBinding();
		}

		private void GetGLDepthStencilFormat(out All glDepthFormat, out All glStencilFormat)
		{
			glDepthFormat = glStencilFormat = 0;
			if (Context.SupportsPackedDepth24Stencil8) {
				glDepthFormat = glStencilFormat = All.Depth24Stencil8Oes;
			} else {
				if (Context.SupportsDepth24) {
					glDepthFormat = All.DepthComponent24Oes;
				} else {
					glDepthFormat = All.DepthComponent16;
				}
				glStencilFormat = All.StencilIndex8;
			}
		}

		public void ReadPixels(Format format, int x, int y, int width, int height, IntPtr pixels)
		{
			GLHelper.GetGLTextureFormat(Context, format, out _, out var glDstFormat, out var glDstType);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, GLFramebuffer);
			GLHelper.CheckGLErrors();
			GL.ReadPixels(x, y, width, height, (PixelFormat)glDstFormat, (PixelType)glDstType, pixels);
			GLHelper.CheckGLErrors();
			Context.InvalidateRenderTargetBinding();
		}
	}
}