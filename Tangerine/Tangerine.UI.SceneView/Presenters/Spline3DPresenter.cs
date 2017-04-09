using System;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class Spline3DPresenter : CustomPresenter<Spline3D>
	{
		protected override void InternalRender(Spline3D spline)
		{
			if (Document.Current.Container.IsRunning) {
				return;
			}
			Renderer.Flush();
			SceneView.Instance.Frame.PrepareRendererState();
			var cameraProjection = Renderer.Projection;
			var oldWorldMatrix = Renderer.World;
			Renderer.Projection = ((WindowWidget)spline.GetRoot()).GetProjection();
			var vp = spline.GetViewport();
			DrawSpline(spline, vp);
			foreach (var p in spline.Nodes) {
				DrawSplinePoint((SplinePoint3D)p, vp, spline.GlobalTransform);
			}
			Renderer.Flush();
			Renderer.Projection = cameraProjection;
			Renderer.World = oldWorldMatrix;
		}

		void DrawSplinePoint(SplinePoint3D point, Viewport3D viewport, Matrix44 splineWorldMatrix)
		{
			for (int i = 0; i < 2; i++) {
				var t = i == 0 ? point.TangentA : point.TangentB;
				var a = (Vector2)viewport.WorldToViewportPoint(point.Position * splineWorldMatrix);
				var b = (Vector2)viewport.WorldToViewportPoint((point.Position + t) * splineWorldMatrix);
				Renderer.World = Matrix44.Identity;
				Renderer.CullMode = CullMode.None;
				var rectHalfSize = new Vector2(2, 2);
				var viewportToSceneFrame = viewport.CalcTransitionToSpaceOf(SceneView.Instance.Frame);
				a *= viewportToSceneFrame;
				b *= viewportToSceneFrame;
				Renderer.DrawLine(a, b, Color4.Red, 1);
				Renderer.DrawRect(b - rectHalfSize, b + rectHalfSize, Color4.Red);
			}
		}

		void DrawSpline(Spline3D spline, Viewport3D viewport)
		{
			var splineWorldMatrix = spline.GlobalTransform;
			var viewportToSceneFrame = viewport.CalcTransitionToSpaceOf(SceneView.Instance.Frame);
			var points = new System.Collections.Generic.List<Vector3>();
			for (var i = 0; i < spline.Nodes.Count - 1; i++) {
				var n1 = (SplinePoint3D)spline.Nodes[i];
				var n2 = (SplinePoint3D)spline.Nodes[i + 1];
				points.Clear();
				points.Add(n1.Position);
				Approximate(n1.Position, n1.Position + n1.TangentA, n2.Position + n2.TangentB, n2.Position, 0.01f, 0, points);
				points.Add(n2.Position);
				var start = Vector2.Zero;
				for (var j = 0; j < points.Count; j++) {
					var end = (Vector2)viewport.WorldToViewportPoint(points[j] * spline.GlobalTransform) * viewportToSceneFrame;
					if (j > 0) {
						SplinePresenter.DrawColoredLine(start, end, Color4.White, Color4.Black);
					}
					start = end;
				}
			}
		}

		void Approximate(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float flatness, int level, System.Collections.Generic.List<Vector3> points)
		{
			if (level == 10) {
				return;
			}
			var d12 = p2 - p1;
			var d13 = p3 - p1;
			var d14 = p4 - p1;
			var n1 = Vector3.CrossProduct(d12, d14);
			var n2 = Vector3.CrossProduct(d13, d14);
			var bn1 = Vector3.CrossProduct(d14, n1);
			var bn2 = Vector3.CrossProduct(d14, n2);
			var h1 = Mathf.Abs(Vector3.DotProduct(d12, bn1.Normalized));
			var h2 = Mathf.Abs(Vector3.DotProduct(d13, bn2.Normalized));
			if (h1 + h2 < flatness)
				return;
			var p12 = (p1 + p2) * 0.5f;
			var p23 = (p2 + p3) * 0.5f;
			var p34 = (p3 + p4) * 0.5f;
			var p123 = (p12 + p23) * 0.5f;
			var p234 = (p23 + p34) * 0.5f;
			var p1234 = (p123 + p234) * 0.5f;
			Approximate(p1, p12, p123, p1234, flatness, level + 1, points);
			points.Add(p1234);
			Approximate(p1234, p234, p34, p4, flatness, level + 1, points);
		}
	}
}