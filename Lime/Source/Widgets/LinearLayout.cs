using Yuzu;

namespace Lime
{
	// TODO: Get rid of the class, use Widget.Layout technology instead.
	public class LinearLayout : Node
	{
		[TangerineIgnore]
		class LayoutHandler : CommonLayout, ILayout
		{
			private LinearLayout layoutData;

			public LayoutHandler(LinearLayout layoutNode)
			{
				this.layoutData = layoutNode;
			}

			public override void OnSizeChanged(Widget widget, Vector2 sizeDelta)
			{
				if (layoutData.Horizontal) {
					UpdateForHorizontalOrientation(widget);
				} else {
					UpdateForVerticalOrientation(widget);
				}
			}

			private void UpdateForHorizontalOrientation(Widget widget)
			{
				int count = CalcVisibleWidgetsCount(widget);
				if (count == 0) return;
				Vector2 parentSize = widget.Size;
				float x = 0;
				Widget lastWidget = null;
				foreach (var node in widget.Nodes) {
					if (ShouldProcessNode(node)) {
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

			private int CalcVisibleWidgetsCount(Widget widget)
			{
				int count = 0;
				foreach (var node in widget.Nodes) {
					if (ShouldProcessNode(node)) {
						count++;
					}
				}
				return count;
			}

			private bool ShouldProcessNode(Node node)
			{
				return node.AsWidget != null && (node.AsWidget.Visible || layoutData.ProcessHidden);
			}

			private void UpdateForVerticalOrientation(Widget widget)
			{
				int count = CalcVisibleWidgetsCount(widget);
				if (count == 0) return;
				Vector2 parentSize = widget.Size;
				float y = 0;
				Widget lastWidget = null;
				foreach (var node in widget.Nodes) {
					if (ShouldProcessNode(node)) {
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

		[YuzuMember]
		public bool Horizontal { get; set; }

		[YuzuMember]
		public bool ProcessHidden { get; set; }

		public LinearLayout()
		{
			RenderChainBuilder = null;
		}

		protected override void OnParentChanged(Node oldParent)
		{
			base.OnParentChanged(oldParent);
			if (Parent != null) {
				Parent.AsWidget.Layout = new LayoutHandler(this);
			}
		}

		public override void AddToRenderChain(RenderChain chain)
		{
		}
	}
}
