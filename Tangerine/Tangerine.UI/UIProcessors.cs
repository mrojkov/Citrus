using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using System.Threading;

namespace Tangerine.UI
{
	public static class UIProcessors
	{
		public static IEnumerator<object> PickColorProcessor(Action<Color4> setter)
		{
			var root = WidgetContext.Current.Root;
			Document.Current.History.BeginTransaction();
			yield return null;
			while (true) {
				Utils.ChangeCursorIfDefault(Cursors.Pipette);
				if (root.Input.IsMousePressed()) {
					setter(ColorPicker.PickAtCursor());
				} else if (root.Input.WasMouseReleased()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Default);
					Document.Current.History.EndTransaction();
					yield break;
				}
				yield return null;
			}
		}
	}
}