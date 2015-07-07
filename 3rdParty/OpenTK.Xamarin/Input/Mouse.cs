#if MAC
using System;

namespace OpenTK.Input
{
	public class Mouse
	{
		public event EventHandler<MouseButtonEventArgs> ButtonDown;
		public event EventHandler<MouseButtonEventArgs> ButtonUp;
		public event EventHandler<MouseMoveEventArgs> Move;
		public event EventHandler<MouseWheelEventArgs> WheelChanged;

		internal void OnButtonUp(MouseButtonEventArgs e)
		{
			if (ButtonUp != null) {
				ButtonUp(this, e);
			}
		}
		
		internal void OnButtonDown(MouseButtonEventArgs e)
		{
			if (ButtonDown != null) {
				ButtonDown(this, e);
			}
		}
		
		internal void OnMove(MouseMoveEventArgs e)
		{
			if (Move != null) {
				Move(this, e);
			}
		}	
	}
}
#endif