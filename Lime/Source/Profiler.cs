using System;

namespace Lime
{
	public static class Profiler
	{
		static int ticks = 0;

		public static void Start()
		{
			ticks = System.Environment.TickCount;
		}

		public static int Stop()
		{
			ticks = System.Environment.TickCount - ticks;
			Console.WriteLine(String.Format("Execution time {0} ms.", ticks));
			return ticks;
		}
	}
}