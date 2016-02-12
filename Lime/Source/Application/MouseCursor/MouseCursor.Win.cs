#if WIN
using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Lime
{
	public partial class MouseCursor
	{
		/// <summary>
		/// Create a cursor from the specified Win32 cursor pointer.
		/// </summary>
		public MouseCursor(IntPtr handle) : this(new Cursor(handle)) { }

		public MouseCursor(Cursor winFormsCursor)
		{
			WinFormsCursor = winFormsCursor;
		}

		public MouseCursor(System.Drawing.Bitmap bitmap, IntVector2 hotSpot)
		{
			var ptr = bitmap.GetHicon();
			var info = new IconInfo();
			GetIconInfo(ptr, ref info);
			info.HotSpotX = hotSpot.X;
			info.HotSpotY = hotSpot.Y;
			info.IsIcon = false;
			ptr = CreateIconIndirect(ref info);
			WinFormsCursor = new Cursor(ptr);
		}

		public Cursor WinFormsCursor { get; private set; }

		/// <summary>
		/// http://www.pinvoke.net/default.aspx/user32.geticoninfo
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private struct IconInfo
		{
			/// <summary>
			/// Specifies whether this structure defines an icon or a cursor.
			/// A value of true specifies an icon; false specifies a cursor.
			/// </summary>
			public bool IsIcon;
			public int HotSpotX;
			public int HotSpotY;

			// TODO: These bitmaps are unmanaged and must be deleted after use.
			public IntPtr MaskBitmap;
			public IntPtr ColorBitmap;
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

		[DllImport("user32.dll")]
		private static extern IntPtr CreateIconIndirect(ref IconInfo icon);
	}
}
#endif
