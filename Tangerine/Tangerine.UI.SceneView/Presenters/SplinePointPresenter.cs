using System;
using Lime;
using Tangerine.Core;
using System.Linq;
using System.Collections.Generic;

namespace Tangerine.UI.SceneView
{
	public class SplinePointPresenter
	{
		private static List<SplinePoint> emptySelection = new List<SplinePoint>();
		private SceneView sv;

		public SplinePointPresenter(SceneView sceneView)
		{
			sv = sceneView;
			sceneView.Frame.CompoundPostPresenter.Add(new DelegatePresenter<Widget>(Render));
		}

		public const float TangentWeightRatio = 10f;

		private static void Render(Widget widget)
		{
			if (!Document.Current.PreviewAnimation && Document.Current.Container is Spline) {
				var spline = Document.Current.Container;
				foreach (SplinePoint point in spline.Nodes) {
					var color = GetSelectedPoints().Contains(point) ?
					ColorTheme.Current.SceneView.Selection :
					ColorTheme.Current.SceneView.PointObject;
					SceneView.Instance.Frame.PrepareRendererState();
					var t = point.Parent.AsWidget.CalcTransitionToSpaceOf(SceneView.Instance.Frame);
					var a = Vector2.CosSin(point.TangentAngle * Mathf.DegToRad) * TangentWeightRatio * point.TangentWeight;
					var p1 = t * (point.TransformedPosition + a);
					var p2 = t * (point.TransformedPosition - a);
					var norm = (p2 - p1).Normalized;
					norm = new Vector2(-norm.Y, norm.X);
					Renderer.DrawLine(p1 + norm, p2 + norm, ColorTheme.Current.SceneView.SplineOutline);
					Renderer.DrawLine(p1, p2, color);
					Renderer.DrawLine(p1 - norm, p2 - norm, ColorTheme.Current.SceneView.SplineOutline);
					Renderer.DrawRound(p1, 5, 10, ColorTheme.Current.SceneView.SplineOutline);
					Renderer.DrawRound(p1, 3, 10, color);
					Renderer.DrawRound(p2, 5, 10, ColorTheme.Current.SceneView.SplineOutline);
					Renderer.DrawRound(p2, 3, 10, color);
				}
			}
		}

		static List<SplinePoint> GetSelectedPoints()
		{
			if (Document.Current.Container is Spline) {
				return Document.Current.SelectedNodes().OfType<SplinePoint>().Editable().ToList();
			}
			return emptySelection;
		}
	}
}
