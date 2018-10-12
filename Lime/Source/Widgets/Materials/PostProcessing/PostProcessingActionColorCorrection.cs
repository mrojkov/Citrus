namespace Lime
{
	internal class PostProcessingActionColorCorrection : PostProcessingAction
	{
		public override bool EnabledCheck(PostProcessingRenderObject ro) => ro.HSLEnabled;
		public override PostProcessingAction.Buffer GetTextureBuffer(PostProcessingRenderObject ro) => ro.ColorCorrectionBuffer;

		public override void Do(PostProcessingRenderObject ro)
		{
			if (ro.ColorCorrectionBuffer.EqualRenderParameters(ro)) {
				ro.ProcessedTexture = ro.ColorCorrectionBuffer.Texture;
				ro.CurrentBufferSize = (Vector2)ro.ColorCorrectionBuffer.Size;
				ro.ProcessedUV1 = (Vector2)ro.ViewportSize / ro.CurrentBufferSize;
				return;
			}

			ro.PrepareOffscreenRendering(ro.Size);
			ro.ColorCorrectionMaterial.HSL = WrappedHSL(ro.HSL);
			ro.ColorCorrectionMaterial.Brightness = ro.Brightness;
			ro.ColorCorrectionMaterial.Contrast = ro.Contrast;
			ro.ColorCorrectionMaterial.Opaque = ro.OpagueRendering;
			ro.RenderToTexture(ro.ColorCorrectionBuffer.Texture, ro.ProcessedTexture, ro.ColorCorrectionMaterial, Color4.White, Color4.Zero);

			ro.ColorCorrectionBuffer.SetRenderParameters(ro);
			ro.MarkBuffersAsDirty = true;
			ro.ProcessedTexture = ro.ColorCorrectionBuffer.Texture;
			ro.CurrentBufferSize = (Vector2)ro.ColorCorrectionBuffer.Size;
			ro.ProcessedUV1 = (Vector2)ro.ViewportSize / ro.CurrentBufferSize;
		}

		internal new class Buffer : PostProcessingAction.Buffer
		{
			private Vector3 hsl = new Vector3(float.NaN, float.NaN, float.NaN);
			private float brightness;
			private float contrast;
			private bool opaque;

			public Buffer(Size size) : base(size) { }

			public bool EqualRenderParameters(PostProcessingRenderObject ro) =>
				!IsDirty && hsl == WrappedHSL(ro.HSL) && brightness == ro.Brightness && contrast == ro.Contrast && opaque == ro.OpagueRendering;

			public void SetRenderParameters(PostProcessingRenderObject ro)
			{
				IsDirty = false;
				hsl = WrappedHSL(ro.HSL);
				brightness = ro.Brightness;
				contrast = ro.Contrast;
				opaque = ro.OpagueRendering;
			}
		}

		private static Vector3 WrappedHSL(Vector3 hsl) => new Vector3(Mathf.Wrap(hsl.X, -0.5f, 0.5f), Mathf.Clamp(hsl.Y, 0f, 2f), Mathf.Clamp(hsl.Z, 0f, 2f));
	}
}
