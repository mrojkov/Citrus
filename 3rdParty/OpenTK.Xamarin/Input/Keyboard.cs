#if MAC
using System;

namespace OpenTK.Input
{
	public class Keyboard
	{
		public event EventHandler<KeyboardKeyEventArgs> KeyDown;
		public event EventHandler<KeyboardKeyEventArgs> KeyUp;
		public event EventHandler<KeyPressEventArgs> KeyPress;

		internal void OnKeyDown(KeyboardKeyEventArgs e)
		{
			if (KeyDown != null) {
				KeyDown(this, e);
			}
		}

		internal void OnKeyUp(KeyboardKeyEventArgs e)
		{
			if (KeyUp != null) {
				KeyUp(this, e);
			}
		}

		internal void OnKeyPress(KeyPressEventArgs e)
		{
			if (KeyPress != null) {
				KeyPress(this, e);
			}
		}
	}
}
#endif
