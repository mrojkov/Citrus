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
