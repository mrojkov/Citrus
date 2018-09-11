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
		private const float RectSize = 15;

		public BoneAsistantPresenter(SceneView sceneView)
		{
			sv = sceneView;
			sceneView.Frame.CompoundPostPresenter.Add(new SyncDelegatePresenter<Widget>(Render));
		}

		void Render(Widget canvas)
		{
			var ctr = Document.Current.Container as Widget;
			if (ctr != null) {
				var t = ctr.CalcTransitionToSpaceOf(canvas);
				if (!Document.Current.PreviewAnimation) {
					var helper = SceneView.Instance.Components.Get<CreateBoneHelper>();
					if (helper != null && helper.HitTip != default(Vector2)) {
						var hull = new Rectangle(helper.HitTip * t, helper.HitTip * t)
							.ExpandedBy(new Thickness(RectSize))
							.ToQuadrangle();
						for (int i = 0; i < 4; i++) {
							var a = hull[i];
							var b = hull[(i + 1) % 4];
							Renderer.DrawLine(a, b, Color4.Green);
						}
					}
				}
			}
		}
	}
}
