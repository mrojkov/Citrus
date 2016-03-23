#if MAC || MONOMAC
using System;

using AppKit;

namespace Lime
{
	/// <summary>
	/// Wraps a native <see cref="NSScreen"/> class.
	/// </summary>
	public class Display : IDisplay
	{
		/// <summary>
		/// Initializes a new instance of <see cref="Display"/> class from native screen.
		/// </summary>
		/// <param name="screen">The native screen.</param>
		public Display(NSScreen screen)
		{
			NSScreen = screen;
		}

		/// <summary>
		/// Gets the native Mac Display.
		/// </summary>
		public NSScreen NSScreen
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the size of display in virtual pixels.
		/// </summary>
		public Size Size
		{
			get { return new Size((int)NSScreen.Frame.Width, (int)NSScreen.Frame.Height); }
		}
	}
}
#endif
