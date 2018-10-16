namespace Lime
{
	internal class PostProcessingActionNoise : PostProcessingAction
	{
		public override bool EnabledCheck(PostProcessingRenderObject ro) => ro.NoiseEnabled;
		public override PostProcessingAction.Buffer GetTextureBuffer(PostProcessingRenderObject ro) => ro.NoiseBuffer;

		public override void Do(PostProcessingRenderObject ro)
		{
			if (ro.NoiseBuffer.EqualRenderParameters(ro)) {
				ro.ProcessedTexture = ro.NoiseBuffer.Texture;
				ro.CurrentBufferSize = (Vector2)ro.NoiseBuffer.Size;
				ro.ProcessedUV1 = (Vector2)ro.ViewportSize / ro.CurrentBufferSize;
				return;
			}

			ro.PrepareOffscreenRendering(ro.Size);
			if (ro.ProcessedViewport.Width != ro.ViewportSize.Width || ro.ProcessedViewport.Height != ro.ViewportSize.Height) {
				Renderer.Viewport = ro.ProcessedViewport = new Viewport(0, 0, ro.ViewportSize.Width, ro.ViewportSize.Height);
			}
			ro.NoiseBuffer.Texture.SetAsRenderTarget();
			try {
				Renderer.Clear(Color4.Zero);
				var noiseUV0 = ro.NoiseOffset;
				var noiseUV1 = ro.Size / ((Vector2)ro.NoiseTexture.ImageSize * ro.NoiseScale) + ro.NoiseOffset;
				ro.NoiseMaterial.BrightThreshold = ro.NoiseBrightThreshold;
				ro.NoiseMaterial.DarkThreshold = ro.NoiseDarkThreshold;
				ro.NoiseMaterial.SoftLight = ro.NoiseSoftLight;
				ro.NoiseMaterial.Opaque = ro.OpagueRendering;
				Renderer.DrawSprite(
					ro.ProcessedTexture,
					ro.NoiseTexture,
					ro.NoiseMaterial,
					Color4.White,
					Vector2.Zero,
					ro.TextureSize,
					Vector2.Zero,
					ro.ProcessedUV1,
					noiseUV0,
					noiseUV1
				);
			} finally {
				ro.NoiseBuffer.Texture.RestoreRenderTarget();
			}

			ro.NoiseBuffer.SetRenderParameters(ro);
			ro.MarkBuffersAsDirty = true;
			ro.ProcessedTexture = ro.NoiseBuffer.Texture;
			ro.CurrentBufferSize = (Vector2)ro.NoiseBuffer.Size;
			ro.ProcessedUV1 = (Vector2)ro.ViewportSize / ro.CurrentBufferSize;
		}

		internal new class Buffer : PostProcessingAction.Buffer
		{
			private float brightThreshold = float.NaN;
			private float darkThreshold = float.NaN;
			private float softLight = float.NaN;
			private bool opaque;

			public Buffer(Size size) : base(size) { }

			public bool EqualRenderParameters(PostProcessingRenderObject ro) =>
				!IsDirty &&
				brightThreshold == ro.NoiseBrightThreshold &&
				darkThreshold == ro.NoiseDarkThreshold &&
				softLight == ro.NoiseSoftLight &&
				opaque == ro.OpagueRendering;

			public void SetRenderParameters(PostProcessingRenderObject ro)
			{
				IsDirty = false;
				brightThreshold = ro.NoiseBrightThreshold;
				darkThreshold = ro.NoiseDarkThreshold;
				softLight = ro.NoiseSoftLight;
				opaque = ro.OpagueRendering;
			}
		}
	}
}
