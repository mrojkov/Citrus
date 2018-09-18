using System;

namespace ChromiumWebBrowser
{
	public class PopupTransformEventArgs : EventArgs
	{
		public int Width;
		public int Height;
		public int X;
		public int Y;

		public PopupTransformEventArgs(int width, int height, int x, int y)
		{
			Width = width;
			Height = height;
			X = x;
			Y = y;
		}
	}
}
