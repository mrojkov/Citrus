using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public class GestureRecognizerManager
	{
		private Input input;
		private WidgetContext context;
		private readonly List<GestureRecognizer> activeRecognizers = new List<GestureRecognizer>();
		private Node activeNode;
		public int CurrentIteration { get; private set; }

		public GestureRecognizerManager(WidgetContext context, Input input)
		{
			this.input = input;
			this.context = context;
		}

		public void Process()
		{
			CurrentIteration++;
			if (context.NodeMousePressedOn != null && context.NodeMousePressedOn != activeNode) {
				activeNode = context.NodeMousePressedOn;
				CancelRecognizers();
				activeRecognizers.AddRange(EnumerateRecognizers(activeNode));
			}
			if (!(activeNode as Widget)?.GloballyVisible ?? false) {
				activeNode = null;
				CancelRecognizers();
			}
			foreach (var r in activeRecognizers) {
				r.Update(activeRecognizers);
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

		private void CancelRecognizers()
		{
			foreach (var r in activeRecognizers) {
				r.Cancel();
			}
			activeRecognizers.Clear();
		}
	}
}
