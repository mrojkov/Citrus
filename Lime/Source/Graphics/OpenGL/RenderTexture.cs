#if OPENGL
using System;
using System.Diagnostics;
#if iOS
using OpenTK.Graphics.ES20;
#elif MAC
using MonoMac.OpenGL;
using MonoMac.OpenGL;
#elif WIN
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
#endif
using System.Collections.Generic;

namespace Lime
{
	public enum RenderTextureFormat
	{
		RGBA8,
		RGB565
	}

	public class RenderTexture : ITexture, IDisposable
	{
		uint handle;
		uint framebuffer;
		readonly Size size = new Size(0, 0);
		readonly Rectangle uvRect;
		static readonly Stack<uint> framebufferStack = new Stack<uint>();

		public RenderTexture(int width, int height, RenderTextureFormat format = RenderTextureFormat.RGBA8)
		{
			size.Width = width;
			size.Height = height;
			uvRect = new Rectangle(0, 0, 1, 1);
			var t = new uint[1];
			GL.GenFramebuffers(1, t);
			framebuffer = t[0];
			GL.GenTextures(1, t);
			handle = t[0];
			GL.BindTexture(TextureTarget.Texture2D, handle);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
			if (format == RenderTextureFormat.RGBA8) {
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)null);
			} else {
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedShort565, (IntPtr)null);
			}
			uint currentFramebuffer = (uint)Renderer.GetCurrentFramebuffer();
			BindFramebuffer(framebuffer);
			Renderer.CheckErrors();
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, handle, 0);
			if ((int)GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != (int)FramebufferErrorCode.FramebufferComplete)
				throw new Exception("Failed to create render texture. Framebuffer is incomplete.");
			BindFramebuffer(currentFramebuffer);
			Renderer.CheckErrors();
		}

		public Size ImageSize {
			get { return size; }
		}

		public Size SurfaceSize {
			get { return size; }
		}		
		
		public Rectangle AtlasUVRect {
			get { return uvRect; }
		}

		public void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv0, ref Vector2 uv1)
		{
		}
		
		public void Dispose()
		{
			if (framebuffer != 0) {
				lock (Texture2D.FramebuffersToDelete) {
					Texture2D.FramebuffersToDelete.Add(framebuffer);
				}
				framebuffer = 0;
			}
			if (handle != 0) {
				lock (Texture2D.TexturesToDelete) {
					Texture2D.TexturesToDelete.Add(handle);
				}
				handle = 0;
			}
		}

		~RenderTexture()
		{
			Dispose();
		}

		public uint GetHandle()
		{
			return handle;
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
			uint currentFramebuffer = (uint)Renderer.GetCurrentFramebuffer();
			BindFramebuffer(framebuffer);
			framebufferStack.Push(currentFramebuffer);
		}

		private static void BindFramebuffer(uint framebuffer)
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
		}

		public void RestoreRenderTarget()
		{
			Renderer.FlushSpriteBatch();
			uint prevFramebuffer = framebufferStack.Pop();
			BindFramebuffer(prevFramebuffer);
		}

		public bool IsTransparentPixel(int x, int y)
		{
			return false;
		}
	}
}
#endif