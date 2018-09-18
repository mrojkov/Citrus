using System;
using CefSharp;

namespace ChromiumWebBrowser
{
	public class CursorChangedEventArgs : EventArgs
	{
		public readonly IntPtr Handle;
		public readonly CefCursorType Type;

		public CursorChangedEventArgs(IntPtr handle, CefCursorType type)
		{
			Handle = handle;
			Type = type;
		}
	}
}