using System;
using System.Collections.Generic;

namespace Lime
{
	public interface IPresenter
	{
		void Render(Node node);
		bool PartialHitTest(Node node, ref HitTestArgs args);
		IPresenter Clone();
	}

	public class DefaultPresenter : IPresenter
	{
		public static readonly DefaultPresenter Instance = new DefaultPresenter();

		public void Render(Node node)
		{
			node.Render();
		}

		public bool PartialHitTest(Node node, ref HitTestArgs args)
		{
			return node.PartialHitTest(ref args);
		}

		public IPresenter Clone()
		{
			return Instance;
		}
	}

	public class CompoundPresenter : List<IPresenter>, IPresenter
	{
		public CompoundPresenter() { }

		public void Push(IPresenter presenter)
		{
			Insert(0, presenter);
		}

		public CompoundPresenter(IPresenter presenter)
		{
			if (presenter != null) {
				Add(presenter);
			}
		}

		public void Render(Node node)
		{
			for (int i = Count - 1; i >= 0; i--) {
				this[i].Render(node);
			}
		}

		public bool PartialHitTest(Node node, ref HitTestArgs args)
		{
			foreach (var i in this) {
				if (i.PartialHitTest(node, ref args)) {
					return true;
				}
			}
			return false;
		}

		public IPresenter Clone()
		{
			var r = new CompoundPresenter();
			foreach (var i in this) {
				r.Add(i.Clone());
			}
			return r;
		}
	}

	public class CustomPresenter : IPresenter
	{
		public virtual void Render(Node node) { }
		public virtual bool PartialHitTest(Node node, ref HitTestArgs args) { return false; }
		public virtual IPresenter Clone() { return (CustomPresenter)MemberwiseClone(); }
	}

	public class DelegatePresenter<T> : CustomPresenter where T: Node
	{
		readonly Action<T> render;

		public DelegatePresenter(Action<T> render) { this.render = render; }

		public override void Render(Node node) { render(node as T); }
	}

	public class WidgetBoundsPresenter : CustomPresenter
	{
		public Color4 Color { get; set; }
		public float Thickness { get; set; }

		public WidgetBoundsPresenter(Color4 color, float thickness = 0)
		{
			Color = color;
			Thickness = thickness;
		}

		public override void Render(Node node)
		{
			var widget = node.AsWidget;
			widget.PrepareRendererState();
			var t = Thickness > 0 ? Thickness : 1 / CommonWindow.Current.PixelScale;
			Renderer.DrawRectOutline(Vector2.Zero, widget.Size, Color * widget.GlobalColor, t);
		}
	}

	public class WidgetFlatFillPresenter : CustomPresenter
	{
		public Color4 Color { get; set; }

		public WidgetFlatFillPresenter(Color4 color)
		{
			Color = color;
		}

		public override void Render(Node node)
		{
			var widget = node.AsWidget;
			widget.PrepareRendererState();
			Renderer.DrawRect(widget.ContentPosition, widget.ContentSize, Color * widget.GlobalColor);
		}
	}
}