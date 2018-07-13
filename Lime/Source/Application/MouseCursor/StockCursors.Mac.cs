#if MAC || MONOMAC
using System;

#if MAC
using AppKit;
#else
using MonoMac.AppKit;
#endif

namespace Lime
{
	internal class StockCursors: IStockCursors
	{
		public MouseCursor Default { get; private set; }
		public MouseCursor Empty { get; private set; }
		public MouseCursor Hand { get; private set; }
		public MouseCursor IBeam { get; private set; }
		public MouseCursor SizeNS { get; private set; }
		public MouseCursor SizeWE { get; private set; }
		public MouseCursor SizeAll { get; private set; }
		public MouseCursor SizeNWSE { get; private set; }
		public MouseCursor SizeNESW { get; private set; }


		private static MouseCursor FromNSCursor(NSCursor cursor)
		{
			var implementation = new MouseCursorImplementation(cursor);
			return new MouseCursor(implementation);
		}

		public StockCursors()
		{
			Default = FromNSCursor(NSCursor.ArrowCursor); 
			Hand = FromNSCursor(NSCursor.PointingHandCursor);
			IBeam = FromNSCursor(NSCursor.IBeamCursor);
			SizeNS = FromNSCursor(NSCursor.ResizeUpDownCursor);
			SizeWE = FromNSCursor(NSCursor.ResizeLeftRightCursor);

			//TODO: Add diagonal cursors
			SizeNWSE = FromNSCursor(NSCursor.ResizeUpDownCursor);
			SizeNESW = FromNSCursor(NSCursor.ResizeUpDownCursor);
		}
	}
}
#endif
