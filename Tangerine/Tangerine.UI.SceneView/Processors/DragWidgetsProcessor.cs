using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.UI.SceneView.WidgetTransforms;

namespace Tangerine.UI.SceneView
{
	public class DragWidgetsProcessor : ITaskProvider
	{
		private enum DragDirection
		{
			Any,
			Horizontal,
			Vertical
		}

		private const float Threshold = 10f;

		private static SceneView SceneView => SceneView.Instance;
		private static ProjectUserPreferences Preferences => ProjectUserPreferences.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>();
				if (Utils.CalcHullAndPivot(widgets, SceneView.Scene, out _, out var pivot) && SceneView.HitTestControlPoint(pivot)) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					if (SceneView.Input.ConsumeKeyPress(Key.Mouse0)) {
						yield return Drag();
					}
				}
				yield return null;
			}
		}

		private static IEnumerator<object> Drag()
		{
			var initialMousePos = SceneView.MousePosition;
			while ((SceneView.MousePosition - initialMousePos).Length <= Threshold && SceneView.Input.IsMousePressed()) {
				Utils.ChangeCursorIfDefault(MouseCursor.Hand);
				yield return null;
			}
			if (!SceneView.Input.IsMousePressed()) {
				yield break;
			}
			if (SceneView.Input.IsKeyPressed(Key.Alt)) {
				Document.Current.History.DoTransaction(SceneView.DuplicateSelectedNodes);
			}
			using (Document.Current.History.BeginTransaction()) {
				var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>().ToList();
				var dragDirection = DragDirection.Any;
				Utils.CalcHullAndPivot(widgets, Document.Current.Container.AsWidget, out _, out var pivot);
				while (SceneView.Input.IsMousePressed()) {
					Document.Current.History.RollbackTransaction();
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					var curMousePos = SceneView.MousePosition;
					var shiftPressed = SceneView.Input.IsKeyPressed(Key.Shift);
					if (shiftPressed) {
						switch (dragDirection) {
							case DragDirection.Horizontal:
								curMousePos.Y = initialMousePos.Y;
								break;
							case DragDirection.Vertical:
								curMousePos.X = initialMousePos.X;
								break;
							default:
								if ((curMousePos - initialMousePos).Length > 5 / SceneView.Scene.Scale.X) {
									var d = curMousePos - initialMousePos;
									dragDirection = d.X.Abs() > d.Y.Abs() ? DragDirection.Horizontal : DragDirection.Vertical;
								}
								break;
						}
					} else {
						dragDirection = DragDirection.Any;
					}

					var mouseDelta = curMousePos - initialMousePos;
					mouseDelta = mouseDelta.Snap(Vector2.Zero);

					var requiredSnap = SceneViewCommands.SnapWidgetPivotToRuler.Checked || SceneViewCommands.SnapWidgetBorderToRuler.Checked;
					if (mouseDelta != Vector2.Zero && requiredSnap) {
						var rulers = GetRulers();
						foreach (var widget in widgets) {
							var points = new List<Vector2>();
							if (SceneViewCommands.SnapWidgetPivotToRuler.Checked) {
								points.Add(widget.CalcPositionInSpaceOf(SceneView.Scene));
							}
							if (SceneViewCommands.SnapWidgetBorderToRuler.Checked) {
								points.AddRange(widget.CalcHullInSpaceOf(SceneView.Scene));
							}
							foreach (var point in points) {
								var pointMoved = point + mouseDelta;
								var pointSnapped = SnapPointToRulers(pointMoved, rulers);
								mouseDelta += pointSnapped - pointMoved;
							}
						}
					}

					Transform2d OnCalculateTransformation(Vector2d originalVectorInObbSpace, Vector2d deformedVectorInObbSpace) =>
						new Transform2d((deformedVectorInObbSpace - originalVectorInObbSpace).Snap(Vector2d.Zero), Vector2d.One, 0);
					WidgetTransformsHelper.ApplyTransformationToWidgetsGroupObb(
						SceneView.Scene,
						widgets,
						widgets.Count <= 1 ? (Vector2?)null : pivot,
						widgets.Count <= 1, initialMousePos + mouseDelta,
						initialMousePos,
						false,
						OnCalculateTransformation
					);
					yield return null;
				}
				Document.Current.History.CommitTransaction();
				SceneView.Input.ConsumeKey(Key.Mouse0);
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
			if (TrySnapPoint(point, rulers, orientationFilter, out var snappedPoint)) {
				return snappedPoint * Document.Current.RootNode.AsWidget.CalcTransitionToSpaceOf(SceneView.Instance.Scene);
			}
			return point;
		}

		private static bool TrySnapPoint(Vector2 pos, List<Ruler> rulers, RulerOrientation orientationFilter, out Vector2 snappedPoint)
		{
			var sceneZoom = SceneView.Instance.Scene.Scale.X;
			foreach (var ruler in rulers) {
				foreach (var line in ruler.Lines) {
					if (line.RulerOrientation != orientationFilter) {
						continue;
					}
					var mask = orientationFilter == RulerOrientation.Vertical ? Vector2.Right : Vector2.Down;
					var transformedPosition = pos * SceneView.Instance.Scene.CalcTransitionToSpaceOf(Document.Current.RootNode.AsWidget);
					var lineVector = LineToVector(line, ruler.AnchorToRoot);
					if ((transformedPosition * mask - lineVector).Length < Threshold / sceneZoom) {
						snappedPoint = transformedPosition * (Vector2.One - mask) + lineVector;
						return true;
					}
				}
			}
			snappedPoint = default;
			return false;
		}
	}
}
