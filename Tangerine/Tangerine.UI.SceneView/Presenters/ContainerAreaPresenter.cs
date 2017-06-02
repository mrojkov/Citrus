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
			const float deviceWidth1 = 1366;
			const float deviceWidth2 = 1152;
			const float deviceWidth3 = 1024;
			sceneView.Scene.CompoundPostPresenter.Push(
				new DelegatePresenter<Widget>(
					(w) => {
						var root = Core.Document.Current.RootNode as Widget;
						if (root != null) {
							root.PrepareRendererState();
							var mtx = root.LocalToWorldTransform;
							var t1 = 1 / mtx.U.Length;
							var t2 = 1 / mtx.V.Length;
							Renderer.Transform1 = mtx;
							var ctrCenter = root.Size * 0.5f;
							var a1 = new Vector2(deviceWidth1, deviceHeight) * 0.5f + ctrCenter;
							var b1 = new Vector2(deviceWidth1, deviceHeight) * -0.5f + ctrCenter;
							var a2 = new Vector2(deviceHeight, deviceWidth1) * 0.5f + ctrCenter;
							var b2 = new Vector2(deviceHeight, deviceWidth1) * -0.5f + ctrCenter;
							var a3 = new Vector2(deviceWidth2, deviceHeight) * 0.5f + ctrCenter;
							var b3 = new Vector2(deviceWidth2, deviceHeight) * -0.5f + ctrCenter;
							var a4 = new Vector2(deviceHeight, deviceWidth2) * 0.5f + ctrCenter;
							var b4 = new Vector2(deviceHeight, deviceWidth2) * -0.5f + ctrCenter;
							var a5 = new Vector2(deviceWidth3, deviceHeight) * 0.5f + ctrCenter;
							var b5 = new Vector2(deviceWidth3, deviceHeight) * -0.5f + ctrCenter;
							var a6 = new Vector2(deviceHeight, deviceWidth3) * 0.5f + ctrCenter;
							var b6 = new Vector2(deviceHeight, deviceWidth3) * -0.5f + ctrCenter;
							Renderer.DrawRectOutline(a1, b1, Color4.White, t1);
							Renderer.DrawRectOutline(a2, b2, Color4.White, t1);
							Renderer.DrawRectOutline(a3, b3, Color4.White, t1);
							Renderer.DrawRectOutline(a4, b4, Color4.White, t1);
							Renderer.DrawRectOutline(a5, b5, Color4.White, t1);
							Renderer.DrawRectOutline(a6, b6, Color4.White, t1);
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
	}
}
