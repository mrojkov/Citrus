#if MAC || MONOMAC
using System;

namespace Lime
{
	internal class CursorCollection: ICursorCollection
	{
		public MouseCursor Default { get { throw new NotImplementedException(); } }
		public MouseCursor Empty { get { throw new NotImplementedException(); } }
		public MouseCursor Hand { get { throw new NotImplementedException(); } }
		public MouseCursor IBeam { get { throw new NotImplementedException(); } }
		public MouseCursor Wait { get { throw new NotImplementedException(); } }
		public MouseCursor Move { get { throw new NotImplementedException(); } }
		public MouseCursor SizeNS { get { throw new NotImplementedException(); } }
		public MouseCursor SizeWE { get { throw new NotImplementedException(); } }
		public MouseCursor SizeNESW { get { throw new NotImplementedException(); } }
		public MouseCursor SizeNWSE { get { throw new NotImplementedException(); } }
	}
}
#endif
