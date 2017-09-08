using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using System.Threading;

namespace Tangerine.UI
{
	public static class UIProcessors
	{
		public static IEnumerator<object> PickColorProcessor(Property<Color4> prop)
		{
			var root = WidgetContext.Current.Root;
			root.Input.CaptureMouse();
			Document.Current.History.BeginTransaction();
			yield return null;
			while (true) {
				Utils.ChangeCursorIfDefault(Cursors.Pipette);
				if (root.Input.IsMousePressed()) {
					prop.Value = ColorPicker.PickAtCursor();
				} else if (root.Input.WasMouseReleased()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Default);
					root.Input.ReleaseMouse();
					Document.Current.History.EndTransaction();
					yield break;
				}
				yield return null;
			}
		}
	}
}