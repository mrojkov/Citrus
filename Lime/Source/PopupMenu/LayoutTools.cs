using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime.PopupMenu
{
	public class ExpandSiblingsToParent : Node
	{
		protected override void SelfUpdate(float delta)
		{
			foreach (var item in Parent.Nodes.OfType<Widget>()) {
				item.Position = Vector2.Zero;
				item.Size = Parent.AsWidget.Size;
				item.Pivot = Vector2.Zero;
			}
		}
	}

	public class CenterSiblingsVertically : Node
	{
		protected override void SelfUpdate(float delta)
		{
			foreach (var item in Parent.Nodes.OfType<Widget>()) {
				item.Y = (Parent.AsWidget.Height - item.Height) / 2;
				item.Pivot *= new Vector2(1, 0);
			}
		}
	}

	public class StackSiblingsHorizontally : Node
	{
		public string StretchWidget { get; set; }

		public StackSiblingsHorizontally() { }

		public StackSiblingsHorizontally(string stretch)
		{
			StretchWidget = stretch;
		}

		protected override void SelfUpdate(float delta)
		{
			var stretch = Parent.Nodes.TryFind(StretchWidget);
			float w = 0;
			foreach (var item in Parent.Nodes.OfType<Widget>()) {
				if (item != stretch) {
					w += item.Width;
				}
			}
			float x = 0;
			foreach (var item in Parent.Nodes.OfType<Widget>()) {
				item.X = x;
				if (item == stretch) {
					item.Width = Math.Max(0, Parent.AsWidget.Width - w);
				}
				item.Pivot *= new Vector2(0, 1);
				x += item.Width;
			}
		}
	}

	public class Spacer : Widget
	{
		public Spacer(float size)
		{
			Size = new Vector2(size, size);
		}

		public Spacer(float width, float height)
		{
			Size = new Vector2(width, height);
		}
	}
}
