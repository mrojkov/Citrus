using System;
using System.Collections.Generic;

namespace Lime
{
	public interface IPresenter
	{
		RenderObject GetRenderObject(Node node);
		bool PartialHitTest(Node node, ref HitTestArgs args);
		IPresenter Clone();
	}

	public class DefaultPresenter : IPresenter
	{
		public static readonly DefaultPresenter Instance = new DefaultPresenter();

		public RenderObject GetRenderObject(Node node)
		{
			return node.GetRenderObject();
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

		public Lime.RenderObject GetRenderObject(Node node)
		{
			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.Objects.Clear();
			for (var i = Count - 1; i >= 0; i--) {
				var obj = this[i].GetRenderObject(node);
				if (obj != null) {
					ro.Objects.Add(obj);
				}
			}
			return ro;
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

		private class RenderObject : Lime.RenderObject
		{
			public readonly List<Lime.RenderObject> Objects = new List<Lime.RenderObject>();

			public override void Render()
			{
				foreach (var obj in Objects) {
					obj.Render();
					obj.Rendered = true;
				}
			}
		}
	}

	public class CustomPresenter : IPresenter
	{
		public virtual Lime.RenderObject GetRenderObject(Node node)
		{
			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.Node = node;
			ro.Presenter = this;
			return ro;
		}

		public virtual void Render(Node node) { }

		private class RenderObject : Lime.RenderObject
		{
			public Node Node;
			public CustomPresenter Presenter;

			public override void Render()
			{
				if (!System.Threading.Thread.CurrentThread.IsMain()) {
					throw new InvalidOperationException();
				}
				Presenter.Render(Node);
			}
		}

		public virtual bool PartialHitTest(Node node, ref HitTestArgs args) => false;
		public virtual IPresenter Clone() { return (CustomPresenter)MemberwiseClone(); }
	}

	public class CustomPresenter<T> : CustomPresenter where T: Node
	{
		public override sealed void Render(Node node)
		{
			InternalRender((T)node);
		}

		public override sealed bool PartialHitTest(Node node, ref HitTestArgs args)
		{
			return InternalPartialHitTest((T)node, ref args);
		}

		protected virtual void InternalRender(T node) { }
		protected virtual bool InternalPartialHitTest(T node, ref HitTestArgs args) => false;
	}

	public class DelegatePresenter<T> : CustomPresenter where T: Node
	{
		public delegate bool HitTestDelegate(T node, ref HitTestArgs args);
		readonly Action<T> render;
		readonly HitTestDelegate hitTest;

		public DelegatePresenter(Action<T> render)
		{
			this.render = render;
		}

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
			render?.Invoke((T)node);
		}

		public override bool PartialHitTest(Node node, ref HitTestArgs args)
		{
			return hitTest != null && hitTest((T)node, ref args);
		}
	}

	public class WidgetBoundsPresenter : IPresenter
	{
		public Color4 Color { get; set; }
		public float Thickness { get; set; }
		public bool IgnorePadding { get; set; }

		public WidgetBoundsPresenter(Color4 color, float thickness = 1)
		{
			Color = color;
			Thickness = thickness;
		}

		public Lime.RenderObject GetRenderObject(Node node)
		{
			var widget = (Widget)node;
			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.CaptureRenderState(widget);
			ro.Color = Color * widget.GlobalColor;
			ro.Thickness = Thickness;
			if (IgnorePadding) {
				ro.Position = Vector2.Zero;
				ro.Size = widget.Size;
			} else {
				ro.Position = widget.ContentPosition;
				ro.Size = widget.ContentSize;
			}
			return ro;
		}

		public bool PartialHitTest(Node node, ref HitTestArgs args) => false;

		public IPresenter Clone()
		{
			return (WidgetBoundsPresenter)MemberwiseClone();
		}

		private class RenderObject : WidgetRenderObject
		{
			public Vector2 Position;
			public Vector2 Size;
			public Color4 Color;
			public float Thickness;

			public override void Render()
			{
				PrepareRenderState();
				Renderer.DrawRectOutline(Position, Size, Color, Thickness);
			}
		}
	}

	public class WidgetFlatFillPresenter : IPresenter
	{
		public Color4 Color { get; set; }
		public bool IgnorePadding { get; set; }

		public WidgetFlatFillPresenter(Color4 color)
		{
			Color = color;
		}

		public Lime.RenderObject GetRenderObject(Node node)
		{
			var widget = (Widget)node;
			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.CaptureRenderState(widget);
			ro.Color = Color * widget.GlobalColor;
			if (IgnorePadding) {
				ro.Position = Vector2.Zero;
				ro.Size = widget.Size;
			} else {
				ro.Position = widget.ContentPosition;
				ro.Size = widget.ContentSize;
			}
			return ro;
		}

		public bool PartialHitTest(Node node, ref HitTestArgs args) => false;

		public IPresenter Clone()
		{
			return (WidgetFlatFillPresenter)MemberwiseClone();
		}

		private class RenderObject : WidgetRenderObject
		{
			public Vector2 Position;
			public Vector2 Size;
			public Color4 Color;

			public override void Render()
			{
				PrepareRenderState();
				Renderer.DrawRect(Position, Size, Color);
			}
		}
	}
}
