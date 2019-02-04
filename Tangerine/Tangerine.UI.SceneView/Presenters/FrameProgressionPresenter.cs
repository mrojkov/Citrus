using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView.Presenters
{
	class FrameProgressionPresenter
	{
		private SceneView sceneView;

		public FrameProgressionPresenter(SceneView sceneView)
		{
			this.sceneView = sceneView;
			sceneView.Scene.CompoundPresenter.Push(new SyncDelegatePresenter<Widget>(Render));
		}

		public void Render(Widget canvas)
		{
			if (
				Document.Current.PreviewAnimation ||
				Document.Current.ExpositionMode || !CoreUserPreferences.Instance.ShowFrameProgression

			) {
				return;
			}

			sceneView.Frame.PrepareRendererState();
			var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>().ToList();
			if (widgets.Count == 0) {
				return;
			}
			int savedAnimationFrame = Document.Current.AnimationFrame;
			foreach (var widget in widgets) {
				if (!(widget is IAnimationHost host)) {
					continue;
				}

				int max = 0, min = 0;
				foreach (var animator in host.Animators) {
					if (animator.AnimationId != Document.Current.AnimationId) {
						continue;
					}
					foreach (var k in animator.ReadonlyKeys) {
						max = max.Max(k.Frame);
						min = min.Min(k.Frame);
					}
				}
				if (max == min) {
					continue;
				}
				for (int i = min; i <= max; i++) {
					foreach (var animator in host.Animators) {
						if (animator.AnimationId == Document.Current.AnimationId) {
							animator.Apply(AnimationUtils.FramesToSeconds(i));
						}
					}
					var color = Color4.Lerp(
						((float)i - min) / (max - min),
						ColorTheme.Current.SceneView.FrameProgressionBeginColor,
						ColorTheme.Current.SceneView.FrameProgressionEndColor
					);
					var hull = widget.CalcHullInSpaceOf(sceneView.Frame);
					for (int j = 0; j < 4; j++) {
						var a = hull[j];
						var b = hull[(j + 1) % 4];
						Renderer.DrawLine(a, b, color);
					}
				}
				foreach (var animator in host.Animators) {
					if (animator.AnimationId == Document.Current.AnimationId) {
						animator.Apply(AnimationUtils.FramesToSeconds(savedAnimationFrame));
					}
				}
			}
		}
	}
}
