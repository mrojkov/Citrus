#if MAC
using System;

namespace OpenTK.Input
{
	public class Keyboard
	{
		public event EventHandler<KeyboardKeyEventArgs> KeyDown;
		public event EventHandler<KeyboardKeyEventArgs> KeyUp;
	}
}
#endif
