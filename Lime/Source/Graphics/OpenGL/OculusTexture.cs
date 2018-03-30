#if OPENGL && WIN
using System;
using OpenTK.Graphics.OpenGL;
using FramebufferSlot = OpenTK.Graphics.OpenGL.FramebufferAttachment;
using RenderbufferInternalFormat = OpenTK.Graphics.OpenGL.RenderbufferStorage;
using System.Collections.Generic;


namespace Lime
{
	public class OculusTexture : CommonTexture, ITexture, IGLObject
	{

		private readonly uint handle;
		private uint framebuffer;
		private uint depthBuffer;

		private readonly Size size = new Size(0, 0);
		private readonly Rectangle uvRect;
		private static readonly Stack<uint> framebufferStack = new Stack<uint>();

		private void SetTextureParameter(TextureParameterName name, int value)
		{
			PlatformRenderer.PushTexture(handle, 0);
			GL.TexParameter(TextureTarget.Texture2D, name, value);
			PlatformRenderer.PopTexture(0);
		}

		private TextureParams textureParams = TextureParams.Default;
		public TextureParams TextureParams
		{
			get
			{
				return textureParams;
			}
			set
			{
				if (textureParams == value) {
					return;
				}
				if (textureParams.WrapModeU != value.WrapModeU) SetTextureParameter(TextureParameterName.TextureWrapS, value.WrapModeU.ToInt());
				if (textureParams.WrapModeV != value.WrapModeV) SetTextureParameter(TextureParameterName.TextureWrapT, value.WrapModeV.ToInt());
				if (textureParams.MinFilter != value.MinFilter) SetTextureParameter(TextureParameterName.TextureMinFilter, value.MinFilter.ToInt());
				if (textureParams.MagFilter != value.MagFilter) SetTextureParameter(TextureParameterName.TextureMagFilter, value.MagFilter.ToInt());
				textureParams = value;
			}
		}

		public RenderTextureFormat Format { get; private set; }

		public OculusTexture(int width, int height, uint handle)
		{
			this.handle = handle;
			size.Width = width;
			size.Height = height;
			uvRect = new Rectangle(0, 0, 1, 1);
			CreateTexture();
		}

		private void CreateTexture()
		{
			if (!Application.CurrentThread.IsMain()) {
				throw new Exception("Attempt to create a RenderTexture not from the main thread");
			}
			var t = new uint[1];
			GL.GenFramebuffers(1, t);
			framebuffer = t[0];
			PlatformRenderer.PushTexture(handle, 0);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, textureParams.MinFilter.ToInt());
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, textureParams.MagFilter.ToInt());
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, textureParams.WrapModeU.ToInt());
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, textureParams.WrapModeV.ToInt());
			int bpp = 4;
			MemoryUsed = SurfaceSize.Width * SurfaceSize.Height * bpp;
			uint oldFramebuffer = PlatformRenderer.CurrentFramebuffer;
			PlatformRenderer.BindFramebuffer(framebuffer);
			PlatformRenderer.CheckErrors();
			GL.FramebufferTexture2D(
				FramebufferTarget.Framebuffer,
				FramebufferSlot.ColorAttachment0,
				TextureTarget.Texture2D,
				handle,
				level: 0);
			if ((int)GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != (int)FramebufferErrorCode.FramebufferComplete)
				throw new Exception("Failed to create render texture. Framebuffer is incomplete.");
			AttachRenderBuffer(FramebufferSlot.DepthAttachment, RenderbufferInternalFormat.DepthComponent32, out depthBuffer);
			Renderer.Clear(0, 0, 0, 0);
			PlatformRenderer.PopTexture(0);
			PlatformRenderer.BindFramebuffer(oldFramebuffer);
			PlatformRenderer.CheckErrors();
		}

		private void AttachRenderBuffer(FramebufferSlot attachement, RenderbufferInternalFormat format, out uint handle)
		{
			var t = new uint[1];
			GL.GenRenderbuffers(1, t);
			handle = t[0];
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, handle);
			GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, format, size.Width, size.Height);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, attachement, RenderbufferTarget.Renderbuffer, handle);
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
		}

		public Size ImageSize
		{
			get { return size; }
		}

		public Size SurfaceSize
		{
			get { return size; }
		}

		public ITexture AtlasTexture => this;

		public Rectangle AtlasUVRect
		{
			get { return uvRect; }
		}

		public ITexture AlphaTexture { get { return null; } }

		public void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv) { }

		public void Discard()
		{
			MemoryUsed = 0;
			if (framebuffer != 0) {
				var h = framebuffer;
				Window.Current.InvokeOnRendering(() => {
					GL.DeleteFramebuffers(1, new uint[] { h });
					PlatformRenderer.CheckErrors();
				});
				framebuffer = 0;
			}
			DeleteRenderBuffer(ref depthBuffer);
		}

		private void DeleteRenderBuffer(ref uint renderBuffer)
		{
			var h = renderBuffer;
			if (h != 0) {
				Window.Current.InvokeOnRendering(() => {
					GL.DeleteRenderbuffers(1, new uint[] { h });
				});
			}
			renderBuffer = 0;
		}

		public override void Dispose()
		{
			Discard();
			base.Dispose();
		}

		~OculusTexture()
		{
			Dispose();
		}

		public uint GetHandle()
		{
			return handle;
		}

		public bool IsStubTexture { get { return false; } }

		public string SerializationPath
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public void SetAsRenderTarget()
		{
			Renderer.Flush();
			uint currentFramebuffer = (uint)PlatformRenderer.CurrentFramebuffer;
			PlatformRenderer.BindFramebuffer(framebuffer);
			framebufferStack.Push(currentFramebuffer);
		}

		public void RestoreRenderTarget()
		{
			Renderer.Flush();
			uint prevFramebuffer = framebufferStack.Pop();
			PlatformRenderer.BindFramebuffer(prevFramebuffer);
		}

		public bool IsTransparentPixel(int x, int y)
		{
			return false;
		}

		public Color4[] GetPixels()
		{
			SetAsRenderTarget();
			try {
				var pixels = new Color4[size.Width * size.Height];
				GL.ReadPixels(0, 0, size.Width, size.Height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
				return pixels;
			} finally {
				RestoreRenderTarget();
			}
		}
	}
}
#endif
