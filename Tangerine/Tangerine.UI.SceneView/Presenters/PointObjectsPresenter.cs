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
				Document.Current.Container.IsRunning
			) {
				return;
			}
			canvas.PrepareRendererState();
			var pointObjects = Document.Current.Container.Nodes.OfType<PointObject>().ToList();
			var t = Document.Current.Container.AsWidget.CalcTransitionToSpaceOf(canvas);
			var sel = Document.Current.SelectedNodes().Editable().ToList();
			foreach (var po in pointObjects) {
				DrawPointObject(t * po.TransformedPosition, selected: sel.Contains(po));
			}
		}

		void DrawPointObject(Vector2 position, bool selected)
		{
			Renderer.DrawRect(
				position - Vector2.One * 3, position + Vector2.One * 3,
				selected ? SceneViewColors.Selection : SceneViewColors.PointObject);
		}
	}
}