using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class DragWidgetsProcessor : ITaskProvider
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				Quadrangle hull;
				Vector2 pivot;
				var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>();
				if (Utils.CalcHullAndPivot(widgets, sv.Scene, out hull, out pivot) && sv.HitTestControlPoint(pivot)) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
						if (sv.Input.IsKeyPressed(Key.Alt)) {
							sv.DuplicateSelectedNodes();
						}
						yield return Drag();
					}
				}
				yield return null;
			}
		}

		enum DragDirection
		{
			Any, Horizontal, Vertical
		}

		IEnumerator<object> Drag()
		{
			Document.Current.History.BeginTransaction();
			try {
				var initialMousePos = sv.MousePosition;
				var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>().ToList();
				var dragDirection = DragDirection.Any;
				Quadrangle hull;
				Vector2 pivot;
				Utils.CalcHullAndPivot(widgets, Document.Current.Container.AsWidget, out hull, out pivot);
				while (sv.Input.IsMousePressed()) {
					Document.Current.History.RevertActiveTransaction();
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					var curMousePos = sv.MousePosition;
					var shiftPressed = sv.Input.IsKeyPressed(Key.Shift);
					if (shiftPressed) {
						if (dragDirection != DragDirection.Any) {
							if (dragDirection == DragDirection.Horizontal) {
								curMousePos.Y = initialMousePos.Y;
							} else if (dragDirection == DragDirection.Vertical) {
								curMousePos.X = initialMousePos.X;
							}
						} else if ((curMousePos - initialMousePos).Length > 5 / sv.Scene.Scale.X) {
							var d = curMousePos - initialMousePos;
							dragDirection = d.X.Abs() > d.Y.Abs() ? DragDirection.Horizontal : DragDirection.Vertical;
						}
					} else {
						dragDirection = DragDirection.Any;
					}

					Vector2 mouseDelta = curMousePos - initialMousePos;
					mouseDelta = mouseDelta.Snap(Vector2.Zero);

					if (
						mouseDelta != Vector2.Zero &&
						(SceneViewCommands.SnapWidgetPivotToRuler.Checked || SceneViewCommands.SnapWidgetBorderToRuler.Checked)
					) {
						List<RulerLine> lines = GetRulerLines();

						foreach (Widget widget in widgets) {
							List<Vector2> points = new List<Vector2>();

							if (SceneViewCommands.SnapWidgetPivotToRuler.Checked) {
								points.Add(widget.CalcPositionInSpaceOf(sv.Scene));
							}
							if (SceneViewCommands.SnapWidgetBorderToRuler.Checked) {
								points.AddRange(widget.CalcHullInSpaceOf(sv.Scene));
							}

							foreach (Vector2 point in points) {
								Vector2 pointMoved = point + mouseDelta;
								Vector2 pointsSnapped = SnapPointToLines(pointMoved, lines);
								mouseDelta += pointsSnapped - pointMoved;
							}
						}
					}

					Utils.ApplyTransformationToWidgetsGroupObb(
						sv.Scene,
						widgets, pivot, widgets.Count <= 1, initialMousePos + mouseDelta, initialMousePos,
						(originalVectorInObbSpace, deformedVectorInObbSpace) => {
							return Matrix32.Translation((deformedVectorInObbSpace - originalVectorInObbSpace).Snap(Vector2.Zero));
						}
					);
					
					yield return null;
				}
			} finally {
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.EndTransaction();
			}
		}

		private static List<RulerLine> GetRulerLines()
		{
			var lines = new List<RulerLine>();
			foreach (var line in Ruler.Lines) {
				lines.Add(line.ToRulerLine());
			}
			foreach (var ruler in Project.Current.Rulers) {
				if (ruler.GetComponents().Get<CommandComponent>().Command.Checked) {
					lines.AddRange(ruler.Lines);
				}
			}
			foreach (var ruler in Project.Current.DefaultRulers) {
				if (ruler.GetComponents().Get<CommandComponent>().Command.Checked) {
					lines.AddRange(ruler.Lines);
				}
			}
			return lines;
		}

		private static RulerLine GetRulerLine(Vector2 pos, List<RulerLine> lines, bool isVertical)
		{
			var t = Document.Current.Container.AsWidget.CalcTransitionToSpaceOf(Document.Current.RootNode.AsWidget);
			return lines.FirstOrDefault(l => {
				if (l.IsVertical != isVertical)
					return false;
				var mask = l.IsVertical ? Vector2.Right : Vector2.Down;
				return (pos * t * mask - (l.ToVector2() + Document.Current.RootNode.AsWidget.Size * mask / 2)).Length < 15;
			});
		}

		private static Vector2 SnapPointToLines(Vector2 point, List<RulerLine> lines)
		{
			point = SnapPointToLine(point, GetRulerLine(point, lines, true));
			return SnapPointToLine(point, GetRulerLine(point, lines, false));
		}

		private static Vector2 SnapPointToLine(Vector2 point, RulerLine line)
		{
			if (line != null) {
				var t = Document.Current.Container.AsWidget.CalcTransitionToSpaceOf(Document.Current.RootNode.AsWidget);
				var mask = line.IsVertical ? Vector2.Right : Vector2.Down;
				return (point * t * (Vector2.One - mask) +
					(line.ToVector2() + Document.Current.RootNode.AsWidget.Size * mask / 2)) * t.CalcInversed();
			}
			return point;
		}
	}
}
