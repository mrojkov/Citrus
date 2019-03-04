using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	public class GridAnimationTrackView : IGridRowView
	{
		private readonly AnimationTrack track;

		public Widget GridWidget { get; }
		public Widget OverviewWidget { get; }
		public AwakeBehavior GridWidgetAwakeBehavior => GridWidget.Components.Get<AwakeBehavior>();
		public AwakeBehavior OverviewWidgetAwakeBehavior => OverviewWidget.Components.Get<AwakeBehavior>();
		private GridKeyframesRenderer keyframesRenderer = new GridKeyframesRenderer();

		public GridAnimationTrackView(AnimationTrack track)
		{
			this.track = track;
			GridWidget = new Widget {
				LayoutCell = new LayoutCell { StretchY = 0 },
				MinHeight = TimelineMetrics.DefaultRowHeight,
				Presenter = new SyncDelegatePresenter<Widget>(Render)
			};
			GridWidget.Components.Add(new AwakeBehavior());
			OverviewWidget = new Widget {
				LayoutCell = new LayoutCell { StretchY = 0 },
				MinHeight = TimelineMetrics.DefaultRowHeight,
				Presenter = new SyncDelegatePresenter<Widget>(Render)
			};
			OverviewWidget.Components.Add(new AwakeBehavior());
		}

		private void Render(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.ContentSize, ColorTheme.Current.TimelineGrid.PropertyRowBackground);
			keyframesRenderer.ClearCells();
			keyframesRenderer.GenerateCells(track.Animators, Document.Current.AnimationId);
			keyframesRenderer.RenderCells(widget);
			foreach (var clip in track.Clips) {
				var a = new Vector2(clip.Begin * TimelineMetrics.ColWidth, 0);
				var b = new Vector2(clip.End * TimelineMetrics.ColWidth, widget.Height);
				Renderer.DrawRect(a, b, Color4.Blue.Transparentify(0.75f));
				Renderer.DrawRectOutline(a, b, Color4.Blue);
				var textHeight = Mathf.Min(Theme.Metrics.TextHeight, widget.Height);
				a.X += 1;
				a.Y = (widget.Height - textHeight) / 2;
				Renderer.DrawTextLine(a, clip.AnimationId, textHeight, Color4.Black, 0);
			}
		}
	}
}
