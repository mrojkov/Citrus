#if WIN
using System.Windows.Forms;

namespace Lime
{
	internal class CursorCollection: ICursorCollection
	{

		public CursorCollection()
		{
			Empty = new MouseCursor(new System.Drawing.Bitmap(1, 1), IntVector2.Zero);
			Hand = new MouseCursor(Cursors.Hand);
			IBeam = new MouseCursor(Cursors.IBeam);
			Default = new MouseCursor(Cursors.Default);
			Wait = new MouseCursor(Cursors.WaitCursor);
			Move = new MouseCursor(Cursors.SizeAll);
			SizeNS = new MouseCursor(Cursors.SizeNS);
			SizeWE = new MouseCursor(Cursors.SizeWE);
			SizeNESW = new MouseCursor(Cursors.SizeNESW);
			SizeNWSE = new MouseCursor(Cursors.SizeNWSE);
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
