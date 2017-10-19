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
				foreach (var r in activeRecognizers) {
					r.Cancel();
				}
				activeRecognizers.Clear();
			}
			foreach (var r in activeRecognizers) {
				r.Update(activeRecognizers);
			}
			if (context.NodeMousePressedOn != activeNode) {
				activeNode = context.NodeMousePressedOn;
				activeRecognizers.Clear();
				if (activeNode != null) {
					activeRecognizers.AddRange(EnumerateRecognizers(activeNode));
					foreach (var r in activeRecognizers) {
						r.Update(activeRecognizers);
					}
				}
			}
		}

		private IEnumerable<GestureRecognizer> EnumerateRecognizers(Node node)
		{
			var noClickRecognizersAnymore = false;
			while (node != null) {
				if (node.HasGestureRecognizers()) {
					var anyClickRecognizer = false;
					foreach (var r in node.GestureRecognizers) {
						if (r is MulticlickRecognizer && noClickRecognizersAnymore) {
							continue;
						}
						anyClickRecognizer |= r is MulticlickRecognizer;
						yield return r;
					}
					noClickRecognizersAnymore |= anyClickRecognizer;
				}
				node = node.Parent;
			}
		}
	}
}
