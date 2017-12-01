using System;
using System.Linq;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.SceneView
{
	class SelectedWidgetsPresenter
	{
		public SelectedWidgetsPresenter(SceneView sceneView)
		{
			sceneView.Frame.CompoundPostPresenter.Add(new DelegatePresenter<Widget>(RenderSelection));
		}

		private void RenderSelection(Widget canvas)
		{
			if (
				Core.Document.Current.ExpositionMode ||
				Core.Document.Current.Container.IsRunning
			) {
				return;
			}
			canvas.PrepareRendererState();
			var widgets = Core.Document.Current.SelectedNodes().Editable().OfType<Widget>().ToList();
			if (widgets.Count == 0) {
				return;
			}
			// Render node icons.
			var iconSize = new Vector2(16, 16);
			foreach (var widget in widgets) {
				var t = NodeIconPool.GetTexture(widget.GetType());
				var p = widget.CalcPositionInSpaceOf(canvas);
				Renderer.DrawSprite(t, Color4.White, p - iconSize / 2, iconSize, Vector2.Zero, Vector2.One);
			}
			Quadrangle hull;
			Vector2 pivot;
			Utils.CalcHullAndPivot(widgets, canvas, out hull, out pivot);
			// Render rectangles.
			var locked = widgets.Any(w => w.GetTangerineFlag(TangerineFlags.Locked));
			var color = locked ? ColorTheme.Current.SceneView.LockedWidgetBorder : ColorTheme.Current.SceneView.Selection;
			for (int i = 0; i < 4; i++) {
				var a = hull[i];
				var b = hull[(i + 1) % 4];
				Renderer.DrawLine(a, b, color, 1);
				DrawStretchMark(a);
				DrawStretchMark((a + b) / 2);
			}
			// Render multi-pivot mark.
			if (widgets.Count > 1) {
				DrawMultiPivotMark(pivot);
			}
		}

		void DrawStretchMark(Vector2 position)
		{
			Renderer.DrawRect(position - Vector2.One * 3, position + Vector2.One * 3, ColorTheme.Current.SceneView.Selection);
		}

		void DrawMultiPivotMark(Vector2 position)
		{
			Renderer.DrawRect(position - Vector2.One * 5, position + Vector2.One * 5, ColorTheme.Current.SceneView.Selection);
		}
	}
}