using System;

namespace Lime
{
	public class Logger
	{
		public static event Action<string> OnWrite;

		public static void Write(string msg)
		{
#if UNITY
			UnityEngine.Debug.Log(msg);
#else
			Console.WriteLine(msg);
#endif
			if (OnWrite != null) {
				OnWrite(msg);
			}
		}

		public static void Write(string format, params object[] args)
		{
			Write(string.Format(format, args));
		}
	}
}
