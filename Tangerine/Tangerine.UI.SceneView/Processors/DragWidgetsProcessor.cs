using System;
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

		private static readonly Vector2[] directions = {
			-Vector2.Half,
			new Vector2(-0.5f, 0.5f),
			Vector2.Half,
			new Vector2(0.5f, -0.5f),
		};

		IEnumerator<object> Drag()
		{
			Document.Current.History.BeginTransaction();
			try {
				var iniMousePos = sv.MousePosition;
				var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>().ToList();
				var transform = sv.Scene.CalcTransitionToSpaceOf(Document.Current.Container.AsWidget);
				var dragDirection = DragDirection.Any;
				var positions = widgets.Select(i => i.Position).ToList();
				Quadrangle hull;
				Vector2 pivot;
				Utils.CalcHullAndPivot(widgets, Document.Current.Container.AsWidget, out hull, out pivot);
				while (sv.Input.IsMousePressed()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					var curMousePos = sv.MousePosition;
					var shiftPressed = sv.Input.IsKeyPressed(Key.Shift);
					if (shiftPressed && dragDirection != DragDirection.Any) {
						if (dragDirection == DragDirection.Horizontal) {
							curMousePos.Y = iniMousePos.Y;
						} else if (dragDirection == DragDirection.Vertical) {
							curMousePos.X = iniMousePos.X;
						}
					}
					var dragDelta = curMousePos * transform - iniMousePos * transform;
					if (shiftPressed && dragDirection == DragDirection.Any && (curMousePos - iniMousePos).Length > 5 / sv.Scene.Scale.X) {
						var d = curMousePos - iniMousePos;
						dragDirection = d.X.Abs() > d.Y.Abs() ? DragDirection.Horizontal : DragDirection.Vertical;
					}
					dragDelta = dragDelta.Snap(Vector2.Zero);
					var lines = GetRulerLines();
					if (dragDelta != Vector2.Zero) {
						SnapPoints(widgets, positions, dragDelta, lines, hull);
					}
					yield return null;
				}
			} finally {
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.EndTransaction();
			}
		}

		private static void SnapPoints(List<Widget> widgets, List<Vector2> positions, Vector2 dragDelta, List<RulerLine> lines, Quadrangle hull)
		{
			if (SceneViewCommands.SnapWidgetBorderToRuler.Checked) {
				var delta = Vector2.Zero;
				for (var j = 0; j < 4; j += 2) {
					delta += SnapPointToLines(hull[j] + dragDelta, lines) - (hull[j] + dragDelta);
				}
				dragDelta += delta;
			}
			for (int i = 0; i < widgets.Count; i++) {
				var pos = positions[i] + dragDelta;
				if (SceneViewCommands.SnapWidgetPivotToRuler.Checked) {
					pos = SnapPointToLines(pos, lines);
				}
				Core.Operations.SetAnimableProperty.Perform(widgets[i], nameof(Widget.Position), pos);
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