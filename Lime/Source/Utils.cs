using System;

namespace Lime
{
	public static partial class Utils
	{
		static int ticks = 0;

		public static void BeginTimeMeasurement()
		{
			ticks = Environment.TickCount;
		}

		public static int EndTimeMeasurement()
		{
			ticks = Environment.TickCount - ticks;
			Console.WriteLine(String.Format("Execution time {0} ms.", ticks));
			return ticks;
		}
		
		public static object CreateObject(string className)
		{
			var type = System.Type.GetType(className);
			if (type == null)
				throw new Exception("Unknown type: {0}", className);
			var ctor = type.GetConstructor(System.Type.EmptyTypes);
			if (ctor == null)
				throw new Exception("No public default constructor is defined for: {0}", className);
			var obj = ctor.Invoke(new object[] {});
			return obj;
		}
	}
}