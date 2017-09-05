using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Tangerine.UI.SceneView
{
	class PointObjectsSelectionPresenter
	{
		private readonly SceneView sv;

		public PointObjectsSelectionPresenter(SceneView sceneView)
		{
			sv = sceneView;
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

			var selectedPointObjects = Document.Current.SelectedNodes().Editable().OfType<PointObject>().ToList();
			foreach (var po in selectedPointObjects) {
				DrawPointObject(po.TransformedPosition * t);
			}
			if (selectedPointObjects.Count == 0)
				return;
			var hull = sv.Components.Get<PointObjectSelectionComponent>()?.СurrentBounds ?? Utils.CalcAABB(selectedPointObjects, true);
			var cornerOffset = PointObjectSelectionComponent.cornerOffset;
			var size = Document.Current.Container.AsWidget.Size;
			var corners = new Quadrangle();

			for (int i = 0; i < 4; i++) {
				hull[i] = hull[i] * size * t;
				corners[i] = Corners[i] * size * t;
			}

			var bounds = new Quadrangle();
			for (int i = 0; i < 4; i++) {
				var next = (i + 1) % 4;
				var prev = (i + 3) % 4;
				var dir1 = hull[i] - hull[next];
				var dir2 = hull[i] - hull[prev];
				if (dir1 + dir2 == Vector2.Zero) {
					dir1 = corners[i] - corners[next];
					dir2 = corners[i] - corners[prev];
				}
				bounds[i] = hull[i] + (dir1.Normalized + dir2.Normalized) * cornerOffset;
			}

			Renderer.DrawQuadrangleOutline(bounds, ColorTheme.Current.SceneView.Selection);
			var hullSize = (hull[0] - hull[2]);
			if (selectedPointObjects.Count() > 1) {
				for (int i = 0; i < 4; i++) {
					var a = bounds[i];
					var b = bounds[(i + 1) % 4];
					if (hullSize.X != 0 && hullSize.Y != 0) {
						DrawStretchMark(a);
					}
					if (hullSize.X == 0 && i % 2 == 1 || hullSize.Y == 0 && i % 2 == 0) {
						continue;
					}
					DrawStretchMark((a + b) / 2);
				}

				DrawStretchMark((bounds.V1 + bounds.V3) / 2);
			}
		}

		readonly List<Vector2> Corners = new List<Vector2>{
			Vector2.Zero,
			Vector2.Right,
			Vector2.One,
			Vector2.Down
		};

		void DrawPointObject(Vector2 position)
		{
			Renderer.DrawRound(position, 4, 10, ColorTheme.Current.SceneView.Selection);
		}

		void DrawStretchMark(Vector2 position)
		{
			Renderer.DrawRect(position - Vector2.One * 3, position + Vector2.One * 3, ColorTheme.Current.SceneView.Selection);
		}
	}
}