#if MAC || MONOMAC
using System;

namespace Lime
{
	internal class MouseCursorImplementation
	{
		public MouseCursorImplementation(Bitmap bitmap, IntVector2 hotSpot)
		{
			throw new NotImplementedException();
		}

		public MouseCursorImplementation(AppKit.NSCursor nativeCursor)
		{
			NativeCursor = nativeCursor;
		}

		public AppKit.NSCursor NativeCursor { get; private set; }
	}
}
#endif