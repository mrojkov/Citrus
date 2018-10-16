namespace Lime
{
	internal class PostProcessingActionVignette : PostProcessingAction
	{
		public override bool EnabledCheck(PostProcessingRenderObject ro) => ro.VignetteEnabled && ro.IsNotDebugViewMode;

		public override void Do(PostProcessingRenderObject ro)
		{
			ro.FinalizeOffscreenRendering();

			ro.VignetteMaterial.Radius = ro.VignetteRadius;
			ro.VignetteMaterial.Softness = ro.VignetteSoftness;
			ro.VignetteMaterial.UV1 = Vector2.One / (ro.ProcessedUV1 * ro.VignetteScale);
			ro.VignetteMaterial.UVOffset = ro.VignettePivot / ro.VignetteScale;
			ro.VignetteMaterial.Color = ro.VignetteColor;
			ro.RenderTexture(ro.TransparentTexture, ro.VignetteMaterial);
		}
	}
}
