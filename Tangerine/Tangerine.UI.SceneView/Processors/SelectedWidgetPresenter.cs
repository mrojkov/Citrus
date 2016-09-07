using System;
using System.Linq;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.SceneView
{
	class SelectedWidgetsPresenter : Core.IProcessor
	{
		SceneView sceneView => SceneView.Instance;

		public IEnumerator<object> Loop()
		{
			var p = new DelegatePresenter<Widget>(canvas => {
				if (sceneView.Components.Get<ExpositionComponent>().InProgress) {
					return;
				}
				canvas.PrepareRendererState();
				foreach (var widget in Core.Document.Current.SelectedNodes.OfType<Widget>()) {
					widget.PrepareRendererState();
					var color = widget.GetTangerineFlag(TangerineFlags.Locked) ?
						Colors.SceneView.LockedWidgetBorder : Colors.SceneView.SelectedWidgetBorder;
					Renderer.DrawRectOutline(Vector2.Zero, widget.Size, color, 1);
				}
			});
			sceneView.CanvasWidget.CompoundPostPresenter.Add(p);
			while (true) {
				yield return null;
			}
		}
	}
}