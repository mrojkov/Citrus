#if MAC || MONOMAC
using System;

#if MAC
using AppKit;
#else
using MonoMac.AppKit;
#endif

namespace Lime
{
	internal class MouseCursorImplementation
	{
		public MouseCursorImplementation(Bitmap bitmap, IntVector2 hotSpot)
		{
			var handle = bitmap.NativeBitmap;
			var icon = new NSImage (bitmap.NativeBitmap, new CoreGraphics.CGSize (bitmap.NativeBitmap.Width, bitmap.NativeBitmap.Height));
			NativeCursor = new NSCursor (icon, new CoreGraphics.CGPoint (hotSpot.X, hotSpot.Y));
		}

		public MouseCursorImplementation(NSCursor nativeCursor)
		{
			NativeCursor = nativeCursor;
		}

		public NSCursor NativeCursor { get; private set; }
	}
}
#endif