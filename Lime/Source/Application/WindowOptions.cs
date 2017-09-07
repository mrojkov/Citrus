
namespace Lime
{
	public enum WindowStyle
	{
		Regular,
		Dialog,
		Borderless
	}

	public class WindowOptions
	{
		public int? Screen = null;
		public bool FullScreen = false;
		public bool FixedSize = true;
		public bool Centered = true;
		public WindowStyle Style = WindowStyle.Regular;
		public Vector2 ClientSize = new Vector2(800, 600);
		public Vector2 MinimumDecoratedSize;
		public Vector2 MaximumDecoratedSize;
		public string Title = "Citrus";
		public bool Visible = true;
		public bool VSync = true;

		// System.Drawing.Icon on Windows
		public object Icon;
	}
}

