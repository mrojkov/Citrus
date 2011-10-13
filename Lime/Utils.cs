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
    }
}