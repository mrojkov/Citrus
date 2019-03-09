using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.UI.Timeline.Components;
using Tangerine.UI.Timeline.Operations.CompoundAnimations;

namespace Tangerine.UI.Timeline.CompoundAnimations
{
	public class SelectAndDragAnimationClipsProcessor : ITaskProvider
	{
		private static Timeline Timeline => Timeline.Instance;
		private static GridPane Grid => Timeline.Instance.Grid;

		public IEnumerator<object> Task()
		{
			var input = Grid.RootWidget.Input;
			while (true) {
				if (input.WasMousePressed() && Document.Current.Animation.IsCompound) {
					Document.Current.GetCompoundAnimationIspectMode = CompoundAnimationInspectionMode.Clips;
					using (Document.Current.History.BeginTransaction()) {
						var initialCell = Grid.CellUnderMouse(clampRow: false);
						if (initialCell.Y >= 0 && initialCell.Y < Document.Current.Rows.Count) {
							if (TryFindClip(initialCell, out var clip) && clip.IsSelected) {
								yield return DragSelectionTask(initialCell);
							} else {
								yield return SelectTask(initialCell);
							}
						}
						Document.Current.History.CommitTransaction();
					}
				}
				yield return null;
			}
		}

		private static IEnumerator<object> DragSelectionTask(IntVector2 initialCell)
		{
			var input = Grid.RootWidget.Input;
			var offset = IntVector2.Zero;
			Grid.OnPostRender += RenderSelectedClips;
			while (input.IsMousePressed()) {
				offset = Grid.CellUnderMouse() - initialCell;
				Timeline.Ruler.MeasuredFrameDistance = Timeline.CurrentColumn - initialCell.X;
				if (input.IsKeyPressed(Key.Shift) == CoreUserPreferences.Instance.InverseShiftKeyframeDrag) {
					offset.Y = 0;
				}
				Window.Current.Invalidate();
				yield return null;
			}
			if (offset != IntVector2.Zero) {
				DragAnimationClips.Perform(offset, !input.IsKeyPressed(Key.Alt));
				Timeline.Ruler.MeasuredFrameDistance = 0;
			} else {
				if (input.IsKeyPressed(Key.Control)) {
					if (TryFindClip(Grid.CellUnderMouse(), out var clip)) {
						Core.Operations.SetProperty.Perform(clip, nameof(AnimationClip.IsSelected), !clip.IsSelected);
					}
				} else {
					DeselectAllClips();
					if (TryFindClip(Grid.CellUnderMouse(), out var clip)) {
						Core.Operations.SetProperty.Perform(clip, nameof(AnimationClip.IsSelected), true);
					}
				}
			}
			Grid.OnPostRender -= RenderSelectedClips;
			Window.Current.Invalidate();

			void RenderSelectedClips(Widget widget)
			{
				foreach (var row in Document.Current.Rows) {
					var track = row.Components.Get<AnimationTrackRow>()?.Track;
					if (track?.EditorState().Locked != false) {
						continue;
					}
					var gw = row.Components.Get<RowView>().GridRow.GridWidget;
					gw.PrepareRendererState();
					foreach (var clip in track.Clips.Where(i => i.IsSelected)) {
						var s = new Vector2(TimelineMetrics.ColWidth, gw.Height);
						var a = new Vector2(clip.Begin + offset.X, offset.Y) * s;
						var b = a + new Vector2(clip.Length, 1) * s;
						Renderer.DrawRect(a, b, ColorTheme.Current.TimelineGrid.AnimationClip);
						Renderer.DrawRectOutline(a, b, ColorTheme.Current.TimelineGrid.AnimationClipBorder);
					}
				}
			}
		}

		private static void DeselectAllClips()
		{
			foreach (var row in Document.Current.Rows) {
				var track = row.Components.Get<AnimationTrackRow>().Track;
				foreach (var c in track.Clips) {
					Core.Operations.SetProperty.Perform(c, nameof(AnimationClip.IsSelected), false);
				}
			}
		}

		private static bool TryFindClip(IntVector2 cell, out AnimationClip clip)
		{
			var track = Document.Current.Rows[cell.Y].Components.Get<AnimationTrackRow>().Track;
			if (track != null) {
				foreach (var c in track.Clips) {
					if (c.Begin <= cell.X && cell.X < c.End) {
						clip = c;
						return true;
					}
				}
			}
			clip = null;
			return false;
		}

		private IEnumerator<object> SelectTask(IntVector2 initialCell)
		{
			var input = Grid.RootWidget.Input;
			var rect = new IntRectangle();
			var showSelection = false;
			var showMeasuredFrameDistance = false;
			Grid.OnPostRender += RenderSelectionRect;
			while (input.IsMousePressed()) {
				rect.A = initialCell;
				rect.B = Grid.CellUnderMouse();
				if (rect.Width >= 0) {
					rect.B.X++;
				} else {
					rect.A.X++;
				}
				if (rect.Height >= 0) {
					rect.B.Y++;
				} else {
					rect.A.Y++;
				}
				rect = rect.Normalized;
				showSelection |= rect.Width > 1 || rect.Height > 1;
				showMeasuredFrameDistance |= rect.Width != 1;
				if (showMeasuredFrameDistance) {
					Timeline.Instance.Ruler.MeasuredFrameDistance = rect.Width;
				}
				Window.Current.Invalidate();
				yield return null;
			}
			Timeline.Instance.Ruler.MeasuredFrameDistance = 0;
			Grid.OnPostRender -= RenderSelectionRect;
			if (!input.IsKeyPressed(Key.Control)) {
				DeselectAllClips();
			}
			for (var r = rect.Top; r < rect.Bottom; r++) {
				var track = Document.Current.Rows[r].Components.Get<AnimationTrackRow>().Track;
				foreach (var clip in track.Clips) {
					if (Math.Max(clip.Begin, rect.Left) >= Math.Min(clip.End, rect.Right)) {
						continue;
					}
					Core.Operations.SetProperty.Perform(clip, nameof(AnimationClip.IsSelected), true);
				}
			}

			void RenderSelectionRect(Widget widget)
			{
				if (showSelection) {
					widget.PrepareRendererState();
					var a = Grid.CellToGridCoordinates(rect.A);
					var b = Grid.CellToGridCoordinates(rect.B);
					Renderer.DrawRect(a, b, ColorTheme.Current.TimelineGrid.Selection);
					Renderer.DrawRectOutline(a, b, ColorTheme.Current.TimelineGrid.SelectionBorder);
				}
			}
		}
	}
}
