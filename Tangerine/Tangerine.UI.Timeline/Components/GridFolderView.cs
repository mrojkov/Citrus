using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	public class GridFolderView : IGridRowView
	{
		public Widget GridWidget { get; }
		public Widget OverviewWidget { get; }
		public AwakeBehavior GridWidgetAwakeBehavior => GridWidget.Components.Get<AwakeBehavior>();
		public AwakeBehavior OverviewWidgetAwakeBehavior => OverviewWidget.Components.Get<AwakeBehavior>();

		public GridFolderView()
		{
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

		private void Render(Widget widget) { }
	}
}
