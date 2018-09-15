using System;

namespace Lime
{
	internal class PostProcessingRenderObject : RenderObject
	{
		private bool wasOffscreenRenderingPrepared;
		private ITexture processedTexture;
		private Viewport processedViewport;
		private Vector2 processedUV1;
		private Size viewportSize;
		private Vector2 textureSize;
		private Vector2 originalSize;

		public readonly RenderObjectList Objects = new RenderObjectList();
		public ITexture Texture;
		public IMaterial Material;
		public Matrix32 LocalToWorldTransform;
		public Vector2 Position;
		public Vector2 Size;
		public Color4 Color;
		public Vector2 UV0;
		public Vector2 UV1;
		public PostProcessingPresenter.DebugViewMode DebugViewMode;
		public PostProcessingPresenter.TextureBuffer SourceTextureBuffer;
		public PostProcessingPresenter.HSLBuffer HSLBuffer;
		public HSLMaterial HSLMaterial;
		public bool HSLEnabled;
		public Vector3 HSL;
		public PostProcessingPresenter.BlurBuffer BlurBuffer;
		public BlurMaterial BlurMaterial;
		public bool BlurEnabled;
		public float BlurRadius;
		public float BlurTextureScaling;
		public float BlurAlphaCorrection;
		public Color4 BlurBackgroundColor;
		public PostProcessingPresenter.BloomBuffer BloomBuffer;
		public BloomMaterial BloomMaterial;
		public bool BloomEnabled;
		public float BloomStrength;
		public float BloomBrightThreshold;
		public Vector3 BloomGammaCorrection;
		public float BloomTextureScaling;
		public bool OverallImpactEnabled;
		public Color4 OverallImpactColor;
		public IMaterial BlendingDefaultMaterial;
		public IMaterial BlendingAddMaterial;

		protected override void OnRelease()
		{
			processedTexture = null;
			Objects.Clear();
			Texture = null;
			Material = null;
			SourceTextureBuffer = null;
			HSLBuffer = null;
			HSLMaterial = null;
			BlurBuffer = null;
			BlurMaterial = null;
			BloomBuffer = null;
			BloomMaterial = null;
			BlendingDefaultMaterial = null;
			BlendingAddMaterial = null;
		}

		public override void Render()
		{
			wasOffscreenRenderingPrepared = false;
			try {
				PrepareTexture();
				ApplyHSL();
				ApplyBlur();
				PrepareBloom();
			} finally {
				FinalizeOffscreenRendering();
			}

			switch (DebugViewMode) {
				case PostProcessingPresenter.DebugViewMode.Bloom when BloomEnabled:
					RenderBloom();
					break;
				default:
					if (OverallImpactEnabled) {
						if (Texture != null) {
							RenderTexture(Texture);
						} else {
							RenderTexture(SourceTextureBuffer.FinalTexture, customUV1: Size / originalSize);
						}
						Color = OverallImpactColor;
					}
					RenderTexture(processedTexture);
					ApplyBloom();
					break;
			}
		}

		private void PrepareTexture()
		{
			if (Texture != null) {
				processedTexture = Texture;
				processedUV1 = Vector2.One;
				viewportSize = processedTexture.ImageSize;
				textureSize = (Vector2)processedTexture.ImageSize;
				originalSize = (Vector2)processedTexture.ImageSize;
			} else {
				PrepareOffscreenRendering(Size);
				Renderer.PushState(
					RenderState.Viewport |
					RenderState.World |
					RenderState.View |
					RenderState.Projection |
					RenderState.DepthState |
					RenderState.ScissorState |
					RenderState.CullMode |
					RenderState.Transform2 |
					RenderState.Transform1
				);
				viewportSize = (Size)Size;
				textureSize = Size;
				originalSize = (Vector2)SourceTextureBuffer.Size;
				SourceTextureBuffer.FinalTexture.SetAsRenderTarget();
				try {
					Renderer.Viewport = new Viewport(0, 0, viewportSize.Width, viewportSize.Height);
					Renderer.Clear(Color4.Zero);
					Renderer.Transform2 = LocalToWorldTransform.CalcInversed();
					Objects.Render();
				} finally {
					SourceTextureBuffer.FinalTexture.RestoreRenderTarget();
					Renderer.PopState();
				}
				processedTexture = SourceTextureBuffer.FinalTexture;
				processedUV1 = (Vector2)viewportSize / originalSize;
				HSLBuffer?.MarkAsDirty();
				BlurBuffer?.MarkAsDirty();
				BloomBuffer?.MarkAsDirty();
			}
			processedViewport = Viewport.Default;
		}

		private void ApplyHSL()
		{
			if (!HSLEnabled) {
				return;
			}
			if (HSLBuffer.EqualRenderParameters(HSL)) {
				processedTexture = HSLBuffer.FinalTexture;
				return;
			}

			PrepareOffscreenRendering(originalSize);
			HSLMaterial.HSL = HSL;
			RenderToTexture(HSLBuffer.FinalTexture, processedTexture, HSLMaterial, Color4.White, Color4.Zero);
			processedTexture = HSLBuffer.FinalTexture;

			HSLBuffer.SetParameters(HSL);
		}

		private void ApplyBlur()
		{
			if (!BlurEnabled) {
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

		private void RenderBloom()
		{
			var bloomUV1 = originalSize * BloomTextureScaling / originalSize;
			RenderTexture(BloomBuffer.FinalTexture, BlendingDefaultMaterial, bloomUV1);
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
				Renderer.DrawSprite(sourceTexture, null, material, color, Vector2.Zero, textureSize, Vector2.Zero, uv1, Vector2.Zero, Vector2.Zero);
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
}
