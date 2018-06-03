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
			var mouseDownPosition = Vector2.Zero;
			while (true) {
				if (sv.Input.WasMousePressed()) {
					mouseDownPosition = sv.Input.MousePosition;	
				}
				if (
					sv.Input.WasMouseReleased() && 
					sv.Input.IsKeyPressed(Key.Shift) && 
					(mouseDownPosition - sv.Input.MousePosition).Length < 5
				) {
					sv.Input.ConsumeKey(Key.Mouse0);
					Core.Document.Current.History.DoTransaction(() => HandleClick(sv));
				}
				yield return null;
			}
		}

		static void HandleClick(SceneView sv)
		{
			var node = Core.Document.Current.SelectedNodes().OfType<Widget>().FirstOrDefault();
			if (node?.CalcHullInSpaceOf(sv.Scene).Contains(sv.MousePosition) ?? false) {
				if (Core.Operations.EnterNode.Perform(node))
					return;
			}
			var ctr = Core.Document.Current.Container as Widget;
			if (ctr == null)
				return;
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
	}
}
