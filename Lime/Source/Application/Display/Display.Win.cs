#if WIN
using System.Collections.Generic;
using System.Windows.Forms;

namespace Lime
{
	internal class Display : IDisplay
	{
		private Screen Screen;
		public static List<Display> Displays = new List<Display>();

		public static Display GetDisplay(Screen screen)
		{
			foreach (var d in Displays)
				if (d.Screen.Equals(screen))
					return d;
			var nd = new Display { Screen = screen };
			Displays.Add(nd);
			return nd;
		}

		public Vector2 Position => new Vector2(
			Screen.Bounds.Left / Window.Current.PixelScale,
			Screen.Bounds.Top / Window.Current.PixelScale);

		public Vector2 Size => new Vector2(
			Screen.Bounds.Width / Window.Current.PixelScale,
			Screen.Bounds.Height / Window.Current.PixelScale);
	}
}
#endif
