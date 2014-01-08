using System.Collections.Generic;
#if OPENGL || GLES11
using System;
using System.Diagnostics;
#if iOS
using OpenTK.Graphics.ES20;
#elif MAC
using MonoMac.OpenGL;
using OGL = MonoMac.OpenGL.GL;
#elif WIN
using OpenTK.Graphics.OpenGL;
using OGL = OpenTK.Graphics.OpenGL.GL;
#endif

namespace Lime
{
	public class RenderTexture : ITexture, IDisposable
	{
		uint id;
		readonly int framebuffer;
		readonly Size size = new Size(0, 0);
		readonly Rectangle uvRect;
		static readonly Stack<int> framebufferStack = new Stack<int>();

		public RenderTexture(int width, int height)
		{
			size.Width = width;
			size.Height = height;
			uvRect = new Rectangle(0, 0, 1, 1);
#if GLES11
			int defaultFramebuffer = 0;
			GL.GetInteger(All.FramebufferBinding, ref defaultFramebuffer);
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
#elif OPENGL
			OGL.GenFramebuffers(1, out framebuffer);
			id = (uint)OGL.GenTexture();
			OGL.BindTexture(TextureTarget.Texture2D, id);
			OGL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			OGL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			OGL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);
			OGL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
				PixelFormat.Bgra, PixelType.UnsignedByte, (IntPtr)null);
			OGL.BindFramebuffer(FramebufferTarget.FramebufferExt, framebuffer);
			OGL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, id, 0);
			if (OGL.CheckFramebufferStatus(FramebufferTarget.FramebufferExt) != FramebufferErrorCode.FramebufferCompleteExt)
				throw new Exception("Failed to create render texture. Framebuffer is incomplete.");
			OGL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
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
				lock (Texture2D.TexturesToDelete) {
					Texture2D.TexturesToDelete.Add(id);
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

		public bool IsStubTexture { get { return false; } }

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
			int currentFramebuffer = 0;
#if GLES11
			GL.GetInteger(All.FramebufferBinding, ref currentFramebuffer);
			GL.BindFramebuffer(All.Framebuffer, framebuffer);
			framebufferStack.Push(currentFramebuffer);
#elif OPENGL
			OGL.GetInteger(GetPName.FramebufferBindingExt, out currentFramebuffer);
			OGL.BindFramebuffer(FramebufferTarget.FramebufferExt, framebuffer);
			framebufferStack.Push(currentFramebuffer);
#endif
		}

		public void RestoreRenderTarget()
		{
			Renderer.FlushSpriteBatch();
			int prevFramebuffer = framebufferStack.Pop();
#if GLES11
			GL.BindFramebuffer(All.Framebuffer, prevFramebuffer);
#elif OPENGL
			OGL.BindFramebuffer(FramebufferTarget.FramebufferExt, prevFramebuffer);
#endif
		}

		public bool IsTransparentPixel(int x, int y)
		{
			return false;
		}
	}
}
#endif