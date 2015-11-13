using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if WIN
using System.Windows.Forms;
#endif

namespace Lime
{
	public class MouseCursor
	{
		public MouseCursor() { }
		public MouseCursor(string name, IntVector2 hotSpot, string assemblyName = null) { }
		public static readonly MouseCursor Default = new MouseCursor();
		public static readonly MouseCursor Empty = new MouseCursor();
#if WIN
		public Cursor WinFormsCursor { get; private set; }
		
		public MouseCursor(IntPtr handle): this(new Cursor(handle)) { }

		public MouseCursor(Cursor winFormsCursor)
		{
			WinFormsCursor = winFormsCursor;
		}
#endif
	}
}