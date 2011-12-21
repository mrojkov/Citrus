namespace Lime
{
	public class Dialog : Frame
	{
		public override void Update (int delta)
		{
			var rootFrame = RootFrame.Instance;
			if (worldShown) {
				if (rootFrame.ActiveDialog == null) {
					rootFrame.ActiveDialog = this;
				}
				if (rootFrame.ActiveDialog == this) {
					if (rootFrame.ActiveWidget != null && !rootFrame.ActiveWidget.ChildOf (this)) {
						rootFrame.ActiveWidget = null;
					}
					base.Update (delta);
					Input.ConsumeAllKeyEvents (true);
					if (rootFrame.ActiveWidget == null) {
						rootFrame.ActiveWidget = this;
					}
				} else {
					base.Update (delta);
				}
			}
		}
	}
}
