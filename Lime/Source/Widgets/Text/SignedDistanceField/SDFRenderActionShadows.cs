using System.Collections.Generic;

namespace Lime.SignedDistanceField
{
	internal class SDFRenderActionOverlays : SDFRenderAction
	{
		public override bool EnabledCheck(SDFRenderObject ro) =>
			ro.OverlaysMaterialProviders != null &&
			ro.OverlaysMaterialProviders.Count > 0;

		public override void Do(SDFRenderObject ro)
		{
			foreach (var provider in ro.OverlaysMaterialProviders) {
				ro.RenderSpriteList(provider, provider.Material.Offset);
			}
		}
	}

	internal class SDFRenderActionShadows : SDFRenderAction
	{
		public override bool EnabledCheck(SDFRenderObject ro) =>
			ro.ShadowsMaterialProviders != null &&
			ro.ShadowsMaterialProviders.Count > 0;

		public override void Do(SDFRenderObject ro)
		{
			foreach (var provider in ro.ShadowsMaterialProviders) {
				ro.RenderSpriteList(provider, provider.Material.Offset);
			}
		}
	}

	internal class SDFRenderActionInnerShadows : SDFRenderAction
	{
		public override bool EnabledCheck(SDFRenderObject ro) =>
			ro.InnerShadowsMaterialProviders != null &&
			ro.InnerShadowsMaterialProviders.Count > 0;

		public override void Do(SDFRenderObject ro)
		{
			foreach (var provider in ro.InnerShadowsMaterialProviders) {
				ro.RenderSpriteList(provider, provider.Material.Offset);
			}
		}
	}
}
