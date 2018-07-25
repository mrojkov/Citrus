using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Tangerine.UI.SceneView
{
	class PointObjectsPresenter
	{
		private readonly SceneView sv;
		public static readonly float CornerOffset = 15f;

		public PointObjectsPresenter(SceneView sceneView)
		{
			sv = sceneView;
			sceneView.Frame.CompoundPostPresenter.Add(new DelegatePresenter<Widget>(Render));
		}

		void Render(Widget canvas)
		{
			if (
				Document.Current.ExpositionMode ||
				Document.Current.PreviewAnimation ||
				!Document.Current.Container.Nodes.Any(i => i is PointObject)
			) {
				return;
			}
			canvas.PrepareRendererState();
			var t = Document.Current.Container.AsWidget.CalcTransitionToSpaceOf(canvas);

			var selectedPointObjects = Document.Current.SelectedNodes().Editable().OfType<PointObject>().ToList();
			var pointObjects = Document.Current.Container.Nodes.OfType<PointObject>().Except(selectedPointObjects).ToList();
			foreach (var po in pointObjects) {
				DrawPointObject(po.TransformedPosition * t, ColorTheme.Current.SceneView.PointObject);
			}
			foreach (var po in selectedPointObjects) {
				DrawPointObject(po.TransformedPosition * t, ColorTheme.Current.SceneView.Selection);
			}
			if (selectedPointObjects.Count == 0)
				return;

			var bounds = CalcExpandedHullInSpaceOf(selectedPointObjects, canvas);
			Renderer.DrawQuadrangleOutline(bounds, ColorTheme.Current.SceneView.Selection);
			var hullSize = (bounds[0] - bounds[2]);
			if (selectedPointObjects.Count() > 1) {
				for (var i = 0; i < 4; i++) {
					var a = bounds[i];
					var b = bounds[(i + 1) % 4];
					if (Mathf.Abs(hullSize.X) > Mathf.ZeroTolerance && Mathf.Abs(hullSize.Y) > Mathf.ZeroTolerance) {
						DrawStretchMark(a);
					}
					if (Mathf.Abs(hullSize.X) < Mathf.ZeroTolerance && i % 2 == 1 ||
					    Mathf.Abs(hullSize.Y) < Mathf.ZeroTolerance && i % 2 == 0
					) {
						continue;
					}
					DrawStretchMark((a + b) / 2);
				}

				DrawStretchMark((bounds.V1 + bounds.V3) / 2);
			}
		}

		public static Quadrangle CalcExpandedHullInSpaceOf(IEnumerable<PointObject> points, Widget destWidget)
		{
			Rectangle aabb;
			Utils.CalcAABB(points, Document.Current.Container.AsWidget, out aabb);
			return ExpandAndTranslateToSpaceOf(aabb.ToQuadrangle(), Document.Current.Container.AsWidget, destWidget);
		}

		public static Quadrangle ExpandAndTranslateToSpaceOf(Quadrangle hull, Widget sourceWidget, Widget destWidget)
		{
			var t = sourceWidget.CalcTransitionToSpaceOf(destWidget);
			var size = sourceWidget.Size;
			var corners = new Quadrangle();
			for (var i = 0; i < 4; i++) {
				corners[i] = Corners[i] * size * t;
				hull[i] *= t;
			}
			var bounds = new Quadrangle();
			for (var i = 0; i < 4; i++) {
				var next = (i + 1) % 4;
				var prev = (i + 3) % 4;
				var dir1 = hull[i] - hull[next];
				var dir2 = hull[i] - hull[prev];
				if (dir1 + dir2 == Vector2.Zero) {
					dir1 = corners[i] - corners[next];
					dir2 = corners[i] - corners[prev];
				}
				bounds[i] = hull[i] + (dir1.Normalized + dir2.Normalized) * CornerOffset;
			}

			return bounds;
		}

		public static readonly List<Vector2> Corners = new List<Vector2>{
			Vector2.Zero,
			Vector2.Right,
			Vector2.One,
			Vector2.Down
		};

		void DrawPointObject(Vector2 position, Color4 color)
		{
			Renderer.DrawRound(position, 6, 10, ColorTheme.Current.SceneView.SplineOutline);
			Renderer.DrawRound(position, 4, 10, color);
		}

		void DrawStretchMark(Vector2 position)
		{
			Renderer.DrawRect(position - Vector2.One * 3, position + Vector2.One * 3, ColorTheme.Current.SceneView.Selection);
		}
	}
}
