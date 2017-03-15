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
			var maxCol = Timeline.Instance.ColumnCount;
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.ContentSize, ColorTheme.Current.TimelineGrid.PropertyRowBackground);
			var colorIndex = PropertyAttributes<TangerineKeyframeColorAttribute>.Get(node.GetType(), animator.TargetProperty)?.ColorIndex ?? 0;
			var color = KeyframePalette.Colors[colorIndex];
			var baseTransform = Renderer.Transform1;
			for (int i = 0; i < animator.ReadonlyKeys.Count; i++) {
				var key = animator.ReadonlyKeys[i];
				Renderer.Transform1 =
					Matrix32.Rotation(Mathf.Pi / 4) * 
					Matrix32.Translation((key.Frame + 0.5f) * TimelineMetrics.ColWidth + 0.5f, widget.Height / 2 + 0.5f) *
					baseTransform;
				var v = TimelineMetrics.ColWidth / 3 * Vector2.One;
				Renderer.DrawRect(-v, v, color);
			}
		}
	}
}