using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView.Presenters
{
	class FrameProgressionProcessor: ITaskProvider
	{
		private SceneView sceneView => SceneView.Instance;
		private List<Widget> widgets = new List<Widget>();
		private readonly List<IPresenter> presenters = new List<IPresenter>();
		private int hash;

		public void RenderWidgetsFrameProgression(Widget widget)
		{
			sceneView.Frame.PrepareRendererState();
			int savedAnimationFrame = Document.Current.AnimationFrame;
			if (!(widget is IAnimationHost host)) {
				return;
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
				return;
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

		private static int CalcHashCode(List<Widget> widgets)
		{
			var r = 0;
			foreach (var widget in widgets) {
				r ^= widget.GetHashCode();
			}
			return r;
		}

		public IEnumerator<object> Task()
		{
			while (true) {
				yield return null;
				if (
					Document.Current.PreviewAnimation ||
					Document.Current.ExpositionMode || !CoreUserPreferences.Instance.ShowFrameProgression

				) {
					if (widgets.Count != 0) {
						for (int i = 0; i < widgets.Count; i++) {
							widgets[i].CompoundPresenter.Remove(presenters[i]);
						}
						presenters.Clear();
						widgets.Clear();
						hash = 0;
					}
					continue;
				}
				var selectedWidgets = Document.Current.SelectedNodes().Editable().OfType<Widget>().ToList();
				var selectedWidgetsHash = CalcHashCode(selectedWidgets);
				if (hash == selectedWidgetsHash) {
					continue;
				}
				hash = selectedWidgetsHash;
				for (int i = 0; i < widgets.Count; i++) {
					widgets[i].CompoundPresenter.Remove(presenters[i]);
				}
				presenters.Clear();
				widgets = selectedWidgets;
				foreach (var widget in widgets) {
					var sdp = new SyncDelegatePresenter<Widget>(RenderWidgetsFrameProgression);
					widget.CompoundPresenter.Add(sdp);
					presenters.Add(sdp);
				}
			}
		}
	}
}
