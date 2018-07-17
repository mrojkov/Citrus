#if ANDROID || iOS
namespace Lime
{
	internal class StockCursors: IStockCursors
	{
		public MouseCursor Default { get { return null; } }
		public MouseCursor Empty { get { return null; } }
		public MouseCursor Hand { get { return null; } }
		public MouseCursor IBeam { get { return null; } }
		public MouseCursor SizeNS { get { return null; } }
		public MouseCursor SizeWE { get { return null; } }
		public MouseCursor SizeAll { get { return null; } }
		public MouseCursor SizeNWSE { get { return null; } }
		public MouseCursor SizeNESW { get { return null; } }
	}
}
#endif
