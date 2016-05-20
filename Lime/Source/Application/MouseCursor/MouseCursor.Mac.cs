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
			throw new NotImplementedException();
		}

		public MouseCursorImplementation(NSCursor nativeCursor)
		{
			NativeCursor = nativeCursor;
		}

		public NSCursor NativeCursor { get; private set; }
	}
}
#endif