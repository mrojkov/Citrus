#if WIN
using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Lime
{
	public class MouseCursorImplementation
	{
		/// <summary>
		/// Create a cursor from the specified Win32 cursor handle.
		/// </summary>
		public MouseCursorImplementation(IntPtr handle) : this(new Cursor(handle)) { }

		public MouseCursorImplementation(Cursor nativeCursor)
		{
			NativeCursor = nativeCursor;
		}

		public MouseCursorImplementation(Bitmap bitmap, IntVector2 hotSpot)
		{
			var handle = bitmap.NativeBitmap.GetHicon();
			var info = new IconInfo();
			GetIconInfo(handle, ref info);
			info.HotSpotX = hotSpot.X;
			info.HotSpotY = hotSpot.Y;
			info.IsIcon = false;
			handle = CreateIconIndirect(ref info);
			NativeCursor = new Cursor(handle);
		}

		public Cursor NativeCursor { get; private set; }

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
