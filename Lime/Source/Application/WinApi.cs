#if WIN
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Lime
{
	static class WinApi
	{
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int MessageBox(IntPtr hWnd, string text, string caption, int options);
	}
}
#endif