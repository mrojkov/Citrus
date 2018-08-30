using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

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
				Core.Document.Current.PreviewAnimation
			) {
				return;
			}
			canvas.PrepareRendererState();
			var widgets = Core.Document.Current.SelectedNodes().Editable().OfType<Widget>().ToList();
			if (widgets.Count == 0) {
				return;
			}
			// Render boreder and icon for widgets.
			var iconSize = new Vector2(16, 16);
			foreach (var widget in widgets) {
				var t = NodeIconPool.GetTexture(widget.GetType());
				var h = widget.CalcHullInSpaceOf(canvas);
				for (int i = 0; i < 4; i++) {
					var a = h[i];
					var b = h[(i + 1) % 4];
					Renderer.DrawLine(a, b, ColorTheme.Current.SceneView.SelectedWidget, 1);
				}
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
				Renderer.DrawLine(a, b, color);
				DrawStretchMark(a);

				if (i < 2) {
					var c = hull[(i + 2) % 4];
					var d = hull[(i + 3) % 4];
					var abCenter = (a + b) * 0.5f;
					var cdCenter = (c + d) * 0.5f;
					Renderer.DrawLine(abCenter, cdCenter, color);
					DrawStretchMark(abCenter);
					DrawStretchMark(cdCenter);
				}
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