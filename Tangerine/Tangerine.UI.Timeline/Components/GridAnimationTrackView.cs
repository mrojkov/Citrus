using Lime;
using System;
using Tangerine.Core;
using Tangerine.Core.Components;

namespace Tangerine.UI.Timeline.Components
{
	public class GridAnimationTrackView : IGridRowView
	{
		private readonly Row row;
		private readonly AnimationTrack track;

		public Widget GridWidget { get; }
		public Widget OverviewWidget { get; }

		public AwakeBehavior GridWidgetAwakeBehavior => GridWidget.Components.GetOrAdd<AwakeBehavior>();
		public AwakeBehavior OverviewWidgetAwakeBehavior => OverviewWidget.Components.GetOrAdd<AwakeBehavior>();

		private GridKeyframesRenderer keyframesRenderer = new GridKeyframesRenderer();

		public GridAnimationTrackView(Row row)
		{
			this.row = row;
			track = row.Components.Get<Core.Components.AnimationTrackRow>().Track;
			GridWidget = new Widget {
				LayoutCell = new LayoutCell { StretchY = 0 },
				MinHeight = TimelineMetrics.DefaultRowHeight,
				Presenter = new SyncDelegatePresenter<Widget>(Render)
			};
			OverviewWidget = new Widget {
				LayoutCell = new LayoutCell { StretchY = 0 },
				MinHeight = TimelineMetrics.DefaultRowHeight,
				Presenter = new SyncDelegatePresenter<Widget>(Render)
			};
		}

		private void Render(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.ContentSize, ColorTheme.Current.TimelineGrid.PropertyRowBackground);
			keyframesRenderer.ClearCells();
			keyframesRenderer.GenerateCells(track.Animators, Document.Current.AnimationId);
			keyframesRenderer.RenderCells(widget);
			var spans = row.Components.GetOrAdd<GridSpanListComponent>().Spans.GetNonOverlappedSpans();
			foreach (var clip in track.Clips) {
				var a = new Vector2(clip.Begin * TimelineMetrics.ColWidth, 0);
				var b = new Vector2(clip.End * TimelineMetrics.ColWidth, widget.Height);
				Renderer.DrawRect(a, b, ColorTheme.Current.TimelineGrid.AnimationClip);
				Renderer.DrawRectOutline(a, b, ColorTheme.Current.TimelineGrid.AnimationClipBorder);
				if (clip.IsSelected) {
					Renderer.DrawRect(a, b, ColorTheme.Current.TimelineGrid.Selection);
					Renderer.DrawRectOutline(a, b, ColorTheme.Current.TimelineGrid.SelectionBorder);
				}
				var textHeight = Mathf.Min(Theme.Metrics.TextHeight, widget.Height);
				a.X += 1;
				a.Y = (widget.Height - textHeight) / 2;
				Renderer.DrawTextLine(a, clip.AnimationId, textHeight, Color4.Black, 0);
			}
		}
	}
}
