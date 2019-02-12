namespace Lime.SignedDistanceField
{
	internal class SDFRenderActionMain : SDFRenderAction
	{
		public override bool EnabledCheck(SDFRenderObject ro) => true;

		public override void Do(SDFRenderObject ro)
		{
			ro.SDFMaterial.Softness = ro.Softness;
			ro.SDFMaterial.Dilate = ro.Dilate;
			ro.SDFMaterial.Thickness = ro.Thickness;
			ro.SDFMaterial.OutlineColor = ro.OutlineColor;
			ro.SDFMaterial.Gradient = ro.Gradient;
			ro.SDFMaterial.GradientEnabled = ro.GradientEnabled;
			ro.SDFMaterial.GradientAngle = ro.GradientAngle;
			ro.RenderSpriteList(ro.SDFMaterialProvider);
		}
	}
}
