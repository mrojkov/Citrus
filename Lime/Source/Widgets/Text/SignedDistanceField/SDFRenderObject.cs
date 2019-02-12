using System.Collections.Generic;

namespace Lime.SignedDistanceField
{
	class SDFRenderObject : TextRenderObject
	{
		private static SDFRenderAction[] RenderActions = new SDFRenderAction[] {
			new SDFRenderActionShadows(),
			new SDFRenderActionMain(),
			new SDFRenderActionOverlays(),
			new SDFRenderActionInnerShadows()
		};

		public SDFMaterialProvider SDFMaterialProvider;
		public SignedDistanceFieldMaterial SDFMaterial => SDFMaterialProvider.Material;
		public float Softness;
		public float Dilate;
		public float Thickness;
		public Color4 OutlineColor;
		public List<SDFShadowMaterialProvider> ShadowsMaterialProviders;
		public List<SDFShadowMaterialProvider> OverlaysMaterialProviders;
		public List<SDFInnerShadowMaterialProvider> InnerShadowsMaterialProviders;
		public bool GradientEnabled;
		public ColorGradient Gradient;
		public float GradientAngle;

		protected override void OnRelease()
		{
			SpriteList = null;
			SDFMaterialProvider = null;
			if (OverlaysMaterialProviders != null) {
				foreach (var item in OverlaysMaterialProviders) {
					item.Release();
				}
				OverlaysMaterialProviders = null;
			}
			if (ShadowsMaterialProviders != null) {
				foreach (var item in ShadowsMaterialProviders) {
					item.Release();
				}
				ShadowsMaterialProviders = null;
			}
			if (InnerShadowsMaterialProviders != null) {
				foreach (var item in InnerShadowsMaterialProviders) {
					item.Release();
				}
				InnerShadowsMaterialProviders = null;
			}
		}

		public void Init(SignedDistanceFieldComponent component)
		{
			SDFMaterialProvider = component.SDFMaterialProvider;
			Softness = component.Softness;
			Dilate = component.Dilate;
			OutlineColor = component.OutlineColor;
			Thickness = component.Thickness;
			GradientEnabled = component.GradientEnabled;
			Gradient = component.Gradient;
			GradientAngle = component.GradientAngle;
			if (component.Shadows != null) {
				PrepareShadows(component.Shadows, ref ShadowsMaterialProviders);
			}
			if (component.Overlays != null) {
				PrepareShadows(component.Overlays, ref OverlaysMaterialProviders);
			}
			if (component.InnerShadows != null) {
				PrepareInnerShadows(component.InnerShadows);
			}
		}

		private void PrepareShadows(List<ShadowParams> shadows, ref List<SDFShadowMaterialProvider> providers)
		{
			foreach (var s in shadows) {
				if (!s.Enabled) {
					continue;
				}
				var materialProvider = SDFShadowMaterialProviderPool<SDFShadowMaterialProvider>.Acquire();
				materialProvider.Material.Dilate = s.Dilate;
				materialProvider.Material.Softness = s.Softness;
				materialProvider.Material.Color = s.Color;
				materialProvider.Material.Offset = new Vector2(s.OffsetX, s.OffsetY) * 0.01f;
				if (providers == null) {
					providers = new List<SDFShadowMaterialProvider>();
				}
				providers.Add(materialProvider);
			}
		}

		private void PrepareInnerShadows(List<ShadowParams> shadows)
		{
			foreach (var s in shadows) {
				if (!s.Enabled) {
					continue;
				}
				var materialProvider = SDFInnerShadowMaterialProviderPool<SDFInnerShadowMaterialProvider>.Acquire();
				materialProvider.Material.Dilate = s.Dilate;
				materialProvider.Material.TextDilate = Dilate;
				materialProvider.Material.TextSoftness = Softness;
				materialProvider.Material.Softness = s.Softness;
				materialProvider.Material.Color = s.Color;
				materialProvider.Material.Offset = new Vector2(s.OffsetX, s.OffsetY) * 0.01f;
				if (InnerShadowsMaterialProviders == null) {
					InnerShadowsMaterialProviders = new List<SDFInnerShadowMaterialProvider>();
				}
				InnerShadowsMaterialProviders.Add(materialProvider);
			}
		}

		public override void Render()
		{
			foreach (var action in RenderActions) {
				if (action.EnabledCheck(this)) {
					action.Do(this);
				}
			}
		}

		internal void RenderSpriteList(Sprite.IMaterialProvider materialProvider)
		{
			Renderer.Transform1 = LocalToWorldTransform;
			SpriteList.Render(Color, materialProvider);
		}
	}
}
