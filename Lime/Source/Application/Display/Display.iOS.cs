#if iOS
using System;

using UIKit;

namespace Lime
{
	/// <summary>
	/// Wraps a native <see cref="UIScreen"/> class.
	/// </summary>
	public class Display : IDisplay
	{
		/// <summary>
		/// Initializes a new instance of <see cref="Display"/> class from native screen.
		/// </summary>
		/// <param name="screen">The native screen.</param>
		public Display(UIScreen screen)
		{
			UIScreen = screen;
		}

		/// <summary>
		/// Gets the native iOS Display.
		/// </summary>
		public UIScreen UIScreen
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the size of display in virtual pixels.
		/// </summary>
		public Size Size
		{
			get
			{
				return new Size((int)UIScreen.Bounds.Width, (int)UIScreen.Bounds.Height);
			}
		}
	}
}
#endif
