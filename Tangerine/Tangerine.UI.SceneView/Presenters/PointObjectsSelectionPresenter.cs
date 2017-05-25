using System;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	class PointObjectsSelectionPresenter
	{
		public PointObjectsSelectionPresenter(SceneView sceneView)
		{
			sceneView.Frame.CompoundPostPresenter.Add(new DelegatePresenter<Widget>(Render));
		}

		void Render(Widget canvas)
		{
			if (
				Document.Current.ExpositionMode ||
				Document.Current.Container.IsRunning ||
				!Document.Current.Container.Nodes.Any(i => i is PointObject)
			) {
				return;
			}
			canvas.PrepareRendererState();
			var t = Document.Current.Container.AsWidget.CalcTransitionToSpaceOf(canvas);
			var selectedPointObjects = Document.Current.SelectedNodes().OfType<PointObject>().Editable().ToList();
			foreach (var po in selectedPointObjects) {
				DrawPointObject(t * po.TransformedPosition);
			}
			Rectangle aabb;
			if (Utils.CalcAABB(selectedPointObjects, canvas, out aabb)) {
				aabb = aabb.ExpandedBy(new Thickness(10));
				Renderer.DrawRectOutline(aabb.A, aabb.B, ColorTheme.Current.SceneView.Selection);
			}
			DrawPivot(aabb.Center);
		}

		void DrawPointObject(Vector2 position)
		{
			Renderer.DrawRound(position, 3, 10, ColorTheme.Current.SceneView.Selection);
		}

		void DrawPivot(Vector2 position)
		{
			Renderer.DrawRound(position, 3, 10, ColorTheme.Current.SceneView.Selection);
		}
	}
}