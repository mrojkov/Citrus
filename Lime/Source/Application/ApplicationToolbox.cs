using System;

namespace Lime
{
	internal static class ApplicationToolbox
	{
		private static long startTime = DateTime.Now.Ticks;
		private static long timeStamp;
		private static int countedFrames;
		public static float FrameRate { get; private set; }
		static DateTime lastReadTime;

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
					FrameRate = (float)countedFrames / (float)milliseconds * 1000.0f;
				}
				timeStamp = t;
				countedFrames = 0;
			}
		}

		public static void SimulateReadDelay(string path, int dataLength)
		{
			const float readSpeed = 5000 * 1024;
			int readTime = (int)(1000L * dataLength / readSpeed);
			if (DateTime.Now - lastReadTime > new TimeSpan(0, 0, 1)) {
				readTime += 200;
				lastReadTime = DateTime.Now;
			}
			if (readTime > 10 && Application.IsMainThread) {
				Logger.Write("Lag {0} ms while reading {1}", readTime, path);
			}
			System.Threading.Thread.Sleep(readTime);
		}
	}
}
