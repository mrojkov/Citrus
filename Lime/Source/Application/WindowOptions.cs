using System;

namespace Lime
{
	public class WindowOptions
	{
		public bool FullScreen = false;
		public bool FixedSize = true;
		public Size ClientSize = new Size(800, 600);
		public Size MinimumDecoratedSize;
		public Size MaximumDecoratedSize;
		public string Title = "Citrus";
		public bool Visible = true;
		// System.Drawing.Icon on Windows
		public object Icon;
	}
}

