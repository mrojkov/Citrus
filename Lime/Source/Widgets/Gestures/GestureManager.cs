using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public class GestureManager
	{
		private readonly WidgetContext context;
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
			var noClickGesturesAnymore = false;
			while (node != null) {
				if (node.HasGestures()) {
					var anyClickGesture = false;
					foreach (var r in node.Gestures) {
						bool isWrongGesture = r is ClickGesture || r is DoubleClickGesture;
						if (isWrongGesture && noClickGesturesAnymore) {
							continue;
						}
						anyClickGesture |= isWrongGesture;
						yield return r;
					}
					noClickGesturesAnymore |= anyClickGesture;
				}
				node = node.Parent;
			}
		}
	}
}
