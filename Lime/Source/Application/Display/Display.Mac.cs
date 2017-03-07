#if MAC || MONOMAC
using System;
#if MAC
using AppKit;
#else
using MonoMac.AppKit;
#endif

namespace Lime
{
	internal class Display : IDisplay
	{
		public readonly NSScreen NativeScreen;

		public Display(NSScreen nativeScreen)
		{
			this.NativeScreen = nativeScreen;
		}

		public Vector2 Position => new Vector2((float)NativeScreen.Frame.Left, (float)NativeScreen.Frame.Top);
		public Vector2 Size => new Vector2((float)NativeScreen.Frame.Width, (float)NativeScreen.Frame.Height);
	}
}
#endif
