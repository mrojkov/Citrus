using Lime;
using Tangerine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.UI.SceneView
{
	public class AnimationPathPresenter: CustomPresenter
	{
		public override void Render(Node node)
		{
			if (CoreUserPreferences.Instance.ShowAnimationPath) {
				return;
			}
			if (node is IAnimable) {
				var animable = node as IAnimable;
				foreach (var animator in animable.Animators) {
					if (animator.TargetProperty == nameof(Widget.Position)) {
						IKeyframeList keys = animator.ReadonlyKeys;
						if (keys.Count == 0) {
							continue;
						}
						SceneView.Instance.Frame.PrepareRendererState();
						var transform = SceneView.Instance.Scene.CalcTransitionToSpaceOf(SceneView.Instance.Frame);
						var prev = (Vector2)keys[0].Value * transform;
						for (int i = 1; i < keys.Count; ++i) {
							var pos = (Vector2)keys[i].Value * transform;
							Renderer.DrawDashedLine(prev, pos, ColorTheme.Current.SceneView.PointObject, new Vector2(4, 2));
							prev = pos;
						}
					}
				}
			}
		}
	}
}
