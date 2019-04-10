using System;
using Lime;
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

		public GridAnimationTrackView(Row row)
		{
			this.row = row;
			track = row.Components.Get<AnimationTrackRow>().Track;
			GridWidget = new Widget {
				LayoutCell = new LayoutCell { StretchY = 0 },
				MinHeight = TimelineMetrics.DefaultRowHeight,
				Presenter = new SyncDelegatePresenter<Widget>(Render)
			};
			GridWidgetAwakeBehavior.Action += _ => {
				GridWidget.AddChangeWatcher(() => CalcLabelsHashCode(track), __ => RefreshLabels(GridWidget, track));
			};
			OverviewWidget = new Widget {
				LayoutCell = new LayoutCell { StretchY = 0 },
				MinHeight = TimelineMetrics.DefaultRowHeight,
				Presenter = new SyncDelegatePresenter<Widget>(Render)
			};
		}

		private static int CalcLabelsHashCode(AnimationTrack track)
		{
			unchecked {
				var r = -511344;
				foreach (var clip in track.Clips) {
					r = r * -1521134295 + clip.AnimationIdComparisonCode;
					r = r * -1521134295 + clip.BeginFrame;
					r = r * -1521134295 + clip.EndFrame;
					r = r * -1521134295 + clip.InFrame;
				}
				return r;
			}
		}

		private void RefreshLabels(Widget widget, AnimationTrack track)
		{
			widget.Nodes.Clear();
			foreach (var clip in track.Clips) {
				var clipLabel = clip.AnimationId;
				var beginMarker = clip.Animation?.Markers.GetByFrame(clip.InFrame);
				var endMarker = clip.Animation?.Markers.GetByFrame(clip.InFrame + clip.DurationInFrames);
				if (beginMarker != null || endMarker != null) {
					clipLabel +=
						" (" + (beginMarker?.Id ?? clip.InFrame.ToString()) + ".." +
						(endMarker?.Id ?? (clip.InFrame + clip.DurationInFrames).ToString()) + ")";
				}
				widget.AddNode(new SimpleText {
					Position = new Vector2((clip.BeginFrame + 0.5f) * TimelineMetrics.ColWidth, 0),
					Size = new Vector2((clip.DurationInFrames + 0.5f) * TimelineMetrics.ColWidth, widget.Height),
					Text = clipLabel,
					Padding = new Thickness(4),
					VAlignment = VAlignment.Center,
					OverflowMode = TextOverflowMode.Ellipsis,
					Color = ColorTheme.Current.Basic.BlackText
				});
			}
		}

		private void Render(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.ContentSize, ColorTheme.Current.TimelineGrid.PropertyRowBackground);
			RenderClips(widget);
			RenderBlendWeightCurve(widget);
		}

		private void RenderClips(Widget widget)
		{
			var spans = row.Components.GetOrAdd<GridSpanListComponent>().Spans.GetNonOverlappedSpans();
			foreach (var clip in track.Clips) {
				var a = new Vector2((clip.BeginFrame + .5f) * TimelineMetrics.ColWidth + .5f, 0);
				var b = new Vector2((clip.EndFrame + .5f) * TimelineMetrics.ColWidth + .5f, widget.Height);
				Renderer.DrawRect(a, b, ColorTheme.Current.TimelineGrid.AnimationClip);
				Renderer.DrawRectOutline(a, b, ColorTheme.Current.TimelineGrid.AnimationClipBorder);
				if (clip.IsSelected) {
					Renderer.DrawRect(a, b, ColorTheme.Current.TimelineGrid.Selection);
					Renderer.DrawRectOutline(a, b, ColorTheme.Current.TimelineGrid.SelectionBorder);
				}
			}
		}

		private readonly Vertex[] vertices = new Vertex[4];

		private void RenderBlendWeightCurve(Widget widget)
		{
			var color = ColorTheme.Current.TimelineGrid.AnimationTrackWeightCurve;
			var a = CalcWeightCurvePoint(-1, track.Weight, widget.Height);
			var b = new Vector2(widget.Width, a.Y);
			if (track.Animators.TryFind<float>(nameof(AnimationTrack.Weight), out var animator, Document.Current.Animation.Id)) {
				var first = true;
				var func = KeyFunction.Steep;
				foreach (var k in animator.ReadonlyKeys) {
					b = CalcWeightCurvePoint(k.Frame, k.Value, widget.Height);
					if (first) {
						a = new Vector2(0, b.Y);
						first = false;
					}
					var b2 = func == KeyFunction.Steep ? new Vector2(b.X, a.Y) : b;
					DrawQuad(a, b2, widget.Height, color);
					func = k.Function;
					a = b;
				}
				foreach (var k in animator.ReadonlyKeys) {
					var x = (k.Frame + .5f) * TimelineMetrics.ColWidth + .5f;
					var h = widget.Height;
					DrawTriangle(
						new Vector2(x - 3, 0),
						new Vector2(x + 3, 0),
						new Vector2(x, 3),
						ColorTheme.Current.TimelineGrid.AnimationTrackWeightCurveKey);
					DrawTriangle(
						new Vector2(x - 3, h),
						new Vector2(x + 3, h),
						new Vector2(x, h - 3),
						ColorTheme.Current.TimelineGrid.AnimationTrackWeightCurveKey);
				}
			}
			DrawQuad(a, new Vector2(widget.Width, b.Y), widget.Height, color);
		}

		private void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, Color4 color)
		{
			vertices[0].Pos = a;
			vertices[0].Color = color;
			vertices[1].Pos = b;
			vertices[1].Color = color;
			vertices[2].Pos = c;
			vertices[2].Color = color;
			Renderer.DrawTriangleFan(vertices, 3);
		}

		private Vector2 CalcWeightCurvePoint(int frame, float weight, float widgetHeight)
		{
			return new Vector2(
				(frame + .5f) * TimelineMetrics.ColWidth,
				Mathf.Lerp(weight.Clamp(0, 100) * .01f, widgetHeight, 0));
		}

		private void DrawQuad(Vector2 prevKeyPos, Vector2 keyPos, float widgetHeight, Color4 color)
		{
			if (keyPos.Y >= widgetHeight && prevKeyPos.Y >= widgetHeight) {
				return;
			}
			vertices[0].Color = color;
			vertices[0].Pos = prevKeyPos;
			vertices[1].Color = color;
			vertices[1].Pos = keyPos;
			vertices[2].Color = color;
			vertices[2].Pos = new Vector2(keyPos.X, widgetHeight);
			vertices[3].Color = color;
			vertices[3].Pos = new Vector2(prevKeyPos.X, widgetHeight);
			Renderer.DrawTriangleFan(vertices, 4);
		}
	}
}
