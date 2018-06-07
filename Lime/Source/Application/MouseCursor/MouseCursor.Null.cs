#if ANDROID || iOS
namespace Lime
{
	public class MouseCursorImplementation
	{
		public MouseCursorImplementation(Bitmap bitmap, IntVector2 hotSpot) { }

		public object NativeCursor { get; private set; }
	}
}
#endif
