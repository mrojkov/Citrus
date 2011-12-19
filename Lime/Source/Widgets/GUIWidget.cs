using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;

namespace Lime
{
	public abstract class GUIWidget : Frame
	{
		/// <summary>
		/// Widget which is in focus (or highlighted). One of usage scenarios is a mouse capturing.
		/// Before processing mouse down event you should test whether FocusedWidget == this.
		/// </summary>
		public static GUIWidget FocusedWidget;

		/// <summary>
		/// Resets GUI widget animation to its initial state.
		/// </summary>
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
