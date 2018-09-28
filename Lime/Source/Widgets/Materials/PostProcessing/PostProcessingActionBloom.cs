namespace Lime
{
	internal class PostProcessingActionBloom : PostProcessingAction
	{
		public override bool Enabled => RenderObject.BloomEnabled;
		public override PostProcessingAction.Buffer TextureBuffer => RenderObject.BloomBuffer;

		public override void Do()
		{
			if (RenderObject.BloomBuffer.EqualRenderParameters(RenderObject)) {
				RenderObject.ProcessedTexture = RenderObject.BloomBuffer.Texture;
				RenderObject.CurrentBufferSize = (Vector2)RenderObject.BloomBuffer.Size;
				RenderObject.ProcessedUV1 = (Vector2)RenderObject.ViewportSize / RenderObject.CurrentBufferSize;
				return;
			}

			RenderObject.PrepareOffscreenRendering(RenderObject.Size);
			var bufferSize = (Vector2)RenderObject.BloomBuffer.Size;
			var bloomViewportSize = (Size)(bufferSize * RenderObject.BloomTextureScaling);
			RenderObject.BloomMaterial.BrightThreshold = RenderObject.BloomBrightThreshold;
			RenderObject.BloomMaterial.InversedGammaCorrection = new Vector3(
				Mathf.Abs(RenderObject.BloomGammaCorrection.X) > Mathf.ZeroTolerance ? 1f / RenderObject.BloomGammaCorrection.X : 0f,
				Mathf.Abs(RenderObject.BloomGammaCorrection.Y) > Mathf.ZeroTolerance ? 1f / RenderObject.BloomGammaCorrection.Y : 0f,
				Mathf.Abs(RenderObject.BloomGammaCorrection.Z) > Mathf.ZeroTolerance ? 1f / RenderObject.BloomGammaCorrection.Z : 0f
			);
			RenderObject.RenderToTexture(RenderObject.FirstTemporaryBuffer.Texture, RenderObject.ProcessedTexture, RenderObject.BloomMaterial, Color4.White, Color4.Black, bloomViewportSize);
			var bloomUV1 = (Vector2)bloomViewportSize / bufferSize;
			RenderObject.BlurMaterial.Radius = RenderObject.BloomStrength;
			RenderObject.BlurMaterial.Step = RenderObject.ProcessedUV1 * RenderObject.BloomTextureScaling / RenderObject.CurrentBufferSize;
			RenderObject.BlurMaterial.Dir = Vector2.Down;
			RenderObject.BlurMaterial.AlphaCorrection = 1f;
			RenderObject.RenderToTexture(RenderObject.SecondTemporaryBuffer.Texture, RenderObject.FirstTemporaryBuffer.Texture, RenderObject.BlurMaterial, RenderObject.BloomColor, Color4.Black, bloomViewportSize, bloomUV1);
			RenderObject.BlurMaterial.Dir = Vector2.Right;

			if (RenderObject.DebugViewMode != PostProcessingPresenter.DebugViewMode.Bloom) {
				RenderObject.RenderToTexture(RenderObject.FirstTemporaryBuffer.Texture, RenderObject.SecondTemporaryBuffer.Texture, RenderObject.BlurMaterial, Color4.White, Color4.Black, bloomViewportSize, bloomUV1);

				if (RenderObject.ProcessedViewport.Width != RenderObject.ViewportSize.Width || RenderObject.ProcessedViewport.Height != RenderObject.ViewportSize.Height) {
					Renderer.Viewport = RenderObject.ProcessedViewport = new Viewport(0, 0, RenderObject.ViewportSize.Width, RenderObject.ViewportSize.Height);
				}
				RenderObject.BloomBuffer.Texture.SetAsRenderTarget();
				try {
					Renderer.Clear(Color4.Zero);
					Renderer.DrawSprite(RenderObject.ProcessedTexture, null, RenderObject.DefaultMaterial, Color4.White, Vector2.Zero, RenderObject.Size, Vector2.Zero, RenderObject.ProcessedUV1, Vector2.Zero, Vector2.Zero);
					Renderer.DrawSprite(RenderObject.FirstTemporaryBuffer.Texture, null, RenderObject.BlendingAddMaterial, Color4.White, Vector2.Zero, RenderObject.Size, Vector2.Zero, bloomUV1, Vector2.Zero, Vector2.Zero);
				} finally {
					RenderObject.BloomBuffer.Texture.RestoreRenderTarget();
				}

				RenderObject.BloomBuffer.SetRenderParameters(RenderObject);
				RenderObject.MarkBuffersAsDirty = true;
			} else {
				RenderObject.RenderToTexture(RenderObject.BloomBuffer.Texture, RenderObject.SecondTemporaryBuffer.Texture, RenderObject.BlurMaterial, Color4.White, Color4.Black, customUV1: bloomUV1);
				RenderObject.BloomBuffer.MarkAsDirty();
			}
			RenderObject.ProcessedTexture = RenderObject.BloomBuffer.Texture;
			RenderObject.CurrentBufferSize = (Vector2)RenderObject.BloomBuffer.Size;
			RenderObject.ProcessedUV1 = (Vector2)RenderObject.ViewportSize / RenderObject.CurrentBufferSize;
		}

		internal new class Buffer : PostProcessingAction.Buffer
		{
			private float strength = float.NaN;
			private float brightThreshold = float.NaN;
			private Vector3 gammaCorrection = -Vector3.One;
			private float textureScaling = float.NaN;
			private Color4 color = Color4.Zero;

			public Buffer(Size size) : base(size) { }

			public bool EqualRenderParameters(PostProcessingRenderObject ro) =>
				!IsDirty &&
				strength == ro.BloomStrength &&
				brightThreshold == ro.BloomBrightThreshold &&
				gammaCorrection == ro.BloomGammaCorrection &&
				textureScaling == ro.BloomTextureScaling &&
				color == ro.BloomColor;

			public void SetRenderParameters(PostProcessingRenderObject ro)
			{
				IsDirty = false;
				strength = ro.BloomStrength;
				brightThreshold = ro.BloomBrightThreshold;
				gammaCorrection = ro.BloomGammaCorrection;
				textureScaling = ro.BloomTextureScaling;
				color = ro.BloomColor;
			}
		}
	}
}
