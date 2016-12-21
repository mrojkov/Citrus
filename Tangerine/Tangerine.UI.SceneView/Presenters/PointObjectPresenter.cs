using System;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class PointObjectPresenter : CustomPresenter<PointObject>
	{
		protected override void InternalRender(PointObject pointObject)
		{
			if (Document.Current.Container.IsRunning) {
				return;
			}
			SceneView.Instance.Frame.PrepareRendererState();
			var t = pointObject.Parent.AsWidget.CalcTransitionToSpaceOf(SceneView.Instance.Frame);
			Renderer.DrawRound(t * pointObject.TransformedPosition, 3, 10, SceneViewColors.PointObject);
		}
	}
}