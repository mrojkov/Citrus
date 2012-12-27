using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Lime
{
	internal static class WinCursors
	{
		static class WinApi
		{
			public static int GCL_HCURSOR = -12;

			public struct IconInfo
			{
				public bool fIcon;
				public int xHotspot;
				public int yHotspot;
				public IntPtr hbmMask;
				public IntPtr hbmColor;
			}

			[DllImport("user32.dll")]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

			[DllImport("user32.dll")]
			public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

			[DllImport("user32.dll")]
			public static extern int SetClassLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
			
			[DllImport("user32.dll")]
			public static extern void SetCursor(IntPtr dwCursor);
		}

		static Dictionary<string, IntPtr> CursorMap = new Dictionary<string, IntPtr>();
		static IntPtr currentCursor;

		public static void SetCursor(string resourceName)
		{
			currentCursor = GetCursor(resourceName);
			WinApi.SetClassLong(GetWindowHandle(), WinApi.GCL_HCURSOR, currentCursor);
			WinApi.SetCursor(currentCursor);
		}

		private static IntPtr GetCursor(string resourceName)
		{
			IntPtr cursor;
			if (CursorMap.TryGetValue(resourceName, out cursor)) {
				return cursor;
			}
			Bitmap bitmap = CreateBitmapFromResource(resourceName);
			cursor = CreateCursor(bitmap, bitmap.Width / 2, bitmap.Height / 2);
			CursorMap[resourceName] = cursor;
			return cursor;
		}

		private static Bitmap CreateBitmapFromResource(string resourceName)
		{
			var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
			var fullResourceName = entryAssembly.GetName().Name + "." + resourceName;
			var file = entryAssembly.GetManifestResourceStream(fullResourceName);
			Bitmap bitmap = new Bitmap(file);
			return bitmap;
		}

		private static IntPtr CreateCursor(Bitmap bmp, int xHotSpot, int yHotSpot)
		{
			IntPtr ptr = bmp.GetHicon();
			WinApi.IconInfo tmp = new WinApi.IconInfo();
			WinApi.GetIconInfo(ptr, ref tmp);
			tmp.xHotspot = xHotSpot;
			tmp.yHotspot = yHotSpot;
			tmp.fIcon = false;
			ptr = WinApi.CreateIconIndirect(ref tmp);
			return ptr;
		}

		private static IntPtr GetWindowHandle()
		{
			OpenTK.Platform.IWindowInfo ii = GameView.Instance.WindowInfo;
			object inf = GameView.Instance.WindowInfo;
			PropertyInfo pi = inf.GetType().GetProperty("WindowHandle");
			IntPtr hwnd = (IntPtr)pi.GetValue(ii, null);
			return hwnd;
		}
	}
}
