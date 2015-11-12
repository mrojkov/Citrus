using System;

namespace ChromiumWebBrowser
{
	public class PopupTransformArgs : EventArgs
	{
		public int Width;
		public int Height;
		public int X;
		public int Y;

		public PopupTransformArgs(int width, int height, int x, int y)
		{
			Width = width;
			Height = height;
			X = x;
			Y = y;
		}
	}
}
