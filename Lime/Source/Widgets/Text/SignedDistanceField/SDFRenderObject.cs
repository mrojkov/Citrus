using System.Collections;
using System.Collections.Generic;

namespace Lime.SignedDistanceField
{

	public abstract class BaseSDFRenderObject : TextRenderObject
	{
		public abstract int GetHash();

		protected void RenderSpriteList(Sprite.IMaterialProvider materialProvider, Vector2 offset)
		{
			if (offset.X != 0f || offset.Y != 0f) {
				Renderer.Transform1 = Matrix32.Translation(offset) * LocalToWorldTransform;
			} else {
				Renderer.Transform1 = LocalToWorldTransform;
			}
			SpriteList.Render(Color, materialProvider);
		}

		protected void RenderSpriteList(Sprite.IMaterialProvider materialProvider)
		{
			Renderer.Transform1 = LocalToWorldTransform;
			SpriteList.Render(Color, materialProvider);
		}

		protected override void OnRelease()
		{
			base.OnRelease();
		}
	}

	internal class SDFShadowRenderObject : BaseSDFRenderObject
	{
		private SDFShadowMaterialProvider materialProvider;

		public override int GetHash() => materialProvider.Material.GetHashCode();

		public void Init(ShadowParams shadowParams, float fontSize)
		{
			materialProvider = shadowParams.MaterialProvider as SDFShadowMaterialProvider;
			materialProvider.Material.FontSize = fontSize;
		}

		public override void Render()
		{
			RenderSpriteList(materialProvider, materialProvider.Material.Offset);
		}

		protected override void OnRelease()
		{
			materialProvider = null;
			base.OnRelease();
		}
	}

	internal class SDFInnerShadowRenderObject : BaseSDFRenderObject
	{
		private SDFInnerShadowMaterialProvider materialProvider;

		public override int GetHash() => materialProvider.Material.GetHashCode();

		public void Init(ShadowParams shadowParams, float fontSize)
		{
			materialProvider = shadowParams.MaterialProvider as SDFInnerShadowMaterialProvider;
			materialProvider.Material.FontSize = fontSize;
		}

		public override void Render()
		{
			RenderSpriteList(materialProvider, materialProvider.Material.Offset);
		}

		protected override void OnRelease()
		{
			materialProvider = null;
			base.OnRelease();
		}
	}

	internal class SDFMainRenderObject : BaseSDFRenderObject
	{
		private SDFMaterialProvider materialProvider;

		public override int GetHash() => materialProvider.Material.GetHashCode();

		public void Init(SignedDistanceFieldComponent component, float fontSize)
		{
			materialProvider = component.MaterialProvider;
			materialProvider.Material.FontSize = fontSize;
		}

		public override void Render()
		{
			RenderSpriteList(materialProvider);
		}

		protected override void OnRelease()
		{
			materialProvider = null;
			base.OnRelease();
		}
	}

	public class SDFRenderObjectList : RenderObject
	{
		public List<BaseSDFRenderObject> Objects = new List<BaseSDFRenderObject>();

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
		public static SDFRenderObjectList GetRenderObject(SignedDistanceFieldComponent component, float fontSize)
		{
			var roList = RenderObjectPool<SDFRenderObjectList>.Acquire();
			if (component.Shadows != null) {
				foreach (var s in component.Shadows) {
					if (!s.Enabled) {
						continue;
					}
					var ro = RenderObjectPool<SDFShadowRenderObject>.Acquire();
					ro.Init(s, fontSize);
					roList.Objects.Add(ro);
				}
			}
			var mainRO = RenderObjectPool<SDFMainRenderObject>.Acquire();
			mainRO.Init(component, fontSize);
			roList.Objects.Add(mainRO);
			if (component.InnerShadows != null) {
				foreach (var s in component.InnerShadows) {
					if (!s.Enabled) {
						continue;
					}
					var ro = RenderObjectPool<SDFInnerShadowRenderObject>.Acquire();
					ro.Init(s, fontSize);
					roList.Objects.Add(ro);
				}
			}
			if (component.Overlays != null) {
				foreach (var s in component.Overlays) {
					if (!s.Enabled) {
						continue;
					}
					var ro = RenderObjectPool<SDFShadowRenderObject>.Acquire();
					ro.Init(s, fontSize);
					roList.Objects.Add(ro);
				}
			}
			return roList;
		}
	}
}
