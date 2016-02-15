#if ANDROID || iOS || UNITY
namespace Lime
{
	internal class CursorCollection: ICursorCollection
	{
		public MouseCursor Default { get { return null; } }
		public MouseCursor Empty { get { return null; } }
		public MouseCursor Hand { get { return null; } }
		public MouseCursor IBeam { get { return null; } }
		public MouseCursor Wait { get { return null; } }
		public MouseCursor Move { get { return null; } }
		public MouseCursor SizeNS { get { return null; } }
		public MouseCursor SizeWE { get { return null; } }
		public MouseCursor SizeNESW { get { return null; } }
		public MouseCursor SizeNWSE { get { return null; } }
	}
}
#endif
