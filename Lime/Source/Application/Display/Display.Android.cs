#if ANDROID
using Android.Graphics;
using AndroidDisplay = Android.Views.Display;

namespace Lime
{
	/// <summary>
	/// Wraps a native <see cref="Android.Views.Display"/> class.
	/// </summary>
	public class Display : IDisplay
	{
		/// <summary>
		/// Initializes a new instance of <see cref="Display"/> class from native screen.
		/// </summary>
		/// <param name="screen">The native screen.</param>
		public Display(AndroidDisplay screen)
		{
			AndroidScreen = screen;
		}

		/// <summary>
		/// Gets the native Android Display.
		/// </summary>
		public AndroidDisplay AndroidScreen
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the real size of display in virtual pixels.
		/// </summary>
		public Size Size
		{
			get
			{
				var size = new Point();
				AndroidScreen.GetRealSize(size);
				return new Size((int)(size.X / Window.Current.PixelScale), (int)(size.Y / Window.Current.PixelScale));
			}
		}
	}
}
#endif
