namespace Lime
{
	internal class PostProcessingActionNoise : PostProcessingAction
	{
		public override bool Enabled => RenderObject.NoiseEnabled;
		public override PostProcessingAction.Buffer TextureBuffer => RenderObject.NoiseBuffer;

		public override void Do()
		{
			if (RenderObject.NoiseBuffer.EqualRenderParameters(RenderObject)) {
				RenderObject.ProcessedTexture = RenderObject.NoiseBuffer.Texture;
				RenderObject.CurrentBufferSize = (Vector2)RenderObject.NoiseBuffer.Size;
				RenderObject.ProcessedUV1 = (Vector2)RenderObject.ViewportSize / RenderObject.CurrentBufferSize;
				return;
			}

			RenderObject.PrepareOffscreenRendering(RenderObject.Size);
			if (RenderObject.ProcessedViewport.Width != RenderObject.ViewportSize.Width || RenderObject.ProcessedViewport.Height != RenderObject.ViewportSize.Height) {
				Renderer.Viewport = RenderObject.ProcessedViewport = new Viewport(0, 0, RenderObject.ViewportSize.Width, RenderObject.ViewportSize.Height);
			}
			RenderObject.NoiseBuffer.Texture.SetAsRenderTarget();
			try {
				Renderer.Clear(Color4.Zero);
				var noiseUV0 = RenderObject.NoiseOffset;
				var noiseUV1 = RenderObject.Size / ((Vector2)RenderObject.NoiseTexture.ImageSize * RenderObject.NoiseScale) + RenderObject.NoiseOffset;
				RenderObject.NoiseMaterial.BrightThreshold = RenderObject.NoiseBrightThreshold;
				RenderObject.NoiseMaterial.DarkThreshold = RenderObject.NoiseDarkThreshold;
				RenderObject.NoiseMaterial.SoftLight = RenderObject.NoiseSoftLight;
				Renderer.DrawSprite(
					RenderObject.ProcessedTexture,
					RenderObject.NoiseTexture,
					RenderObject.NoiseMaterial,
					Color4.White,
					Vector2.Zero,
					RenderObject.TextureSize,
					Vector2.Zero,
					RenderObject.ProcessedUV1,
					noiseUV0,
					noiseUV1
				);
			} finally {
				RenderObject.NoiseBuffer.Texture.RestoreRenderTarget();
			}

			RenderObject.NoiseBuffer.SetRenderParameters(RenderObject);
			RenderObject.MarkBuffersAsDirty = true;
			RenderObject.ProcessedTexture = RenderObject.NoiseBuffer.Texture;
			RenderObject.CurrentBufferSize = (Vector2)RenderObject.NoiseBuffer.Size;
			RenderObject.ProcessedUV1 = (Vector2)RenderObject.ViewportSize / RenderObject.CurrentBufferSize;
		}

		internal new class Buffer : PostProcessingAction.Buffer
		{
			private float brightThreshold = float.NaN;
			private float darkThreshold = float.NaN;
			private float softLight = float.NaN;

			public Buffer(Size size) : base(size) { }

			public bool EqualRenderParameters(PostProcessingRenderObject ro) =>
				!IsDirty &&
				brightThreshold == ro.NoiseBrightThreshold &&
				darkThreshold == ro.NoiseDarkThreshold &&
				softLight == ro.NoiseSoftLight;

			public void SetRenderParameters(PostProcessingRenderObject ro)
			{
				IsDirty = false;
				brightThreshold = ro.NoiseBrightThreshold;
				darkThreshold = ro.NoiseDarkThreshold;
				softLight = ro.NoiseSoftLight;
			}
		}
	}
}
