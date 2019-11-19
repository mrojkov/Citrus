namespace Lime
{
	internal class PostProcessingActionBloom : PostProcessingAction
	{
		public override bool EnabledCheck(PostProcessingRenderObject ro) => ro.BloomEnabled;
		public override PostProcessingAction.Buffer GetTextureBuffer(PostProcessingRenderObject ro) => ro.BloomBuffer;

		public override void Do(PostProcessingRenderObject ro)
		{
			if (ro.BloomBuffer.EqualRenderParameters(ro)) {
				ro.ProcessedTexture = ro.BloomBuffer.Texture;
				ro.CurrentBufferSize = (Vector2)ro.BloomBuffer.Size;
				ro.ProcessedUV1 = (Vector2)ro.ViewportSize / ro.CurrentBufferSize;
				return;
			}

			ro.PrepareOffscreenRendering(ro.Size);
			var bufferSize = (Vector2)ro.BloomBuffer.Size;
			var bloomViewportSize = (Size)(bufferSize * ro.BloomTextureScaling);
			ro.BloomMaterial.BrightThreshold = ro.BloomBrightThreshold;
			ro.BloomMaterial.InversedGammaCorrection = new Vector3(
				Mathf.Abs(ro.BloomGammaCorrection.X) > Mathf.ZeroTolerance ? 1f / ro.BloomGammaCorrection.X : 0f,
				Mathf.Abs(ro.BloomGammaCorrection.Y) > Mathf.ZeroTolerance ? 1f / ro.BloomGammaCorrection.Y : 0f,
				Mathf.Abs(ro.BloomGammaCorrection.Z) > Mathf.ZeroTolerance ? 1f / ro.BloomGammaCorrection.Z : 0f
			);
			ro.RenderToTexture(ro.FirstTemporaryBuffer.Texture, ro.ProcessedTexture, ro.BloomMaterial, Color4.White, ro.TextureClearingColor, bloomViewportSize);
			var bloomUV1 = (Vector2)bloomViewportSize / bufferSize;
			ro.BlurMaterial.Radius = ro.BloomStrength;
			ro.BlurMaterial.BlurShaderId = ro.BloomShaderId;
			ro.BlurMaterial.Step = ro.ProcessedUV1 * ro.BloomTextureScaling / ro.CurrentBufferSize;
			ro.BlurMaterial.Dir = Vector2.Down;
			ro.BlurMaterial.AlphaCorrection = 1f;
			ro.RenderToTexture(ro.SecondTemporaryBuffer.Texture, ro.FirstTemporaryBuffer.Texture, ro.BlurMaterial, Color4.White, ro.TextureClearingColor, bloomViewportSize, bloomUV1);
			ro.BlurMaterial.Dir = Vector2.Right;

			if (ro.DebugViewMode != PostProcessingPresenter.DebugViewMode.Bloom) {
				ro.RenderToTexture(ro.FirstTemporaryBuffer.Texture, ro.SecondTemporaryBuffer.Texture, ro.BlurMaterial, Color4.White, ro.TextureClearingColor, bloomViewportSize, bloomUV1);

				if (ro.ProcessedViewport.Width != ro.ViewportSize.Width || ro.ProcessedViewport.Height != ro.ViewportSize.Height) {
					Renderer.Viewport = ro.ProcessedViewport = new Viewport(0, 0, ro.ViewportSize.Width, ro.ViewportSize.Height);
				}
				ro.BloomBuffer.Texture.SetAsRenderTarget();
				try {
					var material = ro.AlphaDiffuseMaterial;
					Renderer.Clear(Color4.Zero);
					Renderer.DrawSprite(ro.ProcessedTexture, null, material, Color4.White, Vector2.Zero, ro.Size, Vector2.Zero, ro.ProcessedUV1, Vector2.Zero, Vector2.Zero);
					Renderer.DrawSprite(ro.FirstTemporaryBuffer.Texture, null, ro.AddDiffuseMaterial, ro.BloomColor, Vector2.Zero, ro.Size, Vector2.Zero, bloomUV1, Vector2.Zero, Vector2.Zero);
				} finally {
					ro.BloomBuffer.Texture.RestoreRenderTarget();
				}
				ro.BloomBuffer.SetRenderParameters(ro);
			} else {
				ro.RenderToTexture(ro.BloomBuffer.Texture, ro.SecondTemporaryBuffer.Texture, ro.BlurMaterial, ro.BloomColor, ro.TextureClearingColor, customUV1: bloomUV1);
				ro.BloomBuffer.MarkAsDirty();
			}
			ro.MarkBuffersAsDirty = true;
			ro.ProcessedTexture = ro.BloomBuffer.Texture;
			ro.CurrentBufferSize = (Vector2)ro.BloomBuffer.Size;
			ro.ProcessedUV1 = (Vector2)ro.ViewportSize / ro.CurrentBufferSize;
		}

		internal new class Buffer : PostProcessingAction.Buffer
		{
			private float strength = float.NaN;
			private float brightThreshold = float.NaN;
			private Vector3 gammaCorrection = -Vector3.One;
			private float textureScaling = float.NaN;
			private Color4 color = Color4.Zero;
			private Color4 textureClearingColor = Color4.Zero;

			public Buffer(Size size) : base(size) { }

			public bool EqualRenderParameters(PostProcessingRenderObject ro) =>
				!IsDirty &&
				strength == ro.BloomStrength &&
				brightThreshold == ro.BloomBrightThreshold &&
				gammaCorrection == ro.BloomGammaCorrection &&
				textureScaling == ro.BloomTextureScaling &&
				color == ro.BloomColor &&
				textureClearingColor == ro.TextureClearingColor;

			public void SetRenderParameters(PostProcessingRenderObject ro)
			{
				IsDirty = false;
				strength = ro.BloomStrength;
				brightThreshold = ro.BloomBrightThreshold;
				gammaCorrection = ro.BloomGammaCorrection;
				textureScaling = ro.BloomTextureScaling;
				color = ro.BloomColor;
				textureClearingColor = ro.TextureClearingColor;
			}
		}
	}
}
