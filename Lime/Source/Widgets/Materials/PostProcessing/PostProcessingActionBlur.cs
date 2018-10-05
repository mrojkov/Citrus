namespace Lime
{
	internal class PostProcessingActionBlur : PostProcessingAction
	{
		public override bool Enabled => RenderObject.BlurEnabled;
		public override PostProcessingAction.Buffer TextureBuffer => RenderObject.BlurBuffer;

		public override void Do()
		{
			RenderObject.ViewportSize = (Size)((Vector2)RenderObject.ViewportSize * RenderObject.BlurTextureScaling);
			if (RenderObject.BlurBuffer.EqualRenderParameters(RenderObject)) {
				RenderObject.ProcessedTexture = RenderObject.BlurBuffer.Texture;
				RenderObject.CurrentBufferSize = (Vector2)RenderObject.BlurBuffer.Size;
				RenderObject.ProcessedUV1 = (Vector2)RenderObject.ViewportSize / RenderObject.CurrentBufferSize;
				return;
			}

			RenderObject.PrepareOffscreenRendering(RenderObject.Size);
			RenderObject.BlurMaterial.Radius = RenderObject.BlurRadius;
			RenderObject.BlurMaterial.BlurShaderId = RenderObject.BlurShader;
			RenderObject.BlurMaterial.Step = RenderObject.ProcessedUV1 * RenderObject.BlurTextureScaling / RenderObject.CurrentBufferSize;
			RenderObject.BlurMaterial.Dir = Vector2.Down;
			RenderObject.BlurMaterial.AlphaCorrection = RenderObject.BlurAlphaCorrection;
			RenderObject.BlurMaterial.Opaque = RenderObject.OpagueRendering;
			RenderObject.RenderToTexture(RenderObject.FirstTemporaryBuffer.Texture, RenderObject.ProcessedTexture, RenderObject.BlurMaterial, Color4.White, RenderObject.BlurBackgroundColor);
			RenderObject.CurrentBufferSize = (Vector2)RenderObject.BlurBuffer.Size;
			RenderObject.ProcessedUV1 = (Vector2)RenderObject.ViewportSize / RenderObject.CurrentBufferSize;
			RenderObject.BlurMaterial.Dir = Vector2.Right;
			RenderObject.RenderToTexture(RenderObject.BlurBuffer.Texture, RenderObject.FirstTemporaryBuffer.Texture, RenderObject.BlurMaterial, Color4.White, RenderObject.BlurBackgroundColor);

			RenderObject.BlurBuffer.SetRenderParameters(RenderObject);
			RenderObject.MarkBuffersAsDirty = true;
			RenderObject.ProcessedTexture = RenderObject.BlurBuffer.Texture;
		}

		internal new class Buffer : PostProcessingAction.Buffer
		{
			private float radius = float.NaN;
			private float textureScaling = float.NaN;
			private float alphaCorrection = float.NaN;
			private Color4 backgroundColor = Color4.Zero;
			private bool opaque;

			public Buffer(Size size) : base(size) { }

			public bool EqualRenderParameters(PostProcessingRenderObject ro) =>
				!IsDirty &&
				radius == ro.BlurRadius &&
				textureScaling == ro.BlurTextureScaling &&
				alphaCorrection == ro.BlurAlphaCorrection &&
				backgroundColor == ro.BlurBackgroundColor &&
				opaque == ro.OpagueRendering;

			public void SetRenderParameters(PostProcessingRenderObject ro)
			{
				IsDirty = false;
				radius = ro.BlurRadius;
				textureScaling = ro.BlurTextureScaling;
				alphaCorrection = ro.BlurAlphaCorrection;
				backgroundColor = ro.BlurBackgroundColor;
				opaque = ro.OpagueRendering;
			}
		}
	}
}
