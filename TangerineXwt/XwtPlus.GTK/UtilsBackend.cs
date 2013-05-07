using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XwtPlus.GtkBackend
{
	public class UtilsBackend : IUtilsBackend
	{
		public void InitializeBackend(object frontend, Xwt.Backends.ApplicationContext context)
		{
		}

		private Gdk.ModifierType prevPointerModifiers;

		public Xwt.Point GetPointerPosition()
		{
			int x, y;
			Gdk.ModifierType m;
			Gdk.Display.Default.GetPointer(out x, out y, out m);
			// Gdk.Display.Default.DefaultScreen.RootWindow.GetPointer(out x, out y, out m);
			return new Xwt.Point(x, y);
		}

		private Gdk.ModifierType prevPointerButtonsState;

		public bool GetPointerButtonState(Xwt.PointerButton button)
		{
			int x, y;
			Gdk.ModifierType pointerModifiers;
			Gdk.Display.Default.GetPointer(out x, out y, out pointerModifiers);
			// WTF? sometimes m contains this magic value on Mac
			if ((int)pointerModifiers == 8192) {
				return true;
				//pointerModifiers = prevPointerModifiers;
			}// else {
			//	prevPointerModifiers = pointerModifiers;
			//}
			if (button == Xwt.PointerButton.Left) {
				return (pointerModifiers & Gdk.ModifierType.Button1Mask) != 0;
			} else {
				throw new NotImplementedException();
			}
		}

		void Xwt.Backends.IBackend.EnableEvent(object eventId)
		{
		}

		void Xwt.Backends.IBackend.DisableEvent(object eventId)
		{
		}

		public void CaptureMouse(Xwt.Widget widget)
		{
			Gtk.Widget w = widget.Surface.NativeWidget as Gtk.Widget;
			//Gdk.Display.Default.PointerIsGrabbed
			//w.GdkWindow.Raise();
			var status = Gdk.Pointer.Grab(w.GdkWindow, false, w.Events,
				w.GdkWindow, new Gdk.Cursor(Gdk.CursorType.Circle), 0);
			//	//Gdk.EventMask.PointerMotionMask | 
			//	//Gdk.EventMask.PointerMotionHintMask | Gdk.EventMask.ButtonPressMask | 
			//	//Gdk.EventMask.ButtonReleaseMask | Gdk.EventMask.ScrollMask | 
			//	//Gdk.EventMask.LeaveNotifyMask | Gdk.EventMask.EnterNotifyMask,
			//	//null, null, 0);
			//Console.WriteLine(status);
		}

		public void ReleaseMouse()
		{
			Gdk.Pointer.Ungrab(0);
		}
	}
}
