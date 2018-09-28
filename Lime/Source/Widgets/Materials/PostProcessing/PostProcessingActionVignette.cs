namespace Lime
{
	internal class PostProcessingActionVignette : PostProcessingAction
	{
		public override bool Enabled => RenderObject.VignetteEnabled && RenderObject.IsNotDebugViewMode;

		public override void Do()
		{
			RenderObject.FinalizeOffscreenRendering();

			RenderObject.VignetteMaterial.Radius = RenderObject.VignetteRadius;
			RenderObject.VignetteMaterial.Softness = RenderObject.VignetteSoftness;
			RenderObject.VignetteMaterial.UV1 = Vector2.One / (RenderObject.ProcessedUV1 * RenderObject.VignetteScale);
			RenderObject.VignetteMaterial.UVOffset = RenderObject.VignettePivot / RenderObject.VignetteScale;
			RenderObject.VignetteMaterial.Color = RenderObject.VignetteColor;
			RenderObject.RenderTexture(RenderObject.TransparentTexture, RenderObject.VignetteMaterial);
		}
	}
}
