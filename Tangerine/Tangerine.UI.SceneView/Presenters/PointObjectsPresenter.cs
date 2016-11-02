using System;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	class PointObjectsPresenter
	{
		public PointObjectsPresenter(SceneView sceneView)
		{
			sceneView.Frame.CompoundPostPresenter.Add(new DelegatePresenter<Widget>(Render));
		}

		private void Render(Widget canvas)
		{
			if (
				SceneView.Instance.Components.Get<ExpositionComponent>().InProgress ||
				Document.Current.Container.IsRunning ||
				!Document.Current.Container.Nodes.Any(i => i is PointObject)
			) {
				return;
			}
			canvas.PrepareRendererState();
			var pointObjects = Document.Current.Container.Nodes.OfType<PointObject>().ToList();
			var t = Document.Current.Container.AsWidget.CalcTransitionToSpaceOf(canvas);
			var selectedPointObjects = Document.Current.SelectedNodes().OfType<PointObject>().Editable().ToList();
			foreach (var po in pointObjects) {
				DrawPointObject(t * po.TransformedPosition, selected: selectedPointObjects.Contains(po));
			}
			Rectangle aabb;
			if (Utils.CalcAABB(selectedPointObjects, canvas, out aabb)) {
				Renderer.DrawRectOutline(aabb.A, aabb.B, SceneViewColors.Selection);
			}
			DrawPivot(aabb.Center);
		}

		void DrawPointObject(Vector2 position, bool selected)
		{
			Renderer.DrawRect(
				position - Vector2.One * 3, position + Vector2.One * 3,
				selected ? SceneViewColors.Selection : SceneViewColors.PointObject);
		}

		void DrawPivot(Vector2 position)
		{
			Renderer.DrawRect(position - Vector2.One * 5, position + Vector2.One * 5, SceneViewColors.Selection);
		}
	}
}