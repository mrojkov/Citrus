using System.Collections.Generic;

namespace Lime.SignedDistanceField
{
	internal class SDFRenderActionInnerShadows : SDFRenderAction
	{
		public override bool EnabledCheck(SDFRenderObject ro) =>
			ro.InnerShadowMaterialProviders != null &&
			ro.InnerShadowMaterialProviders.Count > 0;

		public override void Do(SDFRenderObject ro)
		{
			foreach (var provider in ro.InnerShadowMaterialProviders) {
				ro.RenderSpriteList(provider);
			}
		}
	}

	internal class SDFRenderActionOuterShadows : SDFRenderAction
	{
		public override bool EnabledCheck(SDFRenderObject ro) =>
			ro.OuterShadowMaterialProviders != null &&
			ro.OuterShadowMaterialProviders.Count > 0;

		public override void Do(SDFRenderObject ro)
		{
			foreach (var provider in ro.OuterShadowMaterialProviders) {
				ro.RenderSpriteList(provider);
			}
		}
	}
}
