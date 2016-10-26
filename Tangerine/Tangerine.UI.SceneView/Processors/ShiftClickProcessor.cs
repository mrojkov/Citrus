using System;
using System.Linq;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.SceneView
{
	public class ShiftClickProcessor : Core.ITaskProvider
	{
		public IEnumerator<object> Task()
		{
			var sv = SceneView.Instance;
			while (true) {
				if (sv.Input.WasMouseReleased() && sv.Input.IsKeyPressed(Key.Shift)) {
					sv.Input.ConsumeKey(Key.Mouse0);
					var ctr = (Widget)Core.Document.Current.Container;
					if (ctr.CalcHullInSpaceOf(sv.Scene).Contains(sv.MousePosition)) {
						foreach (var widget in ctr.Nodes.Editable().OfType<Widget>()) {
							if (widget.CalcHullInSpaceOf(sv.Scene).Contains(sv.MousePosition)) {
								Core.Operations.EnterNode.Perform(widget);
								break;
							}
						}
					} else {
						Core.Operations.LeaveNode.Perform();
					}
				}
				yield return null;
			}
		}
	}
}
