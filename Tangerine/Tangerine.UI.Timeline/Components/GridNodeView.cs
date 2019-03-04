using Lime;
using System.Collections.Generic;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	public class GridNodeView : IGridRowView
	{
		private readonly Node node;

		public Widget GridWidget { get; }
		public Widget OverviewWidget { get; }
		public AwakeBehavior GridWidgetAwakeBehavior => GridWidget.Components.Get<AwakeBehavior>();
		public AwakeBehavior OverviewWidgetAwakeBehavior => OverviewWidget.Components.Get<AwakeBehavior>();

		public GridNodeView(Node node)
		{
			this.node = node;
			GridWidget = new Widget {
				LayoutCell = new LayoutCell {StretchY = 0},
				MinHeight = TimelineMetrics.DefaultRowHeight,
				Presenter = new SyncDelegatePresenter<Widget>(Render)
			};
			GridWidget.Components.Add(new AwakeBehavior());
			OverviewWidget = new Widget {
				LayoutCell = new LayoutCell {StretchY = 0},
				MinHeight = TimelineMetrics.DefaultRowHeight,
				Presenter = new SyncDelegatePresenter<Widget>(Render)
			};
			OverviewWidget.Components.Add(new AwakeBehavior());
		}

		private GridKeyframesRenderer keyframesRenderer = new GridKeyframesRenderer();
		private int animatorsVersion = -1;
		private string animationId;

		protected virtual void Render(Widget widget)
		{
			int v = CalcAnimatorsTotalVersion();
			if (animatorsVersion != v || animationId != Document.Current.AnimationId) {
				animatorsVersion = v;
				animationId = Document.Current.AnimationId;
				keyframesRenderer.ClearCells();
				keyframesRenderer.GenerateCells(node.Animators, Document.Current.AnimationId);
			}
			keyframesRenderer.RenderCells(widget);
		}

		private int CalcAnimatorsTotalVersion()
		{
			int result = node.Animators.Version;
			foreach (var a in node.Animators) {
				result = unchecked(result + a.Version);
			}
			return result;
		}
	}
}
