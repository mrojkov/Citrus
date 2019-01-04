using System;
using System.Collections.Generic;
using Lime.Graphics.Platform;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	public class RenderTexture : CommonTexture, ITexture, IGLObject
	{
		private IPlatformRenderTexture2D platformTexture;

		private readonly Size size = new Size(0, 0);
		private readonly Rectangle uvRect;
		private static readonly Stack<RenderTexture> renderTargetStack = new Stack<RenderTexture>();

		private TextureParams textureParams = TextureParams.Default;
		public TextureParams TextureParams
		{
			get
			{
				return textureParams;
			}
			set
			{
				if (textureParams != value) {
					textureParams = value;
					if (platformTexture != null) {
						platformTexture.TextureParams = textureParams;
					}
				}
			}
		}

		public Format Format { get; private set; }

		public RenderTexture(int width, int height, Format format = Format.R8G8B8A8_UNorm)
		{
			Format = format;
			size.Width = width;
			size.Height = height;
			uvRect = new Rectangle(0, 0, 1, 1);
			GLObjectRegistry.Instance.Add(this);
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
			if (platformTexture != null) {
				var platformTextureCopy = platformTexture;
				Window.Current.InvokeOnRendering(() => {
					platformTextureCopy.Dispose();
				});
				platformTexture = null;
			}
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

		public IPlatformRenderTexture2D GetPlatformTexture()
		{
			if (platformTexture == null) {
				platformTexture = RenderContextManager.CurrentContext.CreateRenderTexture2D(Format, size.Width, size.Height, textureParams);
				// FIXME: Clear texture
			}
			return platformTexture;
		}

		IPlatformTexture2D ITexture.GetPlatformTexture() => GetPlatformTexture();

		public bool IsStubTexture { get { return false; } }

		public void SetAsRenderTarget()
		{
			Renderer.Flush();
			renderTargetStack.Push(PlatformRenderer.CurrentRenderTarget);
			PlatformRenderer.SetRenderTarget(this);
		}

		public void RestoreRenderTarget()
		{
			Renderer.Flush();
			PlatformRenderer.SetRenderTarget(renderTargetStack.Pop());
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
			throw new NotImplementedException();
		}
	}
}
