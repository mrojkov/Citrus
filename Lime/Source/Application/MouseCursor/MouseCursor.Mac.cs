#if MAC || MONOMAC
using System;

namespace Lime
{
	public class MouseCursorImplementation
	{
		public MouseCursorImplementation(Bitmap bitmap, IntVector2 hotSpot)
		{
			throw new NotImplementedException();
		}

		public object NativeCursor { get; private set; }
	}
}
#endif