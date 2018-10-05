namespace Lime
{
	internal class PostProcessingActionHSL : PostProcessingAction
	{
		public override bool Enabled => RenderObject.HSLEnabled;
		public override PostProcessingAction.Buffer TextureBuffer => RenderObject.HSLBuffer;

		public override void Do()
		{
			if (RenderObject.HSLBuffer.EqualRenderParameters(RenderObject)) {
				RenderObject.ProcessedTexture = RenderObject.HSLBuffer.Texture;
				RenderObject.CurrentBufferSize = (Vector2)RenderObject.HSLBuffer.Size;
				RenderObject.ProcessedUV1 = (Vector2)RenderObject.ViewportSize / RenderObject.CurrentBufferSize;
				return;
			}

			RenderObject.PrepareOffscreenRendering(RenderObject.Size);
			RenderObject.HSLMaterial.HSL = WrappedHSL(RenderObject.HSL);
			RenderObject.HSLMaterial.Opaque = RenderObject.OpagueRendering;
			RenderObject.RenderToTexture(RenderObject.HSLBuffer.Texture, RenderObject.ProcessedTexture, RenderObject.HSLMaterial, Color4.White, Color4.Zero);

			RenderObject.HSLBuffer.SetRenderParameters(RenderObject);
			RenderObject.MarkBuffersAsDirty = true;
			RenderObject.ProcessedTexture = RenderObject.HSLBuffer.Texture;
			RenderObject.CurrentBufferSize = (Vector2)RenderObject.HSLBuffer.Size;
			RenderObject.ProcessedUV1 = (Vector2)RenderObject.ViewportSize / RenderObject.CurrentBufferSize;
		}

		internal new class Buffer : PostProcessingAction.Buffer
		{
			private Vector3 hsl = new Vector3(float.NaN, float.NaN, float.NaN);
			private bool opaque;

			public Buffer(Size size) : base(size) { }

			public bool EqualRenderParameters(PostProcessingRenderObject ro) => !IsDirty && hsl == WrappedHSL(ro.HSL) && opaque == ro.OpagueRendering;

			public void SetRenderParameters(PostProcessingRenderObject ro)
			{
				IsDirty = false;
				hsl = WrappedHSL(ro.HSL);
				opaque = ro.OpagueRendering;
			}
		}

		private static Vector3 WrappedHSL(Vector3 hsl) => new Vector3(Mathf.Wrap(hsl.X, -0.5f, 0.5f), Mathf.Clamp(hsl.Y, 0f, 2f), Mathf.Clamp(hsl.Z, 0f, 2f));
	}
}
