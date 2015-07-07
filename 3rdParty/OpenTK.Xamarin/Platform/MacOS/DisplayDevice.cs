#if MAC
using System;
using System.Drawing;
using AppKit;

namespace OpenTK
{
	public class DisplayDevice
	{
		public Rectangle Bounds { get; private set; }
		public float Width { get { return Bounds.Width; } }
		public float Height { get { return Bounds.Height; } }

		public static readonly DisplayDevice Default = new DisplayDevice(GetDefaultDeviceBounds());

		private DisplayDevice(Rectangle bounds)
		{
			this.Bounds = bounds;
		}

		private static Rectangle GetDefaultDeviceBounds()
		{
			var frame = NSScreen.MainScreen.Frame;
			return new Rectangle((int)frame.X, (int)frame.Y, (int)frame.Width, (int)frame.Height);
		}
	}
}
#endif