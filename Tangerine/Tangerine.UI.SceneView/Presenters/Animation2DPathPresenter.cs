using Lime;
using Tangerine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.UI.SceneView
{
	public class Animation2DPathPresenter : CustomPresenter
	{
		private List<Vector2> points = new List<Vector2>();
		private List<Vector2> approximation = new List<Vector2>();

		public override void Render(Node node)
		{
			if (Document.Current.PreviewAnimation) {
				return;
			}
			if (!CoreUserPreferences.Instance.ShowAnimationPath) {
				return;
			}
			if (node is IAnimable) {
				var animable = node as IAnimable;
				foreach (var animator in animable.Animators) {
					if (
						animator is Vector2Animator &&
						animator.TargetProperty == nameof(Widget.Position)
					) {
						var keys = animator.ReadonlyKeys.ToList();
						if (keys.Count < 2) {
							continue;
						}
						points.Clear();
						for (int i = 0; i < keys.Count; ++i) {
							points.Add((Vector2)keys[i].Value);
						}
						var transform = node.Parent.AsWidget.CalcTransitionToSpaceOf(SceneView.Instance.Frame);
						SceneView.Instance.Frame.PrepareRendererState();
						for (int i = 0; i < points.Count - 1; ++i) {
							Approximate(i, i + 1, keys[i].Function, 10, transform);
							for (int j = 0; j < approximation.Count - 1; ++j) {
								Renderer.DrawDashedLine(
									approximation[j],
									approximation[j + 1],
									ColorTheme.Current.SceneView.PointObject,
									new Vector2(4, 1)
								);
							}
							Renderer.DrawRound(points[i] * transform, 3, 10, ColorTheme.Current.SceneView.PointObject.Darken(0.3f));
						}
						Renderer.DrawRound(points[points.Count - 1] * transform, 3, 10, ColorTheme.Current.SceneView.PointObject.Darken(0.3f));
					}
				}
			}
		}

		private void Approximate(int index1, int index2, KeyFunction keyFunction, int numberOfPoints, Matrix32 transform)
		{
			int index0;
			int index3;
			approximation.Clear();
			if (keyFunction == KeyFunction.Spline) {
				index0 = index1 < 1 ? 0 : index1 - 1;
				index3 = index2 >= points.Count - 1 ? points.Count - 1 : index2 + 1;
			} else if (keyFunction == KeyFunction.ClosedSpline) {
				index0 = index1 < 1 ? points.Count - 1 : index1 - 1;
				index3 = index2 >= points.Count - 1 ? 0 : index2 + 1;
			} else {
				approximation.Add(points[index1] * transform);
				approximation.Add(points[index2] * transform);
				return;
			}
			approximation.Add(points[index1] * transform);
			for (int i = 1; i < numberOfPoints - 1; ++i) {
				approximation.Add(
					Mathf.CatmullRomSpline(
						(float)i / numberOfPoints,
						points[index0],
						points[index1],
						points[index2],
						points[index3]
					) * transform
				);
			}
			approximation.Add(points[index2] * transform);
		}
	}
}
