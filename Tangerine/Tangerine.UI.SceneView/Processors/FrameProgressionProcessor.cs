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
		private int widgetsHashCode;
		private readonly List<IPresenter> presenters = new List<IPresenter>();
		private readonly Dictionary<Widget, (List<Quadrangle> Hulls, int HashCode)> cache = new Dictionary<Widget, (List<Quadrangle> Hulls, int HashCode)>();

		public void RenderWidgetsFrameProgression(Widget widget)
		{
			sceneView.Frame.PrepareRendererState();
			int savedAnimationFrame = Document.Current.AnimationFrame;
			if (!(widget is IAnimationHost host)) {
				return;
			}
			int max = 0, min = 0, hash = 0;
			foreach (var animator in host.Animators) {
				hash ^= animator.Version;
				if (animator.AnimationId != Document.Current.AnimationId) {
					continue;
				}
				if (animator.ReadonlyKeys.Count > 0) {
					min = min.Min(animator.ReadonlyKeys[0].Frame);
					max = max.Max(animator.ReadonlyKeys[animator.ReadonlyKeys.Count - 1].Frame);
				}
			}
			if (max == min) {
				return;
			}
			hash ^= widget.Position.GetHashCode();
			hash ^= widget.Rotation.GetHashCode();
			hash ^= widget.Scale.GetHashCode();
			hash ^= widget.Pivot.GetHashCode();
			hash ^= widget.Size.GetHashCode();
			hash ^= (Document.Current.AnimationId ?? "").GetHashCode();
			hash ^= SceneView.Instance.Scene.Scale.X.GetHashCode();
			hash ^= SceneView.Instance.Scene.Position.GetHashCode();
			if (cache.TryGetValue(widget, out var cacheEntry) && hash == cacheEntry.HashCode) {
				DrawHulls(cacheEntry.Hulls, min, max);
				return;
			}
			var hulls = cacheEntry.Hulls ?? new List<Quadrangle>();
			hulls.Clear();
			for (int i = min; i <= max; i++) {
				foreach (var animator in host.Animators) {
					if (animator.AnimationId == Document.Current.AnimationId) {
						animator.Apply(AnimationUtils.FramesToSeconds(i));
					}
				}
				hulls.Add(widget.CalcHullInSpaceOf(sceneView.Frame));
			}
			cache[widget] = (Hulls: hulls, HashCode: hash);
			foreach (var animator in host.Animators) {
				if (animator.AnimationId == Document.Current.AnimationId) {
					animator.Apply(AnimationUtils.FramesToSeconds(savedAnimationFrame));
				}
			}
			DrawHulls(hulls, min, max);
		}

		private static void DrawHulls(List<Quadrangle> hulls, int min, int max)
		{
			for (int i = 0; i < hulls.Count; i++) {
				var hull = hulls[i];
				var color = Color4.Lerp(
					((float)i - min) / (max - min),
					ColorTheme.Current.SceneView.FrameProgressionBeginColor,
					ColorTheme.Current.SceneView.FrameProgressionEndColor
				);
				for (int j = 0; j < 4; j++) {
					var a = hull[j];
					var b = hull[(j + 1) % 4];
					Renderer.DrawLine(a, b, color);
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

		private void ClearCache()
		{
			presenters.Clear();
			widgets.Clear();
			cache.Clear();
			widgetsHashCode = 0;
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
						ClearCache();
					}
					continue;
				}
				var selectedWidgets = Document.Current.SelectedNodes().Editable().OfType<Widget>().ToList();
				var selectedWidgetsHashCode = CalcHashCode(selectedWidgets);
				if (widgetsHashCode == selectedWidgetsHashCode) {
					continue;
				}
				for (int i = 0; i < widgets.Count; i++) {
					widgets[i].CompoundPresenter.Remove(presenters[i]);
				}
				ClearCache();
				widgets = selectedWidgets;
				widgetsHashCode = selectedWidgetsHashCode;
				foreach (var widget in widgets) {
					var sdp = new SyncDelegatePresenter<Widget>(RenderWidgetsFrameProgression);
					widget.CompoundPresenter.Add(sdp);
					presenters.Add(sdp);
				}
			}
		}
	}
}
