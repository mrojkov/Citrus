using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	public class GridCurveView : IGridWidget, IOverviewWidget
	{
		// readonly Node node;
		// readonly IAnimator animator;
		readonly CurveEditorState curve;
		readonly Widget gridWidget;
		readonly Widget overviewWidget;

		public GridCurveView(Node node, IAnimator animator, CurveEditorState curve)
		{
			// this.node = node;
			// this.animator = animator;
			this.curve = curve;
			gridWidget = new Widget { LayoutCell = new LayoutCell { StretchY = 0 }, MinHeight = curve.RowHeight };
			overviewWidget = new Widget { LayoutCell = new LayoutCell { StretchY = 0 }, MinHeight = curve.RowHeight };
			gridWidget.Presenter = new WidgetPresenter(Render);
			overviewWidget.Presenter = new WidgetPresenter(Render);
		}

		Widget IGridWidget.Widget => gridWidget;
		Widget IOverviewWidget.Widget => overviewWidget;
	
		void Render(Widget widget)
		{
			var maxCol = Timeline.Instance.ColumnCount;
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.ContentSize, Colors.GridPropertyRowBackground);
		}
	}
}