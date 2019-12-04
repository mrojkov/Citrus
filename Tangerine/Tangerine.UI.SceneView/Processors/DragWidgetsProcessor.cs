using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;
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

		private static readonly (Key Key, IntVector2 Direction)[] dragKeys = new[] {
			(Key.W, new IntVector2(0, -1)),
			(Key.A, new IntVector2(-1, 0)),
			(Key.S, new IntVector2(0, 1)),
			(Key.D, new IntVector2(1, 0)),
		};

		public IEnumerator<object> Task()
		{
			while (true) {
				if (!SceneView.Instance.InputArea.IsMouseOverThisOrDescendant()) {
					yield return null;
					continue;
				}
				var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>();
				if (Utils.CalcHullAndPivot(widgets, out _, out var pivot) && SceneView.HitTestControlPoint(pivot)) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					if (SceneView.Input.ConsumeKeyPress(Key.Mouse0)) {
						yield return DragByMouse();
					}
				}
				if (dragKeys.Any(i => SceneView.Input.WasKeyPressed(i.Key))) {
					yield return DragByKeys();
				}
				yield return null;
			}
		}

		private static IEnumerator<object> DragByKeys()
		{
			var offset = Vector2.Zero;
			using (Document.Current.History.BeginTransaction()) {
				while (dragKeys.Any(i => SceneView.Input.IsKeyPressed(i.Key))) {
					Document.Current.History.RollbackTransaction();
					var isAccelerated = SceneView.Input.IsKeyPressed(Key.Shift) ? 5 : 1;
					foreach (var (key, direction) in dragKeys) {
						if (SceneView.Input.WasKeyRepeated(key)) {
							offset.X += direction.X * isAccelerated;
							offset.Y += direction.Y * isAccelerated;
						}
					}
					DragNodes(offset);
					yield return null;
				}
				Document.Current.History.CommitTransaction();
			}
		}

		private static void DragNodes(Vector2 delta)
		{
			DragWidgets(delta);
			DragNodes3D(delta);
			DragSplinePoints3D(delta);
		}

		private static void DragWidgets(Vector2 delta)
		{
			if (Document.Current.Container is Widget containerWidget) {
				var transform = containerWidget.LocalToWorldTransform.CalcInversed();
				var dragDelta = transform * delta - transform * Vector2.Zero;
				foreach (var widget in Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
					SetAnimableProperty.Perform(widget, nameof(Widget.Position), widget.Position + dragDelta, CoreUserPreferences.Instance.AutoKeyframes);
				}
			}
		}

		private static void DragNodes3D(Vector2 delta)
		{
			foreach (var node3D in Document.Current.SelectedNodes().Editable().OfType<Node3D>()) {
				SetAnimableProperty.Perform(node3D, nameof(Widget.Position), node3D.Position + (Vector3)delta / 100, CoreUserPreferences.Instance.AutoKeyframes);
			}
		}

		static void DragSplinePoints3D(Vector2 delta)
		{
			foreach (var point in Document.Current.SelectedNodes().Editable().OfType<SplinePoint3D>()) {
				SetAnimableProperty.Perform(point, nameof(Widget.Position), point.Position + (Vector3)delta / 100, CoreUserPreferences.Instance.AutoKeyframes);
			}
		}

		private static IEnumerator<object> DragByMouse()
		{
			var initialMousePos = SceneView.MousePosition;
			while (
				SceneView.Input.IsKeyPressed(Key.Alt) &&
				(SceneView.MousePosition - initialMousePos).Length <= Threshold &&
				SceneView.Input.IsMousePressed()
			) {
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
				Utils.CalcHullAndPivot(widgets, out _, out var pivot);
				pivot = pivot * Document.Current.Container.AsWidget.LocalToWorldTransform.CalcInversed();
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
								points.Add(widget.GlobalPivotPosition);
							}
							if (SceneViewCommands.SnapWidgetBorderToRuler.Checked) {
								points.AddRange(widget.CalcHull());
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
				return snappedPoint * Document.Current.RootNode.AsWidget.LocalToWorldTransform;
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
					var transformedPosition = pos * Document.Current.RootNode.AsWidget.LocalToWorldTransform.CalcInversed();
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
