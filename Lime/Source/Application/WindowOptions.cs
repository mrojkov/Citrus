
namespace Lime
{
	public enum WindowStyle
	{
		Regular,
		Dialog,
		Borderless
	}

	public enum MacWindowTabbingMode
	{
		Automatic,
		Preferred,
		Disallowed
	}

	public class WindowOptions
	{
		public int? Screen = null;
		public bool FullScreen = false;
		public bool FixedSize = true;
		public bool Centered = true;
		public WindowStyle Style = WindowStyle.Regular;
		public MacWindowTabbingMode MacWindowTabbingMode = MacWindowTabbingMode.Automatic;
		public Vector2 ClientSize = new Vector2(800, 600);
		public Vector2 MinimumDecoratedSize;
		public Vector2 MaximumDecoratedSize;
		public string Title = "Citrus";
		public bool Visible = true;
		public bool VSync = true;
		public bool UseTimer = true;

		// System.Drawing.Icon on Windows
		public object Icon;
		public bool ToolWindow = false;
	}
}

