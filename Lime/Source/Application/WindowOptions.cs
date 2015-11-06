using System;

namespace Lime
{
	public class WindowOptions
	{
		public bool FullScreen = false;
		public bool FixedSize = true;
		public Size Size = new Size(800, 600);
		public string Title = "Citrus";
		public bool Visible = true;
		// System.Drawing.Icon on Windows
		public object Icon;
	}
}

