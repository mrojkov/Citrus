#if MAC || MONOMAC
using System;
using System.Drawing;
#if MAC
using AppKit;
#else
using MonoMac.AppKit;
#endif

namespace Lime.Platform
{
	public enum DisplayIndex
	{
		First = 0,
		Second,
		Third,
		Forth,
		Fifth,
		Sixth,
		Default = -1
	}
	
	public class DisplayDevice
	{
		public Rectangle Bounds { get; private set; }
		public float Width { get { return Bounds.Width; } }
		public float Height { get { return Bounds.Height; } }

		public static readonly DisplayDevice Default = new DisplayDevice(GetScreenBounds(NSScreen.MainScreen));

		private DisplayDevice(Rectangle bounds)
		{
			this.Bounds = bounds;
		}

		private static Rectangle GetScreenBounds(NSScreen screen)
		{
			var frame = screen.Frame;
			return new Rectangle((int)frame.X, (int)frame.Y, (int)frame.Width, (int)frame.Height);
		}
		
		public static DisplayDevice GetDisplay(DisplayIndex index)
		{
			if (index == DisplayIndex.Default) {
				return Default;
			}
			var screens = NSScreen.Screens;
			if ((int)index < 0 || (int)index >= screens.Length) {
				return null;
			}
			return new DisplayDevice(GetScreenBounds(screens[(int)index]));
		}
	}
}
#endif