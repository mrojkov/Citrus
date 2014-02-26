using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Lime
{
	public static class Debug
	{
		public static bool BreakOnButtonClick;
		public static void Write(string message)
		{
			Debugger.Log(0, null, message);
		}
	}
}
