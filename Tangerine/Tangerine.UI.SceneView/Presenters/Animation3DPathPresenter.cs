using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class Animation3DPathPresenter : CustomPresenter<Viewport3D>
	{
		protected override void InternalRender(Viewport3D viewport)
		{
			if (Document.Current.PreviewAnimation) {
				return;
			}
			if (!CoreUserPreferences.Instance.ShowAnimationPath) {
				return;
			}
			foreach (var node in viewport.Descendants.OfType<Node3D>()) {
				if (!(node is IAnimable)) {
					continue;
				}
				var animable = (IAnimable)node;
				foreach (var animator in animable.Animators) {
					if (
						animator is Vector3Animator &&
						animator.TargetProperty == nameof(Node3D.Position)
					) {
						var keys = animator.ReadonlyKeys.ToList();
						if (keys.Count == 0) {
							continue;
						}
						var points = new List<Vector3>();
						for (int i = 0; i < keys.Count; ++i) {
							points.Add((Vector3)keys[i].Value);
						}
						if (points.Count < 2) {
							continue;
						}
						var worldTransform = Matrix44.Identity;
						if (node.Parent.AsNode3D != null) {
							worldTransform = node.Parent.AsNode3D.GlobalTransform;
						}
						var viewportToSceneFrame = viewport.CalcTransitionToSpaceOf(SceneView.Instance.Frame);
						SceneView.Instance.Frame.PrepareRendererState();
						for (int i = 0; i < points.Count - 1; ++i) {
							var approximation = Approximate(points, i, i + 1, keys[i].Function, 10);
							var start = (Vector2)viewport.WorldToViewportPoint(approximation[0] * worldTransform) * viewportToSceneFrame;
							for (int j = 1; j < approximation.Count; ++j) {
								var end = (Vector2)viewport.WorldToViewportPoint(approximation[j] * worldTransform) * viewportToSceneFrame;
								Renderer.DrawDashedLine(
									start, end, ColorTheme.Current.SceneView.PointObject, new Vector2(4, 1));
								start = end;
							}
						}
					}
				}
			}
		}

		private static List<Vector3> Approximate(List<Vector3> points, int index1, int index2, KeyFunction keyFunction, int numberOfPoints)
		{
			int index0;
			int index3;
			if (keyFunction == KeyFunction.Spline) {
				index0 = index1 < 1 ? 0 : index1 - 1;
				index3 = index2 >= points.Count - 1 ? points.Count - 1 : index2 + 1;
			} else if (keyFunction == KeyFunction.ClosedSpline) {
				index0 = index1 < 1 ? points.Count - 1 : index1 - 1;
				index3 = index2 >= points.Count - 1 ? 0 : index2 + 1;
			} else {
				return new List<Vector3> { points[index1], points[index2] };
			}
			var result = new List<Vector3> { points[index1] };
			for (int i = 1; i < numberOfPoints - 1; ++i) {
				result.Add(
					Mathf.CatmullRomSpline(
						(float)i / numberOfPoints,
						points[index0],
						points[index1],
						points[index2],
						points[index3]
					)
				);
			}
			result.Add(points[index2]);
			return result;
		}
	}
}
