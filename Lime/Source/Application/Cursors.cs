#if WIN
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;

namespace Lime
{
	internal static class Cursors
	{
		static Dictionary<string, IntPtr> CursorMap = new Dictionary<string, IntPtr>();
		static IntPtr currentCursor;

		public static void SetCursor(string resourceName, IntVector2 hotSpot)
		{
			currentCursor = GetCursor(resourceName, hotSpot);
			Sdl.SetCursor(currentCursor);
		}

		private static IntPtr GetCursor(string resourceName, IntVector2 hotSpot)
		{
			IntPtr cursor;
			if (CursorMap.TryGetValue(resourceName, out cursor)) {
				return cursor;
			}
			cursor = CreateSdlCursorFromResource(resourceName, hotSpot);
			CursorMap[resourceName] = cursor;
			return cursor;
		}

		private static IntPtr CreateSdlCursorFromResource(string resourceName, IntVector2 hotSpot)
		{
			var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
			var fullResourceName = entryAssembly.GetName().Name + "." + resourceName;
			var file = entryAssembly.GetManifestResourceStream(fullResourceName);
			Bitmap bitmap = new Bitmap(file);			
			var lockRect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
			var lockMode = System.Drawing.Imaging.ImageLockMode.ReadOnly;
			var data = bitmap.LockBits(lockRect, lockMode, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			var surface = Sdl.CreateRGBSurfaceFrom(data.Scan0,
				data.Width, data.Height, 32, data.Stride, 
				0x00FF0000, 0x0000FF00, 0x000000FF, 0xFF000000);
			var cursor = Sdl.CreateColorCursor(surface, hotSpot.X, hotSpot.Y);
			Sdl.FreeSurface(surface);
			bitmap.UnlockBits(data);
			return cursor;
		}
	}
}
#endif
