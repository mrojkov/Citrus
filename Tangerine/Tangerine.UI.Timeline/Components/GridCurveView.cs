using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	public class GridCurveView : IGridRowView
	{
		// readonly Node node;
		// readonly IAnimator animator;
		// readonly CurveEditorState curve;
		public Widget GridWidget { get; private set; }
		public Widget OverviewWidget { get; private set; }

		public GridCurveView(Node node, IAnimator animator, CurveEditorState curve)
		{
			// this.node = node;
			// this.animator = animator;
			// this.curve = curve;
			GridWidget = new Widget { LayoutCell = new LayoutCell { StretchY = 0 }, MinHeight = curve.RowHeight };
			OverviewWidget = new Widget { LayoutCell = new LayoutCell { StretchY = 0 }, MinHeight = curve.RowHeight };
			GridWidget.Presenter = new DelegatePresenter<Widget>(Render);
			OverviewWidget.Presenter = new DelegatePresenter<Widget>(Render);
		}

		void Render(Widget widget)
		{
			var maxCol = Timeline.Instance.ColumnCount;
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.ContentSize, ColorTheme.Current.TimelineGrid.PropertyRowBackground);
		}
	}
}