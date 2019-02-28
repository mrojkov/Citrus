using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lime.SignedDistanceField
{
	public class SDFSimpleTextPresenter : IPresenter
	{
		public RenderObject GetRenderObject(Node node)
		{
			var component = node.Components.Get<SignedDistanceFieldComponent>();
			if (component == null) {
				throw new InvalidOperationException();
			}
			var text = node as SimpleText;
			var scale = Mathf.Sqrt(Math.Max(text.LocalToWorldTransform.U.SqrLength, text.LocalToWorldTransform.V.SqrLength));
			var ro = SDFRenderObject.GetRenderObject(component, text.FontHeight * scale);
			var textRO = text.GetRenderObject() as TextRenderObject;
			foreach (var item in ro) {
				item.CaptureRenderState(text);
				item.SpriteList = textRO.SpriteList;
				item.Color = text.GlobalColor * text.TextColor;
			}
			return ro;
		}

		public bool PartialHitTest(Node node, ref HitTestArgs args) => false;

		public IPresenter Clone() => new SDFSimpleTextPresenter();
	}
}
