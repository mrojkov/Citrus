#if ANDROID
using Android.Graphics;
using AndroidDisplay = Android.Views.Display;

namespace Lime
{
	internal class Display : IDisplay
	{
		private readonly AndroidDisplay nativeDisplay;

		public Display(AndroidDisplay nativeDisplay)
		{
			this.nativeDisplay = nativeDisplay;
		}

		public Vector2 Position => Vector2.Zero;

		public Vector2 Size
		{
			get
			{
				var size = new Point();
				nativeDisplay.GetRealSize(size);
				return new Vector2(size.X / Window.Current.PixelScale, size.Y / Window.Current.PixelScale);
			}
		}
	}
}
#endif
