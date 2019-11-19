namespace Lime
{
	internal class PostProcessingActionSharpen : PostProcessingAction
	{
		public override bool EnabledCheck(PostProcessingRenderObject ro) => ro.SharpenEnabled;
		public override PostProcessingAction.Buffer GetTextureBuffer(PostProcessingRenderObject ro) => ro.SharpenBuffer;

		public override void Do(PostProcessingRenderObject ro)
		{
			if (ro.SharpenBuffer.EqualRenderParameters(ro)) {
				ro.ProcessedTexture = ro.SharpenBuffer.Texture;
				ro.CurrentBufferSize = (Vector2)ro.SharpenBuffer.Size;
				ro.ProcessedUV1 = (Vector2)ro.ViewportSize / ro.CurrentBufferSize;
				return;
			}

			ro.PrepareOffscreenRendering(ro.Size);
			ro.SharpenMaterial.Strength = ro.SharpenStrength;
			ro.SharpenMaterial.Limit = ro.SharpenLimit;
			ro.SharpenMaterial.Step = ro.SharpenStep * (ro.ProcessedUV1 / ro.CurrentBufferSize);
			ro.RenderToTexture(ro.SharpenBuffer.Texture, ro.ProcessedTexture, ro.SharpenMaterial, Color4.White, ro.TextureClearingColor);
			ro.CurrentBufferSize = (Vector2)ro.SharpenBuffer.Size;
			ro.ProcessedUV1 = (Vector2)ro.ViewportSize / ro.CurrentBufferSize;

			ro.SharpenBuffer.SetRenderParameters(ro);
			ro.MarkBuffersAsDirty = true;
			ro.ProcessedTexture = ro.SharpenBuffer.Texture;
		}

		internal new class Buffer : PostProcessingAction.Buffer
		{
			private float strength;
			private float limit;
			private Color4 textureClearingColor;

			public Buffer(Size size) : base(size) { }

			public bool EqualRenderParameters(PostProcessingRenderObject ro) =>
				!IsDirty &&
				strength == ro.SharpenStrength &&
				limit == ro.SharpenLimit &&
				textureClearingColor == ro.TextureClearingColor;

			public void SetRenderParameters(PostProcessingRenderObject ro)
			{
				IsDirty = false;
				strength = ro.SharpenStrength;
				limit = ro.SharpenLimit;
				textureClearingColor = ro.TextureClearingColor;
			}
		}
	}
}
