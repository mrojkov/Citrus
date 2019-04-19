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
			public readonly RenderObjectList Objects = new RenderObjectList();

			public override void Render()
			{
				Objects.Render();
			}

			protected override void OnRelease()
			{
				Objects.Clear();
			}
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
				Renderer.DrawRectOutline(Position, Position + Size, Color, Thickness);
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
				Renderer.DrawRect(Position, Position + Size, Color);
			}
		}
	}

	public class CustomPresenter : IPresenter
	{
		public Lime.RenderObject GetRenderObject(Node node)
		{
			var ro = RenderObjectPool<RenderObject>.Acquire();
			var prevRenderer = RendererWrapper.Current;
			RendererWrapper.Current = ro.Renderer;
			try {
				Render(node);
			} finally {
				RendererWrapper.Current = prevRenderer;
			}
			return ro;
		}

		public virtual void Render(Node node) { }

		public virtual bool PartialHitTest(Node node, ref HitTestArgs args) => false;

		public virtual IPresenter Clone() => (IPresenter)MemberwiseClone();

		private class RenderObject : Lime.RenderObject
		{
			public DeferredRendererWrapper Renderer = new DeferredRendererWrapper();

			public override void Render()
			{
				Renderer.ExecuteCommands(RendererWrapper.Current);
			}

			protected override void OnRelease()
			{
				Renderer.ClearCommands();
			}
		}
	}

	public class CustomPresenter<T> : CustomPresenter where T : Node
	{
		public sealed override void Render(Node node) => InternalRender((T)node);

		public sealed override bool PartialHitTest(Node node, ref HitTestArgs args) => InternalPartialHitTest((T)node, ref args);

		protected virtual void InternalRender(T node) { }

		protected virtual bool InternalPartialHitTest(T node, ref HitTestArgs args) => false;
	}

	public class DelegatePresenter<T> : CustomPresenter<T> where T : Node
	{
		public delegate void RenderDelegate(T node);
		public delegate bool PartialHitTestDelegate(T node, ref HitTestArgs args);

		private RenderDelegate render;
		private PartialHitTestDelegate partialHitTest;

		public DelegatePresenter(RenderDelegate render) : this(render, null) { }

		public DelegatePresenter(PartialHitTestDelegate partialHitTest) : this(null, partialHitTest) { }

		public DelegatePresenter(RenderDelegate render, PartialHitTestDelegate partialHitTest)
		{
			this.render = render;
			this.partialHitTest = partialHitTest;
		}

		protected override void InternalRender(T node)
		{
			render?.Invoke(node);
		}

		protected override bool InternalPartialHitTest(T node, ref HitTestArgs args)
		{
			return partialHitTest != null && partialHitTest(node, ref args);
		}
	}
}
