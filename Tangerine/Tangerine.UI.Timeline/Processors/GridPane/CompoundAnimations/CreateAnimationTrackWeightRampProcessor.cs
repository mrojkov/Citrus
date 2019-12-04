using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.CompoundAnimations
{
	public class CreateAnimationTrackWeightRampProcessor : ITaskProvider
	{
		private static Timeline Timeline => Timeline.Instance;
		private static GridPane Grid => Timeline.Instance.Grid;

		public IEnumerator<object> Task()
		{
			Grid.OnPostRender += RenderCursor;
			var input = Grid.RootWidget.Input;
			while (true) {
				if (Document.Current.Animation.IsCompound) {
					if (input.IsKeyPressed(Key.Shift)) {
						Window.Current.Invalidate();
					}
					if (input.WasMousePressed() && input.IsKeyPressed(Key.Shift)) {
						input.ConsumeKey(Key.Mouse0);
						using (Document.Current.History.BeginTransaction()) {
							if (Grid.IsMouseOverRow()) {
								var c = Grid.CellUnderMouse();
								var track = Document.Current.Animation.Tracks[c.Y];
								yield return DragRampTask(track, c.X);
							}
							Document.Current.History.CommitTransaction();
						}
					}
				}
				yield return null;
			}
		}

		private void RenderCursor(Widget widget)
		{
			if (Document.Current.Animation.IsCompound && Grid.RootWidget.Input.IsKeyPressed(Key.Shift)) {
				var c = Grid.CellUnderMouse();
				if (Document.Current.Rows.Count > 0) {
					var w = Document.Current.Rows[c.Y].GridWidget();
					w.PrepareRendererState();
					Renderer.DrawLine(
						(c.X + .5f) * TimelineMetrics.ColWidth + .5f, 0,
						(c.X + .5f) * TimelineMetrics.ColWidth + .5f, w.Height,
						ColorTheme.Current.TimelineGrid.AnimationTrackWeightCurveKey, 2);
				}
			}
		}

		private static IEnumerator<object> DragRampTask(AnimationTrack track, int initFrame)
		{
			var input = Grid.RootWidget.Input;
			var prevFrame = initFrame;
			var currFrame = initFrame;
			var rampAdded = false;
			while (input.IsMousePressed()) {
				currFrame = Grid.CellUnderMouse().X;
				if (prevFrame != currFrame) {
					rampAdded = true;
					prevFrame = currFrame;
					Document.Current.History.RollbackTransaction();
					Operations.SetCurrentColumn.Perform(currFrame);
					Timeline.Ruler.MeasuredFrameDistance = currFrame - initFrame;
					SetRamp(track, initFrame, currFrame);
					Window.Current.Invalidate();
				}
				yield return null;
			}
			if (!rampAdded) {
				var hasKey =
					track.Animators.TryFind(nameof(AnimationTrack.Weight), out var weightAnimator, Document.Current.Animation.Id) &&
					weightAnimator.ReadonlyKeys.Any(k => k.Frame == initFrame);
				if (hasKey && currFrame == initFrame) {
					Core.Operations.RemoveKeyframe.Perform(weightAnimator, currFrame);
				} else {
					var k = new Keyframe<float>(currFrame, AnimationTrack.MaxWeight, KeyFunction.Steep);
					Core.Operations.SetKeyframe.Perform(track, nameof(AnimationTrack.Weight), Document.Current.Animation.Id, k);
				}
			}
		}

		private static void SetRamp(AnimationTrack track, int initFrame, int currFrame)
		{
			if (track.Animators.TryFind(nameof(AnimationTrack.Weight), out var weightAnimator, Document.Current.Animation.Id)) {
				foreach (var k in weightAnimator.ReadonlyKeys.ToList()) {
					if (k.Frame > Math.Min(initFrame, currFrame) && k.Frame < Math.Max(initFrame, currFrame)) {
						Core.Operations.RemoveKeyframe.Perform(weightAnimator, k.Frame);
					}
				}
			}
			var k1 = new Keyframe<float>(initFrame, 0f, initFrame < currFrame ? KeyFunction.Linear : KeyFunction.Steep);
			Core.Operations.SetKeyframe.Perform(track, nameof(AnimationTrack.Weight), Document.Current.Animation.Id, k1);
			var k2 = new Keyframe<float>(currFrame, AnimationTrack.MaxWeight, initFrame < currFrame ? KeyFunction.Steep : KeyFunction.Linear);
			Core.Operations.SetKeyframe.Perform(track, nameof(AnimationTrack.Weight), Document.Current.Animation.Id, k2);
		}
	}
}
