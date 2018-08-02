using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.UI.SceneView.WidgetTransforms;

namespace Tangerine.UI.SceneView
{
	public class DragWidgetsProcessor : ITaskProvider
	{
		SceneView sv => SceneView.Instance;
		static ProjectUserPreferences Preferences => ProjectUserPreferences.Instance;
		private static float Threshold = 10f;

		public IEnumerator<object> Task()
		{
			while (true) {
				Quadrangle hull;
				Vector2 pivot;
				var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>();
				if (Utils.CalcHullAndPivot(widgets, sv.Scene, out hull, out pivot) && sv.HitTestControlPoint(pivot)) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
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
			var initialMousePos = sv.MousePosition;
			while ((sv.MousePosition - initialMousePos).Length <= 10 && sv.Input.IsMousePressed()) {
				Utils.ChangeCursorIfDefault(MouseCursor.Hand);
				yield return null;
			}
			if (!sv.Input.IsMousePressed()) {
				yield break;
			}
			if (sv.Input.IsKeyPressed(Key.Alt)) {
				Document.Current.History.DoTransaction(sv.DuplicateSelectedNodes);
			}
			using (Document.Current.History.BeginTransaction()) {
				var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>().ToList();
				var dragDirection = DragDirection.Any;
				Quadrangle hull;
				Vector2 pivot;
				Utils.CalcHullAndPivot(widgets, Document.Current.Container.AsWidget, out hull, out pivot);
				while (sv.Input.IsMousePressed()) {
					Document.Current.History.RollbackTransaction();
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
						var rulers = GetRulers();

						foreach (Widget widget in widgets) {
							var points = new List<Vector2>();

							if (SceneViewCommands.SnapWidgetPivotToRuler.Checked) {
								points.Add(widget.CalcPositionInSpaceOf(sv.Scene));
							}
							if (SceneViewCommands.SnapWidgetBorderToRuler.Checked) {
								points.AddRange(widget.CalcHullInSpaceOf(sv.Scene));
							}

							foreach (Vector2 point in points) {
								Vector2 pointMoved = point + mouseDelta;
								Vector2 pointSnapped = SnapPointToRulers(pointMoved, rulers);
								mouseDelta += pointSnapped - pointMoved;
							}
						}
					}

					WidgetTransformsHelper.ApplyTransformationToWidgetsGroupObb(
						sv.Scene,
						widgets, widgets.Count <= 1 ? (Vector2?)null : pivot, widgets.Count <= 1, initialMousePos + mouseDelta,
						initialMousePos,
						false,
						(originalVectorInObbSpace, deformedVectorInObbSpace) => new Transform2d(
							(deformedVectorInObbSpace - originalVectorInObbSpace).Snap(Vector2d.Zero),
							Vector2d.One, 0
						));

					yield return null;
				}
				Document.Current.History.CommitTransaction();
				sv.Input.ConsumeKey(Key.Mouse0);
			}
		}

		private static List<Ruler> GetRulers()
		{
			var sets = new List<Ruler>();
			if (Preferences.RulerVisible) {
				sets.Add(Preferences.ActiveRuler);
			}
			sets.AddRange(Preferences.Rulers.Where(r => r.Components.Get<CommandComponent>().Command.Checked));
			sets.AddRange(Preferences.DefaultRulers.Where(r => r.Components.Get<CommandComponent>().Command.Checked));
			return sets;
		}

		private static Vector2 LineToVector(RulerLine line, bool anchorToRoot)
		{
			var mask = line.RulerOrientation == RulerOrientation.Vertical ? Vector2.Right : Vector2.Down;
			if (anchorToRoot) {
				return line.GetClosestPointToOrigin() + Document.Current.RootNode.AsWidget.Size * mask / 2;
			}

			return line.GetClosestPointToOrigin();
		}

		private static Vector2 SnapPointToRulers(Vector2 point, List<Ruler> rulers)
		{
			var p1 = SnapPointToRulers(point, rulers, RulerOrientation.Vertical);
			var p2 = SnapPointToRulers(point, rulers, RulerOrientation.Horizontal);
			return new Vector2(p1.X, p2.Y);
		}

		private static Vector2 SnapPointToRulers(Vector2 point, List<Ruler> rulers, RulerOrientation orientationFilter)
		{
			Vector2 snappedPoint;
			if (TrySnapPoint(point, rulers, orientationFilter, out snappedPoint)) {
				return snappedPoint * Document.Current.RootNode.AsWidget.CalcTransitionToSpaceOf(SceneView.Instance.Scene);
			}
			return point;
		}

		private static bool TrySnapPoint(Vector2 pos, List<Ruler> rulers, RulerOrientation orientationFilter, out Vector2 snappedPoint)
		{
			var sceneZoom = SceneView.Instance.Scene.Scale.X;
			foreach (var ruler in rulers) {
				foreach (var line in ruler.Lines) {
					if (line.RulerOrientation != orientationFilter)
						continue;
					var mask = orientationFilter == RulerOrientation.Vertical ? Vector2.Right : Vector2.Down;
					var transformedPosition = pos * SceneView.Instance.Scene.CalcTransitionToSpaceOf(Document.Current.RootNode.AsWidget);
					var lineVector = LineToVector(line, ruler.AnchorToRoot);
					if ((transformedPosition * mask - lineVector).Length < Threshold / sceneZoom) {
						snappedPoint = transformedPosition * (Vector2.One - mask) + lineVector;
						return true;
					}
				}
			}
			snappedPoint = default(Vector2);
			return false;
		}
	}
}
