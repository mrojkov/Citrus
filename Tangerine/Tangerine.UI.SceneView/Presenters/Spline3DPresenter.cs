using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class Spline3DPresenter : CustomPresenter<Viewport3D>
	{
		List<SplinePoint3D> emptySelection = new List<SplinePoint3D>();
		List<Vector3> splineApproximation = new List<Vector3>();

		protected override void InternalRender(Viewport3D viewport)
		{
			if (Document.Current.PreviewAnimation) {
				return;
			}
			foreach (var spline in viewport.Descendants.OfType<Spline3D>()) {
				Renderer.Flush();
				SceneView.Instance.Frame.PrepareRendererState();
				var cameraProjection = Renderer.Projection;
				var oldWorldMatrix = Renderer.World;
				var oldViewMatrix = Renderer.View;
				var oldCullMode = Renderer.CullMode;
				Renderer.World = Matrix44.Identity;
				Renderer.View = Matrix44.Identity;
				Renderer.CullMode = CullMode.None;
				Renderer.Projection = ((WindowWidget)spline.GetRoot()).GetProjection();
				DrawSpline(spline, viewport);
				if (Document.Current.Container == spline) {
					var selectedPoints = GetSelectedPoints();
					foreach (var p in spline.Nodes) {
						DrawSplinePoint((SplinePoint3D)p, viewport, spline.GlobalTransform, selectedPoints.Contains(p));
					}
				}
				Renderer.Flush();
				Renderer.Projection = cameraProjection;
				Renderer.World = oldWorldMatrix;
				Renderer.View = oldViewMatrix;
				Renderer.CullMode = oldCullMode;
			}
		}
 
		void DrawSplinePoint(SplinePoint3D point, Viewport3D viewport, Matrix44 splineWorldMatrix, bool selected)
		{
			var color = selected ? Color4.Green : Color4.Red;
			var viewportToSceneFrame = viewport.CalcTransitionToSpaceOf(SceneView.Instance.Frame);
			var a = (Vector2)viewport.WorldToViewportPoint(point.Position * splineWorldMatrix) * viewportToSceneFrame;
			Renderer.DrawRect(a - Vector2.One * 3, a + Vector2.One * 3, color);
			for (int i = 0; i < 2; i++) {
				var t = i == 0 ? point.TangentA : point.TangentB;
				var b = (Vector2)viewport.WorldToViewportPoint((point.Position + t) * splineWorldMatrix) * viewportToSceneFrame;
				Renderer.DrawLine(a, b, color, 1);
				Renderer.DrawRect(b - Vector2.One * 1.5f, b + Vector2.One * 1.5f, color);
			}
		}

		List<SplinePoint3D> GetSelectedPoints()
		{
			if (Document.Current.Container is Spline3D) {
				return Document.Current.SelectedNodes().OfType<SplinePoint3D>().Editable().ToList();
			}
			return emptySelection;
		}

		void DrawSpline(Spline3D spline, Viewport3D viewport)
		{
			var splineWorldMatrix = spline.GlobalTransform;
			var viewportToSceneFrame = viewport.CalcTransitionToSpaceOf(SceneView.Instance.Frame);
			for (var i = 0; i < spline.Nodes.Count - 1; i++) {
				var n1 = (SplinePoint3D)spline.Nodes[i];
				var n2 = (SplinePoint3D)spline.Nodes[i + 1];
				splineApproximation.Clear();
				splineApproximation.Add(n1.Position);
				Approximate(n1.Position, n1.Position + n1.TangentA, n2.Position + n2.TangentB, n2.Position, 0.01f, 0, splineApproximation);
				splineApproximation.Add(n2.Position);
				var start = Vector2.Zero;
				for (var j = 0; j < splineApproximation.Count; j++) {
					var end = (Vector2)viewport.WorldToViewportPoint(splineApproximation[j] * spline.GlobalTransform) * viewportToSceneFrame;
					if (j > 0) {
						SplinePresenter.DrawColoredLine(start, end, Color4.White, Color4.Black);
					}
					start = end;
				}
			}
		}

		void Approximate(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float flatness, int level, List<Vector3> points)
		{
			if (level == 10)
				return;
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