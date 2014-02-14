using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	[ProtoContract]
	public class LinearLayout : Node
	{
		[ProtoMember(1)]
		public bool Horizontal { get; set; }

		public override void Update(int delta)
		{
			if (Parent != null && Parent.AsWidget != null) {
				if (Horizontal) {
					UpdateForHorizontalOrientation();
				} else {
					UpdateForVerticalOrientation();
				}
			}
			base.Update(delta);
		}

		private void UpdateForHorizontalOrientation()
		{
			int count = 0;
			foreach (var node in Parent.Nodes) {
				if (node.AsWidget != null) {
					count++;
				}
			}
			if (count == 0) return;
			Vector2 parentSize = Parent.AsWidget.Size;
			float x = 0;
			Widget lastWidget = null;
			foreach (var node in Parent.Nodes) {
				if (node.AsWidget != null) {
					float w = (parentSize.X / count).Floor();
					node.AsWidget.Position = new Vector2(x, 0);
					node.AsWidget.Size = new Vector2(w, parentSize.Y);
					lastWidget = node.AsWidget;
					x += w;
				}
			}
			if (lastWidget != null) {
				lastWidget.Width += parentSize.X - x;
			}
		}

		private void UpdateForVerticalOrientation()
		{
			int count = 0;
			foreach (var node in Parent.Nodes) {
				if (node.AsWidget != null) {
					count++;
				}
			}
			if (count == 0) return;
			Vector2 parentSize = Parent.AsWidget.Size;
			float y = 0;
			Widget lastWidget = null;
			foreach (var node in Parent.Nodes) {
				if (node.AsWidget != null) {
					float h = (parentSize.Y / count).Floor();
					node.AsWidget.Position = new Vector2(0, y);
					node.AsWidget.Size = new Vector2(parentSize.X, h);
					y += h;
					lastWidget = node.AsWidget;
				}
			}
			if (lastWidget != null) {
				lastWidget.Height += parentSize.Y - y;
			}
		}
	}
}
