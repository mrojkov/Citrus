using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;

namespace Lime
{
	public abstract class GUIWidget : Frame
	{
		public static GUIWidget FocusedWidget;

		protected abstract void Reset ();

		bool firstUpdate = true;

		public override void Update (int delta)
		{
			if (firstUpdate) {
				firstUpdate = false;
				Reset ();
			}
			base.Update (delta);
		}

		public static void ResetFocus ()
		{
			if (FocusedWidget != null) {
				FocusedWidget.Reset ();
				FocusedWidget = null;
			}
		}
	}
}
