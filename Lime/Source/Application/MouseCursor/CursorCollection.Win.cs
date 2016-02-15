#if WIN
using System.Windows.Forms;

namespace Lime
{
	internal class CursorCollection: ICursorCollection
	{
		public CursorCollection()
		{
			Empty = new MouseCursor(new Bitmap(new Color4[1], 1, 1), IntVector2.Zero);
			Hand = FromWinFormsCursor(Cursors.Hand);
			IBeam = FromWinFormsCursor(Cursors.IBeam);
			Default = FromWinFormsCursor(Cursors.Default);
			Wait = FromWinFormsCursor(Cursors.WaitCursor);
			Move = FromWinFormsCursor(Cursors.SizeAll);
			SizeNS = FromWinFormsCursor(Cursors.SizeNS);
			SizeWE = FromWinFormsCursor(Cursors.SizeWE);
			SizeNESW = FromWinFormsCursor(Cursors.SizeNESW);
			SizeNWSE = FromWinFormsCursor(Cursors.SizeNWSE);
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
		public MouseCursor Wait { get; private set; }
		public MouseCursor Move { get; private set; }
		public MouseCursor SizeNS { get; private set; }
		public MouseCursor SizeWE { get; private set; }
		public MouseCursor SizeNESW { get; private set; }
		public MouseCursor SizeNWSE { get; private set; }
	}
}
#endif
