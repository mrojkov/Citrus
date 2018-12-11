namespace Lime.SignedDistanceField
{
	internal class SDFRenderActionUnderlay : SDFRenderAction
	{
		public override bool EnabledCheck(SDFRenderObject ro) => ro.UnderlayEnabled;

		public override void Do(SDFRenderObject ro)
		{
			ro.UnderlayMaterial.Dilate = ro.UnderlayDilate;
			ro.UnderlayMaterial.Softness = ro.UnderlaySoftness;
			ro.UnderlayMaterial.Color = ro.UnderlayColor;
			ro.UnderlayMaterial.Offset = ro.UnderlayOffset;
			ro.RenderSpriteList(ro.UnderlayMaterialProvider);
		}
	}
}
