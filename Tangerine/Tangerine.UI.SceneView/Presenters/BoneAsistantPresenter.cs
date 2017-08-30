using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class BoneAsistantPresenter
	{
		private readonly SceneView sv;

		public BoneAsistantPresenter(SceneView sceneView)
		{
			sv = sceneView;
			sceneView.Frame.CompoundPostPresenter.Add(new DelegatePresenter<Widget>(Render));
		}

		void Render(Widget canvas)
		{
			var ctr = Document.Current.Container as Widget;
			if (ctr != null) {
				ctr.PrepareRendererState();
				var mtx = ctr.LocalToWorldTransform;
				Renderer.Transform1 = mtx;
				var t1 = 1 / mtx.U.Length;
				var t2 = 1 / mtx.V.Length;
				Renderer.Flush();
				if (!Document.Current.PreviewAnimation) {
					var helper = SceneView.Instance.Scene.Components.Get<CreateBoneHelper>();
					if (helper != null && helper.HitTip != default(Vector2)) {
						var size = 15 / SceneView.Instance.Scene.Scale.X;
						var lt = helper.HitTip - Vector2.One * size;
						var rb = helper.HitTip + Vector2.One * size;
						var rt = helper.HitTip + new Vector2(-size, size);
						var lb = helper.HitTip + new Vector2(size, -size);
						Renderer.DrawLine(lt, rt, Color4.Green, t1);
						Renderer.DrawLine(rt, rb, Color4.Green, t2);
						Renderer.DrawLine(rb, lb, Color4.Green, t1);
						Renderer.DrawLine(lb, lt, Color4.Green, t2);
					}
				}
			}
		}
	}
}
