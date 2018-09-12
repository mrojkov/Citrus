using System;

namespace Lime
{
	public class PostProcessingPresenter : IPresenter
	{
		private readonly IMaterial blendingAddMaterial = WidgetMaterial.GetInstance(Blending.Add, ShaderId.Inherited, 1);
		private IMaterial material;
		private Blending blending;
		private ShaderId shader;
		private BlurBuffer blurBuffer;
		private BloomBuffer bloomBuffer;

		public Lime.RenderObject GetRenderObject(Node node)
		{
			var component = node.Components.Get<PostProcessingComponent>();
			if (component == null) {
				throw new InvalidOperationException();
			}

			var image = (Image)node;
			if (image.Width <= 0 || image.Height <= 0) {
				return null;
			}

			// TODO: Buffers pool
			// TODO: Recreate buffers when image.Texture was changed
			if (blurBuffer == null || blurBuffer.Size != image.Texture.ImageSize) {
				blurBuffer = new BlurBuffer(image.Texture.ImageSize);
			}
			if (bloomBuffer == null || bloomBuffer.Size != image.Texture.ImageSize) {
				bloomBuffer = new BloomBuffer(image.Texture.ImageSize);
			}

			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.Texture = image.Texture;
			ro.Material = GetImageMaterial(image);
			ro.LocalToWorldTransform = image.LocalToWorldTransform;
			ro.Position = image.ContentPosition;
			ro.Size = image.ContentSize;
			ro.Color = image.GlobalColor;
			ro.UV0 = image.UV0;
			ro.UV1 = image.UV1;
			ro.BlurBuffer = blurBuffer;
			ro.BlurMaterial = component.BlurMaterial;
			ro.BlurRadius = component.BlurRadius;
			ro.BlurTextureScaling = component.BlurTextureScaling;
			ro.BlurAlphaCorrection = component.BlurAlphaCorrection;
			ro.BlurBackgroundColor = component.BlurBackgroundColor;
			ro.BloomBuffer = bloomBuffer;
			ro.BloomMaterial = component.BloomMaterial;
			ro.BloomEnabled = component.BloomEnabled;
			ro.BloomStrength = component.BloomStrength;
			ro.BloomBrightThreshold = component.BloomBrightThreshold;
			ro.BloomGammaCorrection = component.BloomGammaCorrection;
			ro.BloomTextureScaling = component.BloomTextureScaling;
			ro.BlendingAddMaterial = blendingAddMaterial;
			return ro;
		}

		private IMaterial GetImageMaterial(Image image)
		{
			if (image.CustomMaterial != null) {
				return image.CustomMaterial;
			}
			if (material != null && blending == image.GlobalBlending && shader == image.GlobalShader) {
				return material;
			}
			blending = image.GlobalBlending;
			shader = image.GlobalShader;
			return material = WidgetMaterial.GetInstance(blending, shader, 1);
		}

		private class RenderObject : Lime.RenderObject
		{
			private bool wasOffscreenRenderingPrepared;
			private ITexture processedTexture;
			private Viewport processedViewport;
			private Vector2 processedUV1;
			private Size viewportSize;
			private Vector2 originalSize;

			public ITexture Texture;
			public IMaterial Material;
			public Matrix32 LocalToWorldTransform;
			public Vector2 Position;
			public Vector2 Size;
			public Color4 Color;
			public Vector2 UV0;
			public Vector2 UV1;
			public BlurBuffer BlurBuffer;
			public BlurMaterial BlurMaterial;
			public float BlurRadius;
			public float BlurTextureScaling;
			public float BlurAlphaCorrection;
			public Color4 BlurBackgroundColor;
			public BloomBuffer BloomBuffer;
			public BloomMaterial BloomMaterial;
			public bool BloomEnabled;
			public float BloomStrength;
			public float BloomBrightThreshold;
			public Vector3 BloomGammaCorrection;
			public float BloomTextureScaling;
			public IMaterial BlendingAddMaterial;

			protected override void OnRelease()
			{
				processedTexture = null;
				Texture = null;
				Material = null;
				BlurMaterial = null;
				BloomMaterial = null;
				BlendingAddMaterial = null;
			}

			public override void Render()
			{
				wasOffscreenRenderingPrepared = false;
				processedTexture = Texture;
				processedViewport = Viewport.Default;
				processedUV1 = Vector2.One;
				viewportSize = processedTexture.ImageSize;
				originalSize = (Vector2)viewportSize;
				try {
					ApplyBlur();
					PrepareBloom();
				} finally {
					FinalizeOffscreenRendering();
				}

				RenderTexture(processedTexture);
				ApplyBloom();
			}

			private void ApplyBlur()
			{
				if (Mathf.Abs(BlurRadius) <= Mathf.ZeroTolerance) {
					return;
				}
				viewportSize = (Size)((Vector2)viewportSize * BlurTextureScaling);
				if (BlurBuffer.EqualRenderParameters(BlurRadius, BlurTextureScaling, BlurAlphaCorrection, BlurBackgroundColor)) {
					processedTexture = BlurBuffer.FinalTexture;
					processedUV1 = (Vector2)viewportSize / originalSize;
					return;
				}

				PrepareOffscreenRendering(originalSize);
				BlurMaterial.Radius = BlurRadius;
				BlurMaterial.Step = new Vector2(BlurTextureScaling / viewportSize.Width, BlurTextureScaling / viewportSize.Height);
				BlurMaterial.Dir = Vector2.Down;
				BlurMaterial.AlphaCorrection = BlurAlphaCorrection;
				RenderToTexture(BlurBuffer.FirstPassTexture, processedTexture, BlurMaterial, Color4.White, BlurBackgroundColor);
				processedUV1 = (Vector2)viewportSize / originalSize;
				BlurMaterial.Dir = Vector2.Right;
				RenderToTexture(BlurBuffer.FinalTexture, BlurBuffer.FirstPassTexture, BlurMaterial, Color4.White, BlurBackgroundColor);

				processedTexture = BlurBuffer.FinalTexture;
				BlurBuffer.SetParameters(BlurRadius, BlurTextureScaling, BlurAlphaCorrection, BlurBackgroundColor);
			}

			private void PrepareBloom()
			{
				if (!BloomEnabled) {
					return;
				}

				var bloomViewportSize = (Size)(originalSize * BloomTextureScaling);
				if (BloomBuffer.EqualRenderParameters(BloomStrength, BloomBrightThreshold, BloomGammaCorrection, BloomTextureScaling)) {
					return;
				}

				PrepareOffscreenRendering(originalSize);
				BloomMaterial.BrightThreshold = BloomBrightThreshold;
				BloomMaterial.InversedGammaCorrection = new Vector3(
					Math.Abs(BloomGammaCorrection.X) > Mathf.ZeroTolerance ? 1f / BloomGammaCorrection.X : 0f,
					Math.Abs(BloomGammaCorrection.Y) > Mathf.ZeroTolerance ? 1f / BloomGammaCorrection.Y : 0f,
					Math.Abs(BloomGammaCorrection.Z) > Mathf.ZeroTolerance ? 1f / BloomGammaCorrection.Z : 0f
				);
				RenderToTexture(BloomBuffer.BrightColorsTexture, processedTexture, BloomMaterial, Color4.White, Color4.Black, bloomViewportSize);
				var bloomUV1 = (Vector2)bloomViewportSize / originalSize;
				BlurMaterial.Radius = BloomStrength;
				BlurMaterial.Step = new Vector2(BloomTextureScaling / bloomViewportSize.Width, BloomTextureScaling / bloomViewportSize.Height);
				BlurMaterial.Dir = Vector2.Down;
				BlurMaterial.AlphaCorrection = 1f;
				RenderToTexture(BloomBuffer.FirstBlurPassTexture, BloomBuffer.BrightColorsTexture, BlurMaterial, Color4.White, Color4.Black, bloomViewportSize, bloomUV1);
				BlurMaterial.Dir = Vector2.Right;
				RenderToTexture(BloomBuffer.FinalTexture, BloomBuffer.FirstBlurPassTexture, BlurMaterial, Color4.White, Color4.Black, bloomViewportSize, bloomUV1);

				BloomBuffer.SetParameters(BloomStrength, BloomBrightThreshold, BloomGammaCorrection, BloomTextureScaling);
			}

			private void ApplyBloom()
			{
				if (BloomEnabled) {
					var bloomUV1 = originalSize * BloomTextureScaling / originalSize;
					RenderTexture(BloomBuffer.FinalTexture, BlendingAddMaterial, bloomUV1);
				}
			}

			private void RenderToTexture(ITexture renderTargetTexture, ITexture sourceTexture, IMaterial material, Color4 color, Color4 backgroundColor, Size? customViewportSize = null, Vector2? customUV1 = null)
			{
				var vs = customViewportSize ?? viewportSize;
				var uv1 = customUV1 ?? processedUV1;
				if (processedViewport.Width != vs.Width || processedViewport.Height != vs.Height) {
					Renderer.Viewport = processedViewport = new Viewport(0, 0, vs.Width, vs.Height);
				}
				renderTargetTexture.SetAsRenderTarget();
				try {
					Renderer.Clear(backgroundColor);
					Renderer.DrawSprite(sourceTexture, null, material, color, Vector2.Zero, originalSize, Vector2.Zero, uv1, Vector2.Zero, Vector2.Zero);
				} finally {
					renderTargetTexture.RestoreRenderTarget();
				}
			}

			private void RenderTexture(ITexture texture, IMaterial customMaterial = null, Vector2? customUV1 = null)
			{
				var uv1 = customUV1 ?? processedUV1;
				Renderer.Transform1 = LocalToWorldTransform;
				Renderer.DrawSprite(texture, null, customMaterial ?? Material, Color, Position, Size, UV0 * uv1, UV1 * uv1, Vector2.Zero, Vector2.Zero);
			}

			private void PrepareOffscreenRendering(Vector2 orthogonalProjection)
			{
				if (wasOffscreenRenderingPrepared) {
					return;
				}
				Renderer.PushState(
					RenderState.Viewport |
					RenderState.World |
					RenderState.View |
					RenderState.Projection |
					RenderState.DepthState |
					RenderState.ScissorState |
					RenderState.CullMode |
					RenderState.Transform2
				);
				wasOffscreenRenderingPrepared = true;
				Renderer.ScissorState = ScissorState.ScissorDisabled;
				Renderer.World = Renderer.View = Matrix44.Identity;
				Renderer.SetOrthogonalProjection(Vector2.Zero, orthogonalProjection);
				Renderer.DepthState = DepthState.DepthDisabled;
				Renderer.CullMode = CullMode.None;
				Renderer.Transform2 = Matrix32.Identity;
				Renderer.Transform1 = Matrix32.Identity;
			}

			private void FinalizeOffscreenRendering()
			{
				if (!wasOffscreenRenderingPrepared) {
					return;
				}
				Renderer.PopState();
				wasOffscreenRenderingPrepared = false;
			}
		}

		public bool PartialHitTest(Node node, ref HitTestArgs args) => node.PartialHitTest(ref args);

		public IPresenter Clone() => new PostProcessingPresenter();

		private class BlurBuffer
		{
			private float radius = float.NaN;
			private float textureScaling = float.NaN;
			private float alphaCorrection = float.NaN;
			private Color4 backgroundColor = Color4.Zero;

			public RenderTexture FirstPassTexture { get; }
			public RenderTexture FinalTexture { get; }
			public Size Size { get; }

			public BlurBuffer(Size size)
			{
				Size = size;
				FirstPassTexture = new RenderTexture(size.Width, size.Height);
				FinalTexture = new RenderTexture(size.Width, size.Height);
			}

			public bool EqualRenderParameters(float radius, float textureScaling, float alphaCorrection, Color4 backgroundColor) =>
				this.radius == radius && this.textureScaling == textureScaling && this.alphaCorrection == alphaCorrection && this.backgroundColor == backgroundColor;

			public void SetParameters(float radius, float textureScaling, float alphaCorrection, Color4 backgroundColor)
			{
				this.radius = radius;
				this.textureScaling = textureScaling;
				this.alphaCorrection = alphaCorrection;
				this.backgroundColor = backgroundColor;
			}
		}

		private class BloomBuffer
		{
			private float strength = float.NaN;
			private float brightThreshold = float.NaN;
			private Vector3 gammaCorrection = -Vector3.One;
			private float textureScaling = float.NaN;

			public RenderTexture BrightColorsTexture { get; }
			public RenderTexture FirstBlurPassTexture { get; }
			public RenderTexture FinalTexture { get; }
			public Size Size { get; }

			public BloomBuffer(Size size)
			{
				Size = size;
				BrightColorsTexture = new RenderTexture(size.Width, size.Height);
				FirstBlurPassTexture = new RenderTexture(size.Width, size.Height);
				FinalTexture = new RenderTexture(size.Width, size.Height);
			}

			public bool EqualRenderParameters(float strength, float brightThreshold, Vector3 gammaCorrection, float textureScaling) =>
				this.strength == strength && this.brightThreshold == brightThreshold && this.gammaCorrection == gammaCorrection && this.textureScaling == textureScaling;

			public void SetParameters(float strength, float brightThreshold, Vector3 gammaCorrection, float textureScaling)
			{
				this.strength = strength;
				this.brightThreshold = brightThreshold;
				this.gammaCorrection = gammaCorrection;
				this.textureScaling = textureScaling;
			}
		}
	}
}
