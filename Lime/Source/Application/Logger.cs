using System;

namespace Lime
{
	public static class Logger
	{
		public static event Action<string> OnWrite;

		public static void Write(string msg)
		{
			Console.WriteLine(msg);
			OnWrite?.Invoke(msg);
		}

		public static void Write(string format, params object[] args)
		{
			Write(string.Format(format, args));
		}
	}
}
