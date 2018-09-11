using System;
using Lime;

namespace Tangerine.Core
{
	public class SyncCustomPresenter : IPresenter
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
			public SyncCustomPresenter Presenter;

			public override void Render()
			{
				if (!System.Threading.Thread.CurrentThread.IsMain()) {
					throw new InvalidOperationException();
				}
				Presenter.Render(Node);
			}
		}

		public virtual bool PartialHitTest(Node node, ref HitTestArgs args) => false;
		public virtual IPresenter Clone() { return (SyncCustomPresenter)MemberwiseClone(); }
	}

	public class SyncCustomPresenter<T> : SyncCustomPresenter where T : Node
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

	public class SyncDelegatePresenter<T> : SyncCustomPresenter where T : Node
	{
		public delegate bool HitTestDelegate(T node, ref HitTestArgs args);
		readonly Action<T> render;
		readonly HitTestDelegate hitTest;

		public SyncDelegatePresenter(Action<T> render)
		{
			this.render = render;
		}

		public SyncDelegatePresenter(HitTestDelegate hitTest)
		{
			this.hitTest = hitTest;
		}

		public SyncDelegatePresenter(Action<T> render, HitTestDelegate hitTest)
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
}
