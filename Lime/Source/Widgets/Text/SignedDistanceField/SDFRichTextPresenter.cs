using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lime.SignedDistanceField
{
	public class SDFRichTextPresenter : IPresenter
	{
		public Lime.RenderObject GetRenderObject(Node node)
		{
			var ro = RenderObjectPool<RenderObject>.Acquire();
			var text = node as RichText;
			var textRO = text.GetRenderObject() as RichText.RenderObject;
			var scale = Mathf.Sqrt(Math.Max(text.LocalToWorldTransform.U.SqrLength, text.LocalToWorldTransform.V.SqrLength));
			foreach (var styleRO in textRO) {
				var component = styleRO.Style.Components.Get<SignedDistanceFieldComponent>();
				if (component != null) {
					var sdfRO = SDFRenderObject.GetRenderObject(component, styleRO.Style.Size * scale);
					foreach (var item in sdfRO) {
						item.CaptureRenderState(text);
						item.SpriteList = styleRO.SpriteList;
						item.Color = text.GlobalColor * styleRO.Style.TextColor;
					}
					ro.Add(sdfRO);
				} else {
					ro.Add(styleRO);
				}
			}
			return ro;
		}

		public bool PartialHitTest(Node node, ref HitTestArgs args) => false;

		public IPresenter Clone() => new SDFSimpleTextPresenter();

		private class RenderObject : Lime.RenderObject, IEnumerable<Lime.RenderObject>
		{
			private List<Lime.RenderObject> objects = new List<Lime.RenderObject>();

			public int Count => objects.Count;

			public Lime.RenderObject this[int index] => objects[index];

			public void Add(Lime.RenderObject obj)
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

			public List<Lime.RenderObject>.Enumerator GetEnumerator() => objects.GetEnumerator();

			IEnumerator<Lime.RenderObject> IEnumerable<Lime.RenderObject>.GetEnumerator() => GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
