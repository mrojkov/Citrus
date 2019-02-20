using System.Collections.Generic;

namespace Lime
{
	public class GestureManager
	{
		protected readonly WidgetContext context;
		private readonly List<Gesture> activeGestures;
		private Node activeNode;

		public int CurrentIteration { get; private set; }

		public GestureManager(WidgetContext widgetContext)
		{
			context = widgetContext;
			activeGestures = new List<Gesture>();
		}

		public void Update(float delta)
		{
			++CurrentIteration;

			if ((activeNode as Widget)?.GloballyVisible != true) {
				activeNode = null;
				CancelGestures(activeGestures);
			}

			UpdateGestures(delta, activeGestures);
			CleanupGestures(activeGestures);

			if (context.NodeCapturedByMouse != activeNode) {
				activeNode = context.NodeCapturedByMouse;
				CancelGestures(activeGestures);
				if (activeNode != null) {
					activeGestures.Clear();
					activeGestures.AddRange(EnumerateGestures(activeNode));
					UpdateGestures(delta, activeGestures);
				}
			}
		}

		protected virtual IEnumerable<Gesture> EnumerateGestures(Node node)
		{
			for (bool noClickGesturesAnymore = false; node != null; node = node.Parent) {
				if (!node.HasGestures()) {
					continue;
				}
				bool anyClickGesture = false;
				foreach (var gesture in node.Gestures) {
					bool isWrongGesture = gesture is ClickGesture || gesture is DoubleClickGesture;
					if (isWrongGesture && noClickGesturesAnymore) {
						continue;
					}
					anyClickGesture |= isWrongGesture;
					yield return gesture;
				}
				noClickGesturesAnymore |= anyClickGesture;
			}
		}

		private static void UpdateGestures(float delta, List<Gesture> gestures)
		{
			foreach (var gesture in gestures) {
				gesture.Update(delta);
				if (gesture.WasRecognized()) {
					CancelGestures(gestures, sender: gesture);
					break;
				}
			}
		}

		private static void CleanupGestures(List<Gesture> gestures)
		{
			gestures.RemoveAll(gesture => (gesture as SelfEndingGesture)?.WasEnded() == true);
		}

		private static void CancelGestures(List<Gesture> gestures, Gesture sender = null)
		{
			gestures.RemoveAll(gesture => !gesture.Equals(sender) && gesture.Cancel(sender));
		}
	}
}
