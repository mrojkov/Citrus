using System;

namespace Lime
{
	public interface IPresenter
	{
		void Render(Node node);
		bool PerformHitTest(Node node, Vector2 point);
		IPresenter Clone();
	}

	public class DefaultPresenter : IPresenter
	{
		public static readonly DefaultPresenter Instance = new DefaultPresenter();

		public void Render(Node node)
		{
			node.Render();
		}

		public bool PerformHitTest(Node node, Vector2 point)
		{
			return node.PerformHitTest(point);
		}

		public IPresenter Clone()
		{
			return Instance;
		}
	}

	public class CustomPresenter : IPresenter
	{
		private IPresenter previous;

		public CustomPresenter() { }

		public CustomPresenter(IPresenter previous)
		{
			this.previous = previous;
		}

		public virtual void Render(Node node)
		{
			if (previous != null) {
				previous.Render(node);
			}
		}

		public virtual bool PerformHitTest(Node node, Vector2 point)
		{
			return previous != null && previous.PerformHitTest(node, point);
		}

		public virtual IPresenter Clone()
		{
			var r = (CustomPresenter)MemberwiseClone();
			if (previous != null) {
				r.previous = previous.Clone();
			}
			return r;
		}
	}

	public class DelegatePresenter<T> : CustomPresenter where T: Node
	{
		readonly Action<T> render;
		readonly bool reverseOrder;

		public DelegatePresenter(Action<T> render, IPresenter previous = null, bool reverseOrder = false)
			: base(previous)
		{
			this.render = render;
			this.reverseOrder = reverseOrder;
		}

		public override void Render(Node node)
		{
			if (reverseOrder) {
				render(node as T);
				base.Render(node);
			} else {
				base.Render(node);
				render(node as T);
			}
		}
	}

	public class WidgetBoundsPresenter : CustomPresenter
	{
		public readonly Color4 Color;
		public readonly float Thickness;

		public WidgetBoundsPresenter(Color4 color, float thickness = 0, IPresenter previous = null) : base(previous)
		{
			Color = color;
			Thickness = thickness;
		}

		public override void Render(Node node)
		{
			base.Render(node);
			node.AsWidget.PrepareRendererState();
			var t = Thickness > 0 ? Thickness : 1 / Window.Current.PixelScale;
			Renderer.DrawRectOutline(Vector2.Zero, node.AsWidget.Size, Color, t);
		}
	}

	public class WidgetFlatFillPresenter : CustomPresenter
	{
		public readonly Color4 Color;

		public WidgetFlatFillPresenter(Color4 color, IPresenter previous = null) : base(previous)
		{
			Color = color;
		}

		public override void Render(Node node)
		{
			base.Render(node);
			node.AsWidget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, node.AsWidget.Size, Color);
		}
	}
}