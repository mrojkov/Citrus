using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.SceneView
{
	public class ContainerAreaPresenter
	{
		public ContainerAreaPresenter(SceneView sceneView)
		{
			const float inf = 1000000f;
			sceneView.Scene.CompoundPresenter.Push(new DelegatePresenter<Widget>(w => {
				var ctr = Core.Document.Current.Container as Widget;
				if (ctr != null) {
					ctr.PrepareRendererState();
					if (Core.Document.Current.PreviewAnimation) {
						Renderer.DrawRect(new Vector2(-inf, -inf), new Vector2(inf, inf), Color4.Black);
					} else {
						var c = ColorTheme.Current.SceneView.ContainerOuterSpace;
						Renderer.DrawRect(new Vector2(-inf, -inf), new Vector2(inf, 0), c);
						Renderer.DrawRect(new Vector2(-inf, ctr.Height), new Vector2(inf, inf), c);
						Renderer.DrawRect(new Vector2(-inf, 0), new Vector2(0, ctr.Height), c);
						Renderer.DrawRect(new Vector2(ctr.Width, 0), new Vector2(inf, ctr.Height), c);
						Renderer.DrawRect(Vector2.Zero, ctr.Size, ColorTheme.Current.SceneView.ContainerInnerSpace);
					}
				}
			}));

			const float deviceHeight = 768;
			float[] deviceWidths = { 1366, 1152, 1024, 1579 };
			sceneView.Scene.CompoundPostPresenter.Push(
				new DelegatePresenter<Widget>(
					(w) => {
						var root = Core.Document.Current.RootNode as Widget;
						if (root != null && Core.UserPreferences.Instance.Get<UserPreferences>().ShowOverlays) {
							root.PrepareRendererState();
							var mtx = root.LocalToWorldTransform;
							var t1 = 1 / mtx.U.Length;
							Renderer.Transform1 = mtx;
							var rootCenter = root.Size * 0.5f;
							foreach (var width in deviceWidths) {
								SetAndRenderOverlay(width, deviceHeight, rootCenter, t1);
							}
						}
					}));

			sceneView.Scene.CompoundPostPresenter.Push(new DelegatePresenter<Widget>(w => {
				var ctr = Core.Document.Current.Container as Widget;
				if (ctr != null && !Core.Document.Current.PreviewAnimation) {
					ctr.PrepareRendererState();
					var c = ColorTheme.Current.SceneView.ContainerBorder;
					var mtx = ctr.LocalToWorldTransform;
					var t1 = 1 / mtx.U.Length;
					var t2 = 1 / mtx.V.Length;
					Renderer.Transform1 = mtx;
					Renderer.DrawLine(new Vector2(0, -inf), new Vector2(0, inf), c, t1);
					Renderer.DrawLine(new Vector2(ctr.Width, -inf), new Vector2(ctr.Width, inf), c, t1);
					Renderer.DrawLine(new Vector2(-inf, 0), new Vector2(inf, 0), c, t2);
					Renderer.DrawLine(new Vector2(-inf, ctr.Height), new Vector2(inf, ctr.Height), c, t2);
				}
			}));
		}

		private static void SetAndRenderOverlay(float width, float height, Vector2 rootCenter, float thickness)
		{
			var a1 = new Vector2(width, height) * 0.5f + rootCenter;
			var b1 = new Vector2(width, height) * -0.5f + rootCenter;
			var a2 = new Vector2(height, width) * 0.5f + rootCenter;
			var b2 = new Vector2(height, width) * -0.5f + rootCenter;
			Renderer.DrawRectOutline(a1, b1, Color4.White, thickness);
			Renderer.DrawRectOutline(a2, b2, Color4.White, thickness);
		}
	}
}
