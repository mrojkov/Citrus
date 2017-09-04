using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	public class GridFolderView : IGridRowView
	{
		public Widget GridWidget { get; private set; }
		public Widget OverviewWidget { get; private set; }

		public GridFolderView()
		{
			GridWidget = new Widget { LayoutCell = new LayoutCell { StretchY = 0 }, MinHeight = TimelineMetrics.DefaultRowHeight };
			OverviewWidget = new Widget { LayoutCell = new LayoutCell { StretchY = 0 }, MinHeight = TimelineMetrics.DefaultRowHeight };
			GridWidget.Presenter = new DelegatePresenter<Widget>(Render);
			OverviewWidget.Presenter = new DelegatePresenter<Widget>(Render);
		}

		void Render(Widget widget) { }
	}
}