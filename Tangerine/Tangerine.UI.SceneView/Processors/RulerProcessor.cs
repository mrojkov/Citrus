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

		public IEnumerator<object> Task()
		{
			while (true) {
				if (ProjectUserPreferences.Instance.RulerVisible) {
					var line = GetLineUnderMouse();
					if (line != null) {
						Utils.ChangeCursorIfDefault(line.RulerOrientation == RulerOrientation.Horizontal ? MouseCursor.SizeNS : MouseCursor.SizeWE);
						if (sceneView.Input.ConsumeKeyPress(Key.Mouse0)) {
							SceneView.Instance.Components.GetOrAdd<LineSelectionComponent>().Line = line;
							while (Window.Current.Input.IsMousePressed()) {
								var mousePos = container.LocalMousePosition();
								var step = 1f;
								if (sceneView.Input.IsKeyPressed(Key.Shift)) {
									step = (float)Math.Truncate(RulersWidget.CalculateEffectiveStep()) /
										  (RulersWidget.SegmentsTesselation * (1 << (RulersWidget.SegmentLevelsCount - 1)));
								}
								var pos = new Vector2(Utils.RoundTo(mousePos.X, step), Utils.RoundTo(mousePos.Y, step));
								line.MakePassingThroughPoint(pos);
								Window.Current.Invalidate();
								yield return null;
							}
							SceneView.Instance.Components.Remove<LineSelectionComponent>();
						} else if (Window.Current.Input.WasMouseReleased(1)) {
							ProjectUserPreferences.Instance.ActiveRuler.Lines.Remove(line);
							Window.Current.Invalidate();
						}
					}
				}
				yield return null;
			}
		}

		private RulerLine GetLineUnderMouse()
		{
			var pos = SceneView.Instance.Scene.LocalMousePosition();
			return ProjectUserPreferences.Instance.ActiveRuler.Lines.FirstOrDefault(line => {
				var mask = line.RulerOrientation == RulerOrientation.Vertical ? Vector2.Right : Vector2.Down;
				return ((line.GetClosestPointToOrigin() - pos) * mask).Length <= Threshold / sceneView.Scene.Scale.X;
			});
		}
	}
}
