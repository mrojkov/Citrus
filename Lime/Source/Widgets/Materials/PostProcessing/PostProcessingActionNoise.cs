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
				var noiseUV1 = new Vector2(RenderObject.Size.X / RenderObject.NoiseTexture.ImageSize.Width, RenderObject.Size.Y / RenderObject.NoiseTexture.ImageSize.Height);
				RenderObject.SoftLightMaterial.Strength = RenderObject.NoiseStrength;
				Renderer.DrawSprite(
					RenderObject.ProcessedTexture,
					RenderObject.NoiseTexture,
					RenderObject.SoftLightMaterial,
					Color4.White,
					Vector2.Zero,
					RenderObject.TextureSize,
					Vector2.Zero,
					RenderObject.ProcessedUV1,
					Vector2.Zero,
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
			private float strength = float.NaN;

			public Buffer(Size size) : base(size) { }

			public bool EqualRenderParameters(PostProcessingRenderObject ro) => !IsDirty && strength == ro.NoiseStrength;

			public void SetRenderParameters(PostProcessingRenderObject ro)
			{
				IsDirty = false;
				strength = ro.NoiseStrength;
			}
		}
	}
}
