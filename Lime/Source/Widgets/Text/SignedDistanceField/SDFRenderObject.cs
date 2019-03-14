using System.Collections.Generic;

namespace Lime.SignedDistanceField
{
	class SDFRenderObject : TextRenderObject
	{
		private static SDFRenderAction[] RenderActions = new SDFRenderAction[] {
			new SDFRenderActionOuterShadows(),
			new SDFRenderActionMain(),
			new SDFRenderActionInnerShadows(),
			new SDFRenderActionNewInnerShadows()
		};

		public SDFMaterialProvider SDFMaterialProvider;
		public SignedDistanceFieldMaterial SDFMaterial => SDFMaterialProvider.Material;
		public float Softness;
		public float Dilate;
		public Color4 FaceColor;
		public float Thickness;
		public Color4 OutlineColor;
		public List<SDFShadowMaterialProvider> OuterShadowMaterialProviders;
		public List<SDFShadowMaterialProvider> InnerShadowMaterialProviders;
		public List<SDFInnerShadowMaterialProvider> NewInnerShadowMaterialProviders;
		public bool GradientEnabled;
		public ColorGradient Gradient;
		public float GradientAngle;
		public bool BevelEnabled;
		public Color4 LightColor;
		public float LightAngle;
		public float ReflectionPower;
		public float BevelRoundness;
		public float BevelWidth;

		protected override void OnRelease()
		{
			SpriteList = null;
			SDFMaterialProvider = null;
			if (InnerShadowMaterialProviders != null) {
				foreach (var item in InnerShadowMaterialProviders) {
					item.Release();
				}
				InnerShadowMaterialProviders = null;
			}
			if (OuterShadowMaterialProviders != null) {
				foreach (var item in OuterShadowMaterialProviders) {
					item.Release();
				}
				OuterShadowMaterialProviders = null;
			}
			if (NewInnerShadowMaterialProviders != null) {
				foreach (var item in NewInnerShadowMaterialProviders) {
					item.Release();
				}
				NewInnerShadowMaterialProviders = null;
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
			BevelEnabled = component.BevelEnabled;
			LightAngle = component.LightAngle;
			LightColor = component.LightColor;
			ReflectionPower = component.ReflectionPower;
			BevelRoundness = component.BevelRoundness;
			BevelWidth = component.BevelWidth;
			if (component.Shadows != null) {
				PrepareShadows(component.Shadows);
			}
			if (component.InnerShadows != null) {
				PrepareInnerShadows(component.InnerShadows);
			}
		}

		private void PrepareShadows(List<ShadowParams> shadows)
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
				if (s.Type == ShadowParams.ShadowType.Inner) {
					if (InnerShadowMaterialProviders == null) {
						InnerShadowMaterialProviders = new List<SDFShadowMaterialProvider>();
					}
					InnerShadowMaterialProviders.Add(materialProvider);
				} else if (s.Type == ShadowParams.ShadowType.Outer) {
					if (OuterShadowMaterialProviders == null) {
						OuterShadowMaterialProviders = new List<SDFShadowMaterialProvider>();
					}
					OuterShadowMaterialProviders.Add(materialProvider);
				}
			}
		}

		private void PrepareInnerShadows(List<BaseShadowParams> shadows)
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
				if (NewInnerShadowMaterialProviders == null) {
					NewInnerShadowMaterialProviders = new List<SDFInnerShadowMaterialProvider>();
				}
				NewInnerShadowMaterialProviders.Add(materialProvider);
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
