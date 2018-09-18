#if OPENGL
using System;
using System.Diagnostics;
#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
using FramebufferSlot = OpenTK.Graphics.OpenGL.FramebufferAttachment;
using RenderbufferInternalFormat = OpenTK.Graphics.OpenGL.RenderbufferStorage;
#endif
using System.Collections.Generic;

#pragma warning disable 0618

namespace Lime
{
	public enum RenderTextureFormat
	{
		RGBA8,
		RGB565
	}

	[YuzuDontGenerateDeserializer]
	public class RenderTexture : CommonTexture, ITexture, IGLObject
	{
		private uint handle;
		private uint framebuffer;
		private uint depthBuffer;

		private readonly Size size = new Size(0, 0);
		private readonly Rectangle uvRect;
		private static readonly Stack<uint> framebufferStack = new Stack<uint>();

		private void SetTextureParameter(TextureParameterName name, int value)
		{
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, handle);
			GL.TexParameter(TextureTarget.Texture2D, name, value);
			PlatformRenderer.MarkTextureSlotAsDirty(0);
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

		public RenderTexture(int width, int height, RenderTextureFormat format = RenderTextureFormat.RGBA8)
		{
			Format = format;
			size.Width = width;
			size.Height = height;
			uvRect = new Rectangle(0, 0, 1, 1);
			GLObjectRegistry.Instance.Add(this);
		}

		private void CreateTexture()
		{
			if (IsDisposed) {
				throw new ObjectDisposedException(GetType().Name);
			}
			var t = new uint[1];
			GL.GenFramebuffers(1, t);
			framebuffer = t[0];
			GL.GenTextures(1, t);
			handle = t[0];
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, handle);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, textureParams.MinFilter.ToInt());
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, textureParams.MagFilter.ToInt());
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, textureParams.WrapModeU.ToInt());
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, textureParams.WrapModeV.ToInt());
			int bpp;
			if (Format == RenderTextureFormat.RGBA8) {
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.Width, size.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)null);
				bpp = 4;
			} else {
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, size.Width, size.Height, 0, PixelFormat.Rgb, PixelType.UnsignedShort565, (IntPtr)null);
				bpp = 2;
			}
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
			AttachRenderBuffer(FramebufferSlot.DepthAttachment, RenderbufferInternalFormat.DepthComponent16, out depthBuffer);
			Renderer.Clear(Color4.Black);
			PlatformRenderer.MarkTextureSlotAsDirty(0);
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

		public Size ImageSize {
			get { return size; }
		}

		public Size SurfaceSize {
			get { return size; }
		}

		public ITexture AtlasTexture => this;

		public Rectangle AtlasUVRect {
			get { return uvRect; }
		}

		public ITexture AlphaTexture { get { return null; } }

		public void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv) {}

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
			if (handle != 0) {
				var h = handle;
				Window.Current.InvokeOnRendering(() => {
					GL.DeleteTextures(1, new uint[] { h });
					PlatformRenderer.CheckErrors();
				});
				handle = 0;
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

		~RenderTexture()
		{
			Dispose();
		}

		public uint GetHandle()
		{
			if (handle == 0) {
				CreateTexture();
			}
			return handle;
		}

		public bool IsStubTexture { get { return false; } }

		public void SetAsRenderTarget()
		{
			Renderer.Flush();
			GetHandle();
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

		/// <summary>
		/// Copies all pixels from texture.
		/// </summary>
		/// <returns></returns>
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
