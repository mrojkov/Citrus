using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public class GestureRecognizerManager
	{
		private readonly WidgetContext context;
		private readonly List<GestureRecognizer> activeRecognizers = new List<GestureRecognizer>();
		private Node activeNode;
		public int CurrentIteration { get; private set; }

		public GestureRecognizerManager(WidgetContext context)
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
			foreach (var r in activeRecognizers) {
				r.Update(activeRecognizers);
			}
			if (context.NodeMousePressedOn != activeNode) {
				activeNode = context.NodeMousePressedOn;
				CancelGestures();
				if (activeNode != null) {
					activeRecognizers.AddRange(EnumerateRecognizers(activeNode));
					foreach (var r in activeRecognizers) {
						r.Update(activeRecognizers);
					}
				}
			}
		}

		private void CancelGestures()
		{
			foreach (var r in activeRecognizers) {
				r.Cancel();
			}
			activeRecognizers.Clear();
		}

		private IEnumerable<GestureRecognizer> EnumerateRecognizers(Node node)
		{
			var noClickRecognizersAnymore = false;
			while (node != null) {
				if (node.HasGestureRecognizers()) {
					var anyClickRecognizer = false;
					foreach (var r in node.GestureRecognizers) {
						bool isWrongRecognizer = r is ClickRecognizer || r is DoubleClickRecognizer;
						if (isWrongRecognizer && noClickRecognizersAnymore) {
							continue;
						}
						anyClickRecognizer |= isWrongRecognizer;
						yield return r;
					}
					noClickRecognizersAnymore |= anyClickRecognizer;
				}
				node = node.Parent;
			}
		}
	}
}
