#if WIN
using System.Windows.Forms;

namespace Lime
{
	internal class Display : IDisplay
	{
		public readonly Screen Screen;

		public Display(Screen screen)
		{
			Screen = screen;
		}

		public Vector2 Size => new Vector2(
			Screen.Bounds.Width / Window.Current.PixelScale,
			Screen.Bounds.Height / Window.Current.PixelScale)
	}
}
#endif
