#if WIN
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;

namespace Lime
{
	internal static class WinCursors
	{
		static Dictionary<string, IntPtr> CursorMap = new Dictionary<string, IntPtr>();
		static IntPtr currentCursor;

		public static void SetCursor(string resourceName, IntVector2 hotSpot)
		{
			currentCursor = GetCursor(resourceName, hotSpot);
			WinApi.SetClassLong(GetWindowHandle(), WinApi.GCL_HCURSOR, currentCursor);
			WinApi.SetCursor(currentCursor);
		}

		private static IntPtr GetCursor(string resourceName, IntVector2 hotSpot)
		{
			IntPtr cursor;
			if (CursorMap.TryGetValue(resourceName, out cursor)) {
				return cursor;
			}
			Bitmap bitmap = CreateBitmapFromResource(resourceName);
			cursor = CreateCursor(bitmap, hotSpot);
			CursorMap[resourceName] = cursor;
			return cursor;
		}

		private static Bitmap CreateBitmapFromResource(string resourceName)
		{
			var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
			var fullResourceName = entryAssembly.GetName().Name + "." + resourceName;
			var file = entryAssembly.GetManifestResourceStream(fullResourceName);
			Bitmap bitmap = new Bitmap(file);
			if (bitmap.Width > 32 || bitmap.Height > 32) {
				// Заменил Exception на Warning, так как SP1 и SP2 встречаются редко.
				// Если вдруг нам всё-таки понадобится их поддержка, то сюда нужно
				// добавить код, который будет ресайзить битмап до нужного размера.
				// Нужный размер можно узнать с помощью 
				// GetSystemMetrics(SM_CXCURSOR) и GetSystemMetrics(SM_CYCURSOR)
				// -- Гриша.
				Console.WriteLine("WARNING: A cursor should not exceeds 32x32 pixels (Windows XP SP1 and SP2 compatibility)");
			}
			return bitmap;
		}

		private static IntPtr CreateCursor(Bitmap bmp, IntVector2 hotSpot)
		{
			IntPtr ptr = bmp.GetHicon();
			WinApi.IconInfo tmp = new WinApi.IconInfo();
			WinApi.GetIconInfo(ptr, ref tmp);
			tmp.xHotspot = hotSpot.X;
			tmp.yHotspot = hotSpot.Y;
			tmp.fIcon = false;
			ptr = WinApi.CreateIconIndirect(ref tmp);
			return ptr;
		}

		private static IntPtr GetWindowHandle()
		{
			OpenTK.Platform.IWindowInfo wi = GameView.Instance.WindowInfo;
			object inf = GameView.Instance.WindowInfo;
			PropertyInfo pi = inf.GetType().GetProperty("WindowHandle");
			IntPtr hwnd = (IntPtr)pi.GetValue(wi, null);
			return hwnd;
		}
	}
}
#endif
