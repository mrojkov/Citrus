using System;
using System.Linq;
using Lime;

namespace Tangerine.UI.SceneView
{
	class TransparentWidgetsPresenter
	{
		public TransparentWidgetsPresenter(SceneView sceneView)
		{
			sceneView.Frame.CompoundPostPresenter.Add(new DelegatePresenter<Widget>(RenderTransparentWidgetPivots));
		}

		private void RenderTransparentWidgetPivots(Widget canvas)
		{
			if (
				Core.Document.Current.ExpositionMode ||
				Core.Document.Current.Container.IsRunning
			) {
				return;
			}
			canvas.PrepareRendererState();
			var widgets = Core.Document.Current.Container.Nodes.Editable().
				OfType<Widget>().Where(w => w.Color.A == 0).ToList();
			if (widgets.Count == 0) {
				return;
			}
			var iconSize = new Vector2(16, 16);
			foreach (var widget in widgets) {
				var t = NodeIconPool.GetTexture(widget.GetType());
				var p = widget.CalcPositionInSpaceOf(canvas);
				Renderer.DrawSprite(t, Color4.White, p - iconSize / 2, iconSize, Vector2.Zero, Vector2.One);
			}
		}
	}
}