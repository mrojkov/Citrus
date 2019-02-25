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
			foreach (var c in track.Clips) {
				var a = new Vector2(c.Begin * TimelineMetrics.ColWidth, 0);
				var b = new Vector2(c.End * TimelineMetrics.ColWidth, widget.Height);
				Renderer.DrawRect(a, b, Color4.Blue.Transparentify(0.75f));
				Renderer.DrawRectOutline(a, b, Color4.Blue);
				var textHeight = Mathf.Min(Theme.Metrics.TextHeight, widget.Height);
				a.X += 1;
				a.Y = (widget.Height - textHeight) / 2;
				Renderer.DrawTextLine(a, c.AnimationId, textHeight, Color4.Black, 0);
			}
			//var colorIndex = PropertyAttributes<TangerineKeyframeColorAttribute>.Get(animator.Animable.GetType(), animator.TargetPropertyPath)?.ColorIndex ?? 0;
			//var color = KeyframePalette.Colors[colorIndex];
			//for (int i = 0; i < animator.ReadonlyKeys.Count; i++) {
			//	var key = animator.ReadonlyKeys[i];
			//	Renderer.Transform1 =
			//		Matrix32.RotationRough(Mathf.Pi / 4) *
			//		Matrix32.Translation((key.Frame + 0.5f) * TimelineMetrics.ColWidth + 0.5f, widget.Height / 2 + 0.5f) *
			//		widget.LocalToWorldTransform;
			//	var v = TimelineMetrics.ColWidth / 3 * Vector2.One;
			//	Renderer.DrawRect(-v, v, color);
			//}
		}
	}
}
