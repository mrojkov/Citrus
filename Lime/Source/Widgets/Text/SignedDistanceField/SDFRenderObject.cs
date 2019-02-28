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

	public class SDFRenderObjectList : RenderObject, IEnumerable<BaseSDFRenderObject>
	{
		private List<BaseSDFRenderObject> objects = new List<BaseSDFRenderObject>();

		public int Count => objects.Count;

		public RenderObject this[int index] => objects[index];

		public void Add(BaseSDFRenderObject obj)
		{
			objects.Add(obj);
		}

		public override void Render()
		{
			foreach (var ro in objects) {
				ro.Render();
			}
		}

		protected override void OnRelease()
		{
			foreach (var ro in objects) {
				ro.Release();
			}
			objects.Clear();
			base.OnRelease();
		}

		public List<BaseSDFRenderObject>.Enumerator GetEnumerator() => objects.GetEnumerator();

		IEnumerator<BaseSDFRenderObject> IEnumerable<BaseSDFRenderObject>.GetEnumerator() => GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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
					roList.Add(ro);
				}
			}
			var mainRO = RenderObjectPool<SDFMainRenderObject>.Acquire();
			mainRO.Init(component, fontSize);
			roList.Add(mainRO);
			if (component.InnerShadows != null) {
				foreach (var s in component.InnerShadows) {
					if (!s.Enabled) {
						continue;
					}
					var ro = RenderObjectPool<SDFInnerShadowRenderObject>.Acquire();
					ro.Init(s, fontSize);
					roList.Add(ro);
				}
			}
			if (component.Overlays != null) {
				foreach (var s in component.Overlays) {
					if (!s.Enabled) {
						continue;
					}
					var ro = RenderObjectPool<SDFShadowRenderObject>.Acquire();
					ro.Init(s, fontSize);
					roList.Add(ro);
				}
			}
			return roList;
		}
	}
}
