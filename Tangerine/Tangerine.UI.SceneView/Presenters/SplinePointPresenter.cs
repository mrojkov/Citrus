using System;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class SplinePointPresenter : CustomPresenter<SplinePoint>
	{
		protected override void InternalRender(SplinePoint point)
		{
			if (Document.Current.Container.IsRunning) {
				return;
			}
			SceneView.Instance.Frame.PrepareRendererState();
			var t = point.Parent.AsWidget.CalcTransitionToSpaceOf(SceneView.Instance.Frame);
			var a = Vector2.CosSin(point.TangentAngle * Mathf.DegToRad) * 10 * point.TangentWeight;
			var p1 = t * (point.TransformedPosition + a);
			var p2 = t * (point.TransformedPosition - a);
			Renderer.DrawLine(p1, p2, SceneViewColors.PointObject);
			Renderer.DrawRound(p1, 3, 10, SceneViewColors.PointObject);
			Renderer.DrawRound(p2, 3, 10, SceneViewColors.PointObject);
		}
	}
}
