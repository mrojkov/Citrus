using System.Collections.Generic;

namespace Lime
{
	public class GestureManager
	{
		protected readonly WidgetContext context;
		private readonly List<Gesture> activeGestures = new List<Gesture>();
		private Node activeNode;
		public int CurrentIteration { get; private set; }

		public GestureManager(WidgetContext context)
		{
			this.context = context;
		}

		public void Process()
		{
			CurrentIteration++;
			if (!(activeNode as Widget)?.GloballyVisible ?? false) {
				activeNode = null;
				CancelGestures();
			}
			foreach (var r in activeGestures) {
				r.Update(activeGestures);
			}
			if (context.NodeCapturedByMouse != activeNode) {
				activeNode = context.NodeCapturedByMouse;
				CancelGestures();
				if (activeNode != null) {
					activeGestures.AddRange(EnumerateGestures(activeNode));
					foreach (var r in activeGestures) {
						r.Update(activeGestures);
					}
				}
			}
		}

		private void CancelGestures()
		{
			foreach (var r in activeGestures) {
				r.Cancel();
			}
			activeGestures.Clear();
		}

		protected virtual IEnumerable<Gesture> EnumerateGestures(Node node)
		{
			var noMoreClicks = new bool[3];
			var noMoreDoubleClicks = new bool[3];
			for (; node != null; node = node.Parent) {
				if (node.HasGestures()) {
					foreach (var g in node.Gestures) {
						if (g is ClickGesture cg) {
							if (noMoreClicks[cg.ButtonIndex]) {
								continue;
							}
							noMoreClicks[cg.ButtonIndex] = true;
						}
						if (g is DoubleClickGesture dcg) {
							if (noMoreDoubleClicks[dcg.ButtonIndex]) {
								continue;
							}
							noMoreDoubleClicks[dcg.ButtonIndex] = true;
						}
						yield return g;
					}
				}
			}
		}
	}
}
