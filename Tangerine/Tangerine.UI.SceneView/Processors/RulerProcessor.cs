using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class RulerProcessor : ITaskProvider
	{
		private const float Threshold = 5f;
		private static SceneView sceneView => SceneView.Instance;
		private static Widget container => Document.Current.RootNode.AsWidget;
		private static Ruler Ruler => ProjectUserPreferences.Instance.ActiveRuler;

		public IEnumerator<object> Task()
		{
			while (true) {
				if (ProjectUserPreferences.Instance.RulerVisible &&
					!Document.Current.ExpositionMode &&
					!Document.Current.PreviewScene &&
					sceneView.Frame.ParentWidget.IsMouseOverThisOrDescendant()
				) {
					RulerLine line;
					bool lineCaptured;
					var hulls = Document.Current.Container.Nodes.OfType<Widget>().Editable()
						.Select(w => w.CalcHullInSpaceOf(container)).ToList();
					if (lineCaptured = SceneView.Instance.Components.Contains<CreateLineRequestComponent>()) {
						var comp = SceneView.Instance.Components.Get<CreateLineRequestComponent>();
						line = new RulerLine(Vector2.Zero, comp.Orientation);
						SnapLineToNearestPoint(line, hulls);
						if (IsRulerContainLine(line.Value, line.RulerOrientation)) {
							line = GetLineUnderMouse();
						}
						SceneView.Instance.Components.Remove<CreateLineRequestComponent>();
					} else {
						line = GetLineUnderMouse();
					}
					if (line != null) {
						Utils.ChangeCursorIfDefault(line.RulerOrientation == RulerOrientation.Horizontal ? MouseCursor.SizeNS : MouseCursor.SizeWE);
						if (sceneView.Input.ConsumeKeyPress(Key.Mouse0) || lineCaptured) {
							Window.Current.Invalidate();
							using (Document.Current.History.BeginTransaction()) {
								if (Ruler.ContainsLine(line)) {
									Core.Operations.DeleteRuler.Perform(Ruler, line = line.Clone());
								}
								line = line.Clone();
								SceneView.Instance.Components.GetOrAdd<LineSelectionComponent>().Line = line;
								while (Window.Current.Input.IsMousePressed()) {
									SnapLineToNearestPoint(line, hulls);
									yield return null;
								}
								Core.Operations.CreateRuler.Perform(ProjectUserPreferences.Instance.ActiveRuler, line = line.Clone());
								Document.Current.History.CommitTransaction();
							}
							Window.Current.Invalidate();
							SceneView.Instance.Components.Remove<LineSelectionComponent>();
						} else if (Window.Current.Input.WasMouseReleased(1)) {
							using (Document.Current.History.BeginTransaction()) {
								Core.Operations.DeleteRuler.Perform(ProjectUserPreferences.Instance.ActiveRuler, line);
								Document.Current.History.CommitTransaction();
							}
							Window.Current.Invalidate();
						}
					}
				}
				yield return null;
			}
		}

		public static bool IsRulerContainLine(float value, RulerOrientation orientation) =>
			Ruler.Lines.Any(l => l.RulerOrientation == orientation && Math.Abs(l.Value - value) < Mathf.ZeroTolerance);

		private static void SnapLineToNearestPoint(RulerLine line, IEnumerable<Quadrangle> hulls)
		{
			var step = 1f;
			var mousePos = container.LocalMousePosition();
			if (!sceneView.Input.IsKeyPressed(Key.Shift)) {
				step = (float)Math.Truncate(RulersWidget.CalculateEffectiveStep()) / RulersWidget.Tesselation;
			}
			var curValue = new Vector2(Utils.RoundTo(mousePos.X, step), Utils.RoundTo(mousePos.Y, step));

			if (SceneViewCommands.SnapRulerLinesToWidgets.Checked) {
				var mask = line.RulerOrientation.GetDirection();
				foreach (var hull in hulls) {
					for (int i = 0; i < 4; i++) {
						if (((hull[i] - mousePos) * mask).Length < ((mousePos - curValue) * mask).Length &&
							!IsRulerContainLine(line.RulerOrientation.GetComponentFor(hull[i]), line.RulerOrientation)
						) {
							line.MakePassingThroughPoint(hull[i]);
							Window.Current.Invalidate();
							return;
						}
					}
				}
			}

			if (!IsRulerContainLine(line.RulerOrientation.GetComponentFor(curValue), line.RulerOrientation)) {
				line.MakePassingThroughPoint(curValue);
				Window.Current.Invalidate();
			}
		}

		private RulerLine GetLineUnderMouse()
		{
			var pos = SceneView.Instance.Scene.LocalMousePosition();
			return ProjectUserPreferences.Instance.ActiveRuler.Lines.FirstOrDefault(line => {
				var mask = line.RulerOrientation.GetDirection();
				return ((line.GetClosestPointToOrigin() - pos) * mask).Length <= Threshold / sceneView.Scene.Scale.X;
			});
		}
	}
}
