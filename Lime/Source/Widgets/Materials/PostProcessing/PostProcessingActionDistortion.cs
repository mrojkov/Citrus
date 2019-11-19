namespace Lime
{
	internal class PostProcessingActionDistortion : PostProcessingAction
	{
		public override bool EnabledCheck(PostProcessingRenderObject ro) => ro.DistortionEnabled;
		public override PostProcessingAction.Buffer GetTextureBuffer(PostProcessingRenderObject ro) => ro.DistortionBuffer;

		public override void Do(PostProcessingRenderObject ro)
		{
			if (ro.DistortionBuffer.EqualRenderParameters(ro)) {
				ro.ProcessedTexture = ro.DistortionBuffer.Texture;
				ro.CurrentBufferSize = (Vector2)ro.DistortionBuffer.Size;
				ro.ProcessedUV1 = (Vector2)ro.ViewportSize / ro.CurrentBufferSize;
				return;
			}

			ro.PrepareOffscreenRendering(ro.Size);
			ro.DistortionMaterial.BarrelPincushion = ro.DistortionBarrelPincushion;
			ro.DistortionMaterial.ChromaticAberration = ro.DistortionChromaticAberration;
			ro.DistortionMaterial.Red = ro.DistortionRed;
			ro.DistortionMaterial.Green = ro.DistortionGreen;
			ro.DistortionMaterial.Blue = ro.DistortionBlue;
			ro.RenderToTexture(ro.DistortionBuffer.Texture, ro.ProcessedTexture, ro.DistortionMaterial, Color4.White, ro.TextureClearingColor);
			ro.CurrentBufferSize = (Vector2)ro.DistortionBuffer.Size;
			ro.ProcessedUV1 = (Vector2)ro.ViewportSize / ro.CurrentBufferSize;

			ro.DistortionBuffer.SetRenderParameters(ro);
			ro.MarkBuffersAsDirty = true;
			ro.ProcessedTexture = ro.DistortionBuffer.Texture;
		}

		internal new class Buffer : PostProcessingAction.Buffer
		{
			private float barrelPincushion;
			private float chromaticAberration;
			private float red;
			private float green;
			private float blue;
			private Color4 textureClearingColor;

			public Buffer(Size size) : base(size) { }

			public bool EqualRenderParameters(PostProcessingRenderObject ro) =>
				!IsDirty &&
				barrelPincushion == ro.DistortionBarrelPincushion &&
				chromaticAberration == ro.DistortionChromaticAberration &&
				red == ro.DistortionRed &&
				green == ro.DistortionGreen &&
				blue == ro.DistortionBlue &&
				textureClearingColor == ro.TextureClearingColor;

			public void SetRenderParameters(PostProcessingRenderObject ro)
			{
				IsDirty = false;
				barrelPincushion = ro.DistortionBarrelPincushion;
				chromaticAberration = ro.DistortionChromaticAberration;
				red = ro.DistortionRed;
				green = ro.DistortionGreen;
				blue = ro.DistortionBlue;
				textureClearingColor = ro.TextureClearingColor;
			}
		}
	}
}
