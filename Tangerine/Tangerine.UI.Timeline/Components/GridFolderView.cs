using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	public class GridFolderView : IGridWidget, IOverviewWidget
	{
		readonly Widget gridWidget;
		readonly Widget overviewWidget;

		public GridFolderView()
		{
			gridWidget = new Widget { LayoutCell = new LayoutCell { StretchY = 0 }, MinHeight = TimelineMetrics.DefaultRowHeight };
			overviewWidget = new Widget { LayoutCell = new LayoutCell { StretchY = 0 }, MinHeight = TimelineMetrics.DefaultRowHeight };
			gridWidget.Presenter = new DelegatePresenter<Widget>(Render);
			overviewWidget.Presenter = new DelegatePresenter<Widget>(Render);
		}

		Widget IGridWidget.Widget => gridWidget;
		Widget IOverviewWidget.Widget => overviewWidget;

		float IGridWidget.Top => gridWidget.Y;
		float IGridWidget.Bottom => gridWidget.Y + gridWidget.Height;
		float IGridWidget.Height => gridWidget.Height;

		void Render(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.ContentSize, ColorTheme.Current.Basic.WhiteBackground);
		}
	}
}