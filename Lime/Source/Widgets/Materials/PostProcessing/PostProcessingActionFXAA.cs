namespace Lime
{
	internal class PostProcessingActionFXAA : PostProcessingAction
	{
		public override bool EnabledCheck(PostProcessingRenderObject ro) => ro.FXAAEnabled;
		public override PostProcessingAction.Buffer GetTextureBuffer(PostProcessingRenderObject ro) => ro.FXAABuffer;

		public override void Do(PostProcessingRenderObject ro)
		{
			if (ro.FXAABuffer.EqualRenderParameters(ro)) {
				ro.ProcessedTexture = ro.FXAABuffer.Texture;
				ro.CurrentBufferSize = (Vector2)ro.FXAABuffer.Size;
				ro.ProcessedUV1 = (Vector2)ro.ViewportSize / ro.CurrentBufferSize;
				return;
			}

			ro.PrepareOffscreenRendering(ro.Size);
			ro.FXAAMaterial.TexelStep = ro.ProcessedUV1 / ro.CurrentBufferSize;
			ro.FXAAMaterial.LumaTreshold = ro.FXAALumaTreshold;
			ro.FXAAMaterial.MulReduce = ro.FXAAMulReduce;
			ro.FXAAMaterial.MinReduce = ro.FXAAMinReduce;
			ro.FXAAMaterial.MaxSpan = ro.FXAAMaxSpan;
			ro.RenderToTexture(ro.FXAABuffer.Texture, ro.ProcessedTexture, ro.FXAAMaterial, Color4.White, ro.TextureClearingColor);
			ro.CurrentBufferSize = (Vector2)ro.FXAABuffer.Size;
			ro.ProcessedUV1 = (Vector2)ro.ViewportSize / ro.CurrentBufferSize;

			ro.FXAABuffer.SetRenderParameters(ro);
			ro.MarkBuffersAsDirty = true;
			ro.ProcessedTexture = ro.FXAABuffer.Texture;
		}

		internal new class Buffer : PostProcessingAction.Buffer
		{
			private float lumaTreshold;
			private float mulReduce;
			private float minReduce;
			private float maxSpan;
			private Color4 textureClearingColor;

			public Buffer(Size size) : base(size) { }

			public bool EqualRenderParameters(PostProcessingRenderObject ro) =>
				!IsDirty &&
				lumaTreshold == ro.FXAALumaTreshold &&
				mulReduce == ro.FXAAMulReduce &&
				minReduce == ro.FXAAMinReduce &&
				maxSpan == ro.FXAAMaxSpan &&
				textureClearingColor == ro.TextureClearingColor;

			public void SetRenderParameters(PostProcessingRenderObject ro)
			{
				IsDirty = false;
				lumaTreshold = ro.FXAALumaTreshold;
				mulReduce = ro.FXAAMulReduce;
				minReduce = ro.FXAAMinReduce;
				maxSpan = ro.FXAAMaxSpan;
				textureClearingColor = ro.TextureClearingColor;
			}
		}
	}
}
