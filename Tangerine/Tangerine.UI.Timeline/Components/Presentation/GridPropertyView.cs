using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	public class GridPropertyView : IGridWidget, IOverviewWidget
	{
		readonly Node node;
		readonly IAnimator animator;
		readonly Widget gridWidget;
		readonly Widget overviewWidget;

		public GridPropertyView(Node node, IAnimator animator)
		{
			this.node = node;
			this.animator = animator;
			gridWidget = new Widget { LayoutCell = new LayoutCell { StretchY = 0 }, MinHeight = Metrics.TimelineDefaultRowHeight };
			overviewWidget = new Widget { LayoutCell = new LayoutCell { StretchY = 0 }, MinHeight = Metrics.TimelineDefaultRowHeight };
			gridWidget.Presenter = new DelegatePresenter<Widget>(Render);
			overviewWidget.Presenter = new DelegatePresenter<Widget>(Render);
		}

		Widget IGridWidget.Widget => gridWidget;
		Widget IOverviewWidget.Widget => overviewWidget;

		void Render(Widget widget)
		{
			var maxCol = Timeline.Instance.ColumnCount;
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.ContentSize, Colors.GridPropertyRowBackground);
			var colorIndex = PropertyRegistry.GetTangerineAttribute(node.GetType(), animator.TargetProperty)?.ColorIndex;
			var color = KeyframePalette.Colors[colorIndex.Value];
			var baseTransform = Renderer.Transform1;
			for (int i = 0; i < animator.ReadonlyKeys.Count; i++) {
				var key = animator.ReadonlyKeys[i];
				Renderer.Transform1 =
					Matrix32.Rotation(Mathf.Pi / 4) * 
					Matrix32.Translation((key.Frame + 0.5f) * Metrics.TimelineColWidth + 0.5f, widget.Height / 2 + 0.5f) *
					baseTransform;
				var v = Metrics.TimelineColWidth / 3 * Vector2.One;
				Renderer.DrawRect(-v, v, color);
				Renderer.DrawRectOutline(-v, v, Colors.GridLines);
			}
		}
	}
}