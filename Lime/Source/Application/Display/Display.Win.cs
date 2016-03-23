#if WIN
using System.Windows.Forms;

namespace Lime
{
	/// <summary>
	/// Wraps a native <see cref="Screen"/> class.
	/// </summary>
	public class Display : IDisplay
	{
		/// <summary>
		/// Initializes a new instance of <see cref="Display"/> class from native screen.
		/// </summary>
		/// <param name="screen">The native screen.</param>
		public Display(Screen screen)
		{
			WinFormsScreen = screen;
		}

		/// <summary>
		/// Gets the native Windows Forms Screen.
		/// </summary>
		public Screen WinFormsScreen
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the size of the screen in virtual pixels.
		/// </summary>
		public Size Size
		{
			get
			{
				return new Size(
					(int)(WinFormsScreen.Bounds.Width / Window.Current.PixelScale),
					(int)(WinFormsScreen.Bounds.Height / Window.Current.PixelScale));
			}
		}
	}
}
#endif
