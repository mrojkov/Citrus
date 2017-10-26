using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public static class Debug
	{
		public static void Write(string message)
		{
			Logger.Write(message);
		}

		public static void Write(object value)
		{
			Write(value?.ToString() ?? "null");
		}

		public static void Write(string msg, params object[] args)
		{
			Logger.Write(msg, args);
		}
	}
}
