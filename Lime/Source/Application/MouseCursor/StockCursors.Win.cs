#if WIN
using System.Windows.Forms;

namespace Lime
{
	internal class StockCursors: IStockCursors
	{
		public StockCursors()
		{
			Empty = new MouseCursor(new Bitmap(new Color4[1], 1, 1), IntVector2.Zero);
			Hand = FromWinFormsCursor(Cursors.Hand);
			IBeam = FromWinFormsCursor(Cursors.IBeam);
			Default = FromWinFormsCursor(Cursors.Default);
			SizeNS = FromWinFormsCursor(Cursors.SizeNS);
			SizeWE = FromWinFormsCursor(Cursors.SizeWE);
			SizeAll = FromWinFormsCursor(Cursors.SizeAll);
			SizeNWSE = FromWinFormsCursor(Cursors.SizeNWSE);
			SizeNESW = FromWinFormsCursor(Cursors.SizeNESW);
		}

		private static MouseCursor FromWinFormsCursor(Cursor cursor)
		{
			var implementation = new MouseCursorImplementation(cursor);
			return new MouseCursor(implementation);
		}

		public MouseCursor Default { get; private set; }
		public MouseCursor Empty { get; private set; }
		public MouseCursor Hand { get; private set; }
		public MouseCursor IBeam { get; private set; }
		public MouseCursor SizeNS { get; private set; }
		public MouseCursor SizeWE { get; private set; }
		public MouseCursor SizeAll { get; private set; }
		public MouseCursor SizeNWSE { get; private set; }
		public MouseCursor SizeNESW { get; private set; }
	}
}
#endif
