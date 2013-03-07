using System;

namespace Lime
{
	internal static class TimeUtils
	{
		private static long startTime = DateTime.Now.Ticks;
		private static long timeStamp;
		private static int countedFrames;
		public static float FrameRate { get; private set; }

		public static long GetMillisecondsSinceGameStarted()
		{
			long t = DateTime.Now.Ticks;
			if (startTime == 0) {
				startTime = t;
			}
			return (t - startTime) / 10000;
		}

		public static void RefreshFrameRate()
		{
			countedFrames++;
			long t = DateTime.Now.Ticks;
			long milliseconds = (t - timeStamp) / 10000;
			if (milliseconds > 1000) {
				if (timeStamp > 0) {
					FrameRate = (float)countedFrames / (float)milliseconds / 1000.0f;
				}
				timeStamp = t;
				countedFrames = 0;
			}
		}
	}
}
