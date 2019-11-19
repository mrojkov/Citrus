namespace Lime
{
	internal class PostProcessingActionBlur : PostProcessingAction
	{
		public override bool EnabledCheck(PostProcessingRenderObject ro) => ro.BlurEnabled;
		public override PostProcessingAction.Buffer GetTextureBuffer(PostProcessingRenderObject ro) => ro.BlurBuffer;

		public override void Do(PostProcessingRenderObject ro)
		{
			ro.ViewportSize = (Size)((Vector2)ro.ViewportSize * ro.BlurTextureScaling);
			if (ro.BlurBuffer.EqualRenderParameters(ro)) {
				ro.ProcessedTexture = ro.BlurBuffer.Texture;
				ro.CurrentBufferSize = (Vector2)ro.BlurBuffer.Size;
				ro.ProcessedUV1 = (Vector2)ro.ViewportSize / ro.CurrentBufferSize;
				return;
			}

			ro.PrepareOffscreenRendering(ro.Size);
			ro.BlurMaterial.Radius = ro.BlurRadius;
			ro.BlurMaterial.BlurShaderId = ro.BlurShader;
			ro.BlurMaterial.Step = ro.ProcessedUV1 * ro.BlurTextureScaling / ro.CurrentBufferSize;
			ro.BlurMaterial.Dir = Vector2.Down;
			ro.BlurMaterial.AlphaCorrection = ro.BlurAlphaCorrection;
			ro.RenderToTexture(ro.FirstTemporaryBuffer.Texture, ro.ProcessedTexture, ro.BlurMaterial, Color4.White, ro.TextureClearingColor);
			ro.CurrentBufferSize = (Vector2)ro.BlurBuffer.Size;
			ro.ProcessedUV1 = (Vector2)ro.ViewportSize / ro.CurrentBufferSize;
			ro.BlurMaterial.Dir = Vector2.Right;
			ro.RenderToTexture(ro.BlurBuffer.Texture, ro.FirstTemporaryBuffer.Texture, ro.BlurMaterial, Color4.White, ro.TextureClearingColor);

			ro.BlurBuffer.SetRenderParameters(ro);
			ro.MarkBuffersAsDirty = true;
			ro.ProcessedTexture = ro.BlurBuffer.Texture;
		}

		internal new class Buffer : PostProcessingAction.Buffer
		{
			private float radius = float.NaN;
			private float textureScaling = float.NaN;
			private float alphaCorrection = float.NaN;
			private Color4 textureClearingColor = Color4.Zero;

			public Buffer(Size size) : base(size) { }

			public bool EqualRenderParameters(PostProcessingRenderObject ro) =>
				!IsDirty &&
				radius == ro.BlurRadius &&
				textureScaling == ro.BlurTextureScaling &&
				alphaCorrection == ro.BlurAlphaCorrection &&
				textureClearingColor == ro.TextureClearingColor;

			public void SetRenderParameters(PostProcessingRenderObject ro)
			{
				IsDirty = false;
				radius = ro.BlurRadius;
				textureScaling = ro.BlurTextureScaling;
				alphaCorrection = ro.BlurAlphaCorrection;
				textureClearingColor = ro.TextureClearingColor;
			}
		}
	}
}
