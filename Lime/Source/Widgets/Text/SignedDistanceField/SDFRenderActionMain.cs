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
			ro.SDFMaterial.BevelEnabled = ro.BevelEnabled;
			ro.SDFMaterial.BevelRoundness = ro.BevelRoundness;
			ro.SDFMaterial.BevelWidth = ro.BevelWidth;
			ro.SDFMaterial.LightAngle = ro.LightAngle;
			ro.SDFMaterial.LightColor = ro.LightColor;
			ro.SDFMaterial.ReflectionPower = ro.ReflectionPower;
			ro.RenderSpriteList(ro.SDFMaterialProvider);
		}
	}
}
