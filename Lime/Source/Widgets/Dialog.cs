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
			base.Update(delta);
			if (worldShown) {
				// Cosume all input events and drive mouse into the hole.
				Input.ConsumeAllKeyEvents(true);
				Input.MousePosition = MouseRefuge;
			}
		}
	}
}
