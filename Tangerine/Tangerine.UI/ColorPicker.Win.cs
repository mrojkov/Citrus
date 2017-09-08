#if WIN
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using Lime;

namespace Tangerine.UI
{
	public static class ColorPicker
	{
		[DllImport("gdi32")]
		private static extern uint GetPixel(IntPtr hDC, int XPos, int YPos);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool GetCursorPos(out Point pt);

		[DllImport("User32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr GetWindowDC(IntPtr hWnd);

		public static Color4 PickAtCursor()
		{
			var handle = GetWindowDC(IntPtr.Zero);
			Point p;
			GetCursorPos(out p);
			var color = new Color4(GetPixel(handle, p.X, p.Y));
			color.A = 255;
			return color;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct Point
		{
			public int X;
			public int Y;
			public Point(int x, int y)
			{
				X = x;
				Y = y;
			}
		}
	}
}
#endif