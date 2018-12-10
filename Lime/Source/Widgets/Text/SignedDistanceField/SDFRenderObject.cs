using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lime
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
		public SDFUnderlayMaterialProvider UnderlayMaterialProvider;
		public SDFUnderlayMaterial UnderlayMaterial => UnderlayMaterialProvider.Material;
		public Vector2 UnderlayOffset;
		public float UnderlaySoftness;
		public float UnderlayDilate;
		public bool UnderlayEnabled;
		public Color4 UnderlayColor;
		public bool GradientEnabled;
		public ColorGradient Gradient;
		public float GradientAngle;

		protected override void OnRelease()
		{
			Objects.Clear();
			SpriteList = null;
			Material = null;
			SDFMaterialProvider = null;
			UnderlayMaterialProvider = null;
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
