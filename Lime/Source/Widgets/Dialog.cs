namespace Lime
{
	public class Dialog : Frame
	{
		static Vector2 MouseRefuge = new Vector2(-100000, -100000);

		public override void Update(int delta)
		{
			if (worldShown && !Input.MousePosition.Equals(MouseRefuge)) {
				if (RootFrame.Instance.ActiveWidget != null && !RootFrame.Instance.ActiveWidget.ChildOf(this)) {
					// Discard active widget if it's not a child of the topmost dialog.
					RootFrame.Instance.ActiveWidget = null;
				}
			}
			if (worldShown) {
				if (RootFrame.Instance.ActiveTextWidget != null && !RootFrame.Instance.ActiveTextWidget.ChildOf(this)) {
					// Discard active text widget if it's not a child of the topmost dialog.
					RootFrame.Instance.ActiveTextWidget = null;
				}
			}
			base.Update(delta);
			if (worldShown) {
				// Cosume all input events and drive mouse out of the screen.
				Input.ConsumeAllKeyEvents(true);
				Input.MousePosition = MouseRefuge;
				Input.TextInput = null;
			}
		}
	}
}
