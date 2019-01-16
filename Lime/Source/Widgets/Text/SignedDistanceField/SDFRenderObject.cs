using System.Collections.Generic;

namespace Lime.SignedDistanceField
{
	class SDFRenderObject : RenderObject
	{
		public readonly RenderObjectList Objects = new RenderObjectList();
		public SpriteList SpriteList;
		public SDFRenderAction[] RenderActions;
		public IMaterial Material;
		public Matrix32 LocalToWorldTransform;
		public Vector2 Position;
		public Vector2 Size;
		public Color4 Color;
		public Vector2 UV0;
		public Vector2 UV1;

		public SDFMaterialProvider SDFMaterialProvider;
		public SignedDistanceFieldMaterial SDFMaterial => SDFMaterialProvider.Material;
		public float Softness;
		public float Dilate;
		public Color4 FaceColor;
		public float Thickness;
		public Color4 OutlineColor;
		public List<SDFShadowMaterialProvider> OuterShadowMaterialProviders;
		public List<SDFShadowMaterialProvider> InnerShadowMaterialProviders;
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
			Objects.Clear();
			SpriteList = null;
			Material = null;
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
			SpriteList.Render(FaceColor, materialProvider);
		}
	}
}
