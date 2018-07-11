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
#if PROFILE
			var watch = System.Diagnostics.Stopwatch.StartNew();
			node.Render();
			watch.Stop();
			NodeProfiler.RegisterRender(node, watch.ElapsedTicks);
#else
			node.Render();
#endif
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

	public class CustomPresenter<T> : IPresenter where T: Node
	{
		public void Render(Node node) => InternalRender((T)node);
		public bool PartialHitTest(Node node, ref HitTestArgs args) => InternalPartialHitTest((T)node, ref args);

		protected virtual void InternalRender(T node) { }
		protected virtual bool InternalPartialHitTest(T node, ref HitTestArgs args) => false;

		public virtual IPresenter Clone() { return (CustomPresenter<T>)MemberwiseClone(); }
	}

	public class DelegatePresenter<T> : CustomPresenter where T: Node
	{
		public delegate bool HitTestDelegate(T node, ref HitTestArgs args);
		readonly Action<T> render;
		readonly HitTestDelegate hitTest;

		public DelegatePresenter(Action<T> render) { this.render = render; }

		public DelegatePresenter(HitTestDelegate hitTest)
		{
			this.hitTest = hitTest;
		}

		public DelegatePresenter(Action<T> render, HitTestDelegate hitTest)
		{
			this.render = render;
			this.hitTest = hitTest;
		}

		public override void Render(Node node)
		{
			if (render != null)
				render((T)node);
		}

		public override bool PartialHitTest(Node node, ref HitTestArgs args)
		{
			return hitTest != null && hitTest((T)node, ref args);
		}
	}

	public class WidgetBoundsPresenter : CustomPresenter<Widget>
	{
		public Color4 Color { get; set; }
		public float Thickness { get; set; }
		public bool IgnorePadding { get; set; }

		public WidgetBoundsPresenter(Color4 color, float thickness = 1)
		{
			Color = color;
			Thickness = thickness;
		}

		protected override void InternalRender(Widget widget)
		{
			widget.PrepareRendererState();
			if (IgnorePadding) {
				Renderer.DrawRectOutline(Vector2.Zero, widget.Size, Color * widget.GlobalColor, Thickness);
			} else {
				Renderer.DrawRectOutline(widget.ContentPosition, widget.ContentSize, Color * widget.GlobalColor, Thickness);
			}
		}
	}

	public class WidgetFlatFillPresenter : CustomPresenter<Widget>
	{
		public Color4 Color { get; set; }
		public bool IgnorePadding { get; set; }

		public WidgetFlatFillPresenter(Color4 color)
		{
			Color = color;
		}

		protected override void InternalRender(Widget widget)
		{
			widget.PrepareRendererState();
			if (IgnorePadding) {
				Renderer.DrawRect(Vector2.Zero, widget.Size, Color * widget.GlobalColor);
			} else {
				Renderer.DrawRect(widget.ContentPosition, widget.ContentSize, Color * widget.GlobalColor);
			}
		}
	}
}
