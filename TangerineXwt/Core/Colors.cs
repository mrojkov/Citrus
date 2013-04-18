using System;
using System.Collections.Generic;
using System.Linq;
using Xwt.Drawing;

namespace Tangerine
{
	public static class Colors
	{
		public static Color ToolPanelBackground = Xwt.Drawing.Color.FromName("#E0E6FF");
		public static Color Text = Color.FromName("#000000");
		public static Color SelectedRow = new Color(0.8, 0.8, 0.8);
		public static Color NotSelectedRow = Color.FromName("#E0E0E0");
		public static Color ActiveBackground = Color.FromName("#FFFFFF");
		public static Color GridLines = Color.FromName("#E0E0E0");
		public static Color TimelineCursor = Color.FromName("#E05050");
	}
}