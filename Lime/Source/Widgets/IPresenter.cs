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

	public class DelegatePresenter : CustomPresenter
	{
		readonly Action<Node> render;
		readonly bool reverseOrder;

		public DelegatePresenter(Action<Node> render, IPresenter previous = null, bool reverseOrder = false)
			: base(previous)
		{
			this.render = render;
			this.reverseOrder = reverseOrder;
		}

		public override void Render(Node node)
		{
			if (reverseOrder) {
				render(node);
				base.Render(node);
			} else {
				base.Render(node);
				render(node);
			}
		}
	}
}