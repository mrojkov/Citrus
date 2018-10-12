namespace Lime
{
	internal class PostProcessingActionColorCorrection : PostProcessingAction
	{
		public override bool Enabled => RenderObject.HSLEnabled;
		public override PostProcessingAction.Buffer TextureBuffer => RenderObject.ColorCorrectionBuffer;

		public override void Do()
		{
			if (RenderObject.ColorCorrectionBuffer.EqualRenderParameters(RenderObject)) {
				RenderObject.ProcessedTexture = RenderObject.ColorCorrectionBuffer.Texture;
				RenderObject.CurrentBufferSize = (Vector2)RenderObject.ColorCorrectionBuffer.Size;
				RenderObject.ProcessedUV1 = (Vector2)RenderObject.ViewportSize / RenderObject.CurrentBufferSize;
				return;
			}

			RenderObject.PrepareOffscreenRendering(RenderObject.Size);
			RenderObject.ColorCorrectionMaterial.HSL = WrappedHSL(RenderObject.HSL);
			RenderObject.ColorCorrectionMaterial.Brightness = RenderObject.Brightness;
			RenderObject.ColorCorrectionMaterial.Contrast = RenderObject.Contrast;
			RenderObject.ColorCorrectionMaterial.Opaque = RenderObject.OpagueRendering;
			RenderObject.RenderToTexture(RenderObject.ColorCorrectionBuffer.Texture, RenderObject.ProcessedTexture, RenderObject.ColorCorrectionMaterial, Color4.White, Color4.Zero);

			RenderObject.ColorCorrectionBuffer.SetRenderParameters(RenderObject);
			RenderObject.MarkBuffersAsDirty = true;
			RenderObject.ProcessedTexture = RenderObject.ColorCorrectionBuffer.Texture;
			RenderObject.CurrentBufferSize = (Vector2)RenderObject.ColorCorrectionBuffer.Size;
			RenderObject.ProcessedUV1 = (Vector2)RenderObject.ViewportSize / RenderObject.CurrentBufferSize;
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
