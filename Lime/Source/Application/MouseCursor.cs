using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class MouseCursor
	{
		public MouseCursor() { }
		public MouseCursor(string name, IntVector2 hotSpot, string assemblyName = null) { }
		public static readonly MouseCursor Default = new MouseCursor();
		public static readonly MouseCursor Empty = new MouseCursor();
	}
}