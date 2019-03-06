using System.Collections;
using System.Collections.Generic;

namespace Lime.SignedDistanceField
{
	internal class BaseSDFRenderObject : TextRenderObject
	{
		public Sprite.IMaterialProvider MaterialProvider;

		public override void Render()
		{
			Renderer.Transform1 = LocalToWorldTransform;
			SpriteList.Render(Color, MaterialProvider);
		}

		protected override void OnRelease()
		{
			MaterialProvider = null;
			base.OnRelease();
		}
	}

	internal class SDFShadowRenderObject : TextRenderObject
	{
		private SDFShadowMaterialProvider materialProvider;

		public void Init(ShadowParams shadowParams)
		{
			materialProvider = shadowParams.MaterialProvider as SDFShadowMaterialProvider;
		}

		public override void Render()
		{
			Renderer.Transform1 = Matrix32.Translation(materialProvider.Offset) * LocalToWorldTransform;
			SpriteList.Render(Color, materialProvider);
		}

		protected override void OnRelease()
		{
			materialProvider = null;
			base.OnRelease();
		}
	}

	public class SDFRenderObjectList : RenderObject
	{
		public List<TextRenderObject> Objects = new List<TextRenderObject>();

		public override void Render()
		{
			foreach (var ro in Objects) {
				ro.Render();
			}
		}

		protected override void OnRelease()
		{
			foreach (var ro in Objects) {
				ro.Release();
			}
			Objects.Clear();
			base.OnRelease();
		}
	}

	public static class SDFRenderObject
	{
		public static SDFRenderObjectList GetRenderObject(SignedDistanceFieldComponent component)
		{
			var roList = RenderObjectPool<SDFRenderObjectList>.Acquire();
			if (component.Shadows != null) {
				foreach (var s in component.Shadows) {
					if (!s.Enabled) {
						continue;
					}
					var ro = RenderObjectPool<SDFShadowRenderObject>.Acquire();
					ro.Init(s);
					roList.Objects.Add(ro);
				}
			}
			var mainRO = RenderObjectPool<BaseSDFRenderObject>.Acquire();
			mainRO.MaterialProvider = component.MaterialProvider;
			roList.Objects.Add(mainRO);
			if (component.InnerShadows != null) {
				foreach (var s in component.InnerShadows) {
					if (!s.Enabled) {
						continue;
					}
					var ro = RenderObjectPool<BaseSDFRenderObject>.Acquire();
					ro.MaterialProvider = s.MaterialProvider;
					roList.Objects.Add(ro);
				}
			}
			if (component.Overlays != null) {
				foreach (var s in component.Overlays) {
					if (!s.Enabled) {
						continue;
					}
					var ro = RenderObjectPool<SDFShadowRenderObject>.Acquire();
					ro.Init(s);
					roList.Objects.Add(ro);
				}
			}
			return roList;
		}
	}
}
