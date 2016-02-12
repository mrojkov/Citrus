#if ANDROID || iOS || UNITY
namespace Lime
{
	internal class CursorCollection: ICursorCollection
	{
		public MouseCursor Default { get { return new MouseCursor(); } }
		public MouseCursor Empty { get { return new MouseCursor(); } }
		public MouseCursor Hand { get { return new MouseCursor(); } }
		public MouseCursor IBeam { get { return new MouseCursor(); } }
		public MouseCursor Wait { get { return new MouseCursor(); } }
		public MouseCursor Move { get { return new MouseCursor(); } }
		public MouseCursor SizeNS { get { return new MouseCursor(); } }
		public MouseCursor SizeWE { get { return new MouseCursor(); } }
		public MouseCursor SizeNESW { get { return new MouseCursor(); } }
		public MouseCursor SizeNWSE { get { return new MouseCursor(); } }
	}
}
#endif
