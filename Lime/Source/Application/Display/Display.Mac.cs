#if MAC || MONOMAC
using System;
using AppKit;

namespace Lime
{
	internal class Display : IDisplay
	{
		public readonly NSScreen NativeScreen;

		public Display(NSScreen nativeScreen)
		{
			this.NativeScreen = nativeScreen;
		}

		public Vector2 Size => new Vector2((float)NativeScreen.Frame.Width, (float)NativeScreen.Frame.Height);
	}
}
#endif
