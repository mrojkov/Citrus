using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public static class Ruler
	{
		public static readonly IList<Line> Lines = new List<Line>();
		private static SceneView sv => SceneView.Instance;
		private static Widget cntr => Document.Current.RootNode.AsWidget;

		public static Widget TopRuler { get; private set; }
		public static Widget LeftRuler { get; private set; }

		private static object Tag = new object();
		private const int RulerSize = 30;
		private const int Stroke = 25;
		private static readonly IPresenter Presenter;

		public enum Orientation
		{
			Horizontal,
			Vertical
		}

		public class Line
		{
			public Vector2 Val { get; set; }
			public Orientation Orientation { get; set; }
			public RulerLine ToRulerLine()
			{
				return new RulerLine {
					Value = Orientation == Orientation.Vertical ? Val.X : Val.Y,
					IsVertical = Orientation == Orientation.Vertical
				};
			}
		}

		static Ruler()
		{
			TopRuler = new Widget {
				Anchors = Anchors.Left | Anchors.Top | Anchors.Right,
				Layout = new HBoxLayout(),
				HitTestTarget = true,
				Size = new Vector2(sv.Frame.Size.X, RulerSize),
				LayoutCell = new LayoutCell { StretchY = 0 },
				Padding = new Thickness { Left = 30 }
			};

			LeftRuler = new Widget {
				Anchors = Anchors.Left | Anchors.Top | Anchors.Bottom,
				Layout = new VBoxLayout(),
				HitTestTarget = true,
				Size = new Vector2(RulerSize, sv.Frame.Size.Y),
				LayoutCell = new LayoutCell { StretchX = 0 },
				Padding = new Thickness { Top = 30 }
			};

			Presenter = new DelegatePresenter<Widget>(widget => {
				if (Document.Current.PreviewAnimation)
					return;
				//Draw left ruler
				LeftRuler.PrepareRendererState();
				var containerHull = cntr.CalcHullInSpaceOf(widget);
				Renderer.DrawRect(Vector2.Zero, LeftRuler.Size, Color4.DarkGray);
				DrawSegments(
					new Vector2(0, containerHull.V1.Y),
					new Vector2(0, containerHull.V4.Y),
					Vector2.Right * Stroke,
					new Vector2(0, RulerSize));
				//Draw right Ruler
				TopRuler.PrepareRendererState();
				containerHull = cntr.CalcHullInSpaceOf(widget);
				Renderer.DrawRect(Vector2.Zero, TopRuler.Size, Color4.DarkGray);
				DrawSegments(
					new Vector2(containerHull.V1.X, 0),
					new Vector2(containerHull.V2.X, 0),
					Vector2.Down * Stroke,
					new Vector2(RulerSize, 0));

				//Draw ruler lines
				widget.PrepareRendererState();
				var t = cntr.CalcTransitionToSpaceOf(widget);
				foreach (var line in Lines.ToList()) {
					var val = (line.Val + cntr.Size / 2) * t;
					if (line.Orientation == Orientation.Horizontal) {
						if (val.Y >= RulerSize) {
							Renderer.DrawLine(new Vector2(0, val.Y), new Vector2(widget.Size.X, val.Y), ColorTheme.Current.SceneView.RulerEditable);
						}
					} else {
						if (val.X >= RulerSize) {
							Renderer.DrawLine(new Vector2(val.X, 0), new Vector2(val.X, widget.Size.Y), ColorTheme.Current.SceneView.RulerEditable);
						}
					}
				}

				var lineComponent = SceneView.Instance.Components.Get<LineSelectionComponent>();
				if (lineComponent != null) {
					var val = (lineComponent.Line.Val + cntr.Size / 2);
					var pos = val * t;
					var isVertical = lineComponent.Line.Orientation == Orientation.Vertical;
					var x = isVertical ? pos.X + 10 : LeftRuler.Size.X;
					var y = isVertical ? TopRuler.Size.Y : pos.Y + 10;
					Renderer.DrawRect(new Vector2(x, y), new Vector2(x + 80, y + 20), Color4.Gray);
					Renderer.DrawRectOutline(new Vector2(x, y), new Vector2(x + 80, y + 20), Color4.DarkGray);
					Renderer.DrawTextLine(new Vector2(x, y + 2), $"{(isVertical ? "X:" : "Y:")} {(int)(isVertical ? val.X : val.Y)}", 15, Color4.White, 2f);
				}
			});
			TopRuler.Tasks.Add(ProcessLines(TopRuler, Orientation.Vertical));
			LeftRuler.Tasks.Add(ProcessLines(LeftRuler));
		}

		public static bool ToggleDisplay()
		{
			var removedFromCurrentScene = false;
			var removed = false;
			foreach (var doc in Project.Current.Documents) {
				var sceneView = doc.Views.OfType<SceneView>().FirstOrDefault();
				if (sceneView.Frame.Nodes.Contains(TopRuler)) {
					sceneView.Frame.Nodes.Remove(TopRuler);
					sceneView.Frame.Nodes.Remove(LeftRuler);
					sceneView.Frame.CompoundPostPresenter.Remove(Presenter);
					removedFromCurrentScene = sv == sceneView;
					removed = true;
					break;
				}
			}

			if (!removedFromCurrentScene) {
				sv.Frame.Nodes.Insert(0, TopRuler);
				sv.Frame.Nodes.Insert(0, LeftRuler);
				sv.Frame.CompoundPostPresenter.Push(Presenter);
			}
			Window.Current.Invalidate();
			return !removedFromCurrentScene || !removed;
		}

		private static IEnumerator<object> ProcessLines(Widget widget, Orientation lineOrientation = Orientation.Horizontal)
		{
			while (true) {
				var rect = new Rectangle(Vector2.Zero, widget.Size);
				var hull = rect.ToQuadrangle();
				if ((rect.Contains(widget.Input.LocalMousePosition))) {
					if (widget.Input.WasMousePressed()) {
						widget.Input.CaptureMouse();
						var line = GetLineUnderMouse(widget, lineOrientation);
						if (line != null) {
							SceneView.Instance.Components.Add(new LineSelectionComponent { Line = line });
							while (widget.Input.IsMousePressed()) {
								line.Val = cntr.Input.LocalMousePosition - cntr.Size / 2;
								Window.Current.Invalidate();
								yield return null;
							}
						} else {
							line = new Line {
								Val = cntr.Input.LocalMousePosition - cntr.Size / 2,
								Orientation = lineOrientation
							};
							SceneView.Instance.Components.Add(new LineSelectionComponent { Line = line });
							Lines.Add(line);
							yield return null;
							while (widget.Input.IsMousePressed()) {
								line.Val = cntr.Input.LocalMousePosition - cntr.Size / 2;
								Window.Current.Invalidate();
								yield return null;
							}
						}
						widget.Input.ReleaseMouse();
						SceneView.Instance.Components.Remove<LineSelectionComponent>();
					} else if (widget.Input.WasMouseReleased(1)) {
						Lines.Remove(GetLineUnderMouse(widget, lineOrientation));
					}
					Window.Current.Invalidate();
				}
				yield return null;
			}
		}

		internal static RulerData GetRulerData()
		{
			var overlay = new RulerData();
			foreach (var l in Lines) {
				overlay.Lines.Add(l.ToRulerLine());
			}
			return overlay;
		}

		internal static void Clear()
		{
			Lines.Clear();
		}

		private static bool IsLineUnderMouse(Widget widget, Line line)
		{
			return GetLineUnderMouse(widget, line.Orientation) == line;
		}

		private static Line GetLineUnderMouse(Widget widget, Orientation orientation)
		{
			var mPos = cntr.Input.LocalMousePosition;
			var rect = new Rectangle(Vector2.Zero, widget.Size);
			var dir = orientation == Orientation.Vertical ? Vector2.Right : Vector2.Down;
			if (!rect.Contains(widget.Input.LocalMousePosition))
				return null;
			return Lines.FirstOrDefault(line => line.Orientation == orientation &&
				((line.Val + cntr.Size / 2) * dir - mPos * dir).Length <= 6f);
		}

		private static void DrawSegments(Vector2 start, Vector2 end, Vector2 delta, Vector2 dir, Vector2 blocker)
		{
			var p = start;
			do {
				var d = p - blocker;
				if (d.X >= 0 && d.Y >= 0) {
					Renderer.DrawLine(p, p + dir, Color4.Gray);
				}
				p += delta;
			} while ((end - start).Length >= (p - start).Length);
		}

		private static void DrawSegments(Vector2 start, Vector2 end, Vector2 stroke, Vector2 blocker)
		{
			var delta = end - start;
			var bigDelta = delta / 2;
			var mediumDelta = bigDelta / 4;
			var smalDelta = mediumDelta / 4;
			DrawSegments(start, end, bigDelta, stroke, blocker);
			DrawSegments(start, end, mediumDelta, stroke / 2, blocker);
			DrawSegments(start, end, smalDelta, stroke / 4, blocker);
		}
	}

	internal class LineSelectionComponent : Component
	{
		public Ruler.Line Line { get; set; }
	}
}
