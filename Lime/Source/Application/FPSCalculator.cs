using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	class FPSCalculator
	{
		private static DateTime fpsRefreshTimeStamp = DateTime.UtcNow;
		private static int countedFrames;

		public static float FPS { get; private set; }

		public static void Refresh()
		{
			countedFrames++;
			var now = DateTime.UtcNow;
			float milliseconds = (float)(now - fpsRefreshTimeStamp).TotalMilliseconds;
			if (milliseconds > 1000) {
				FPS = countedFrames / milliseconds * 1000.0f;
				fpsRefreshTimeStamp = now;
				countedFrames = 0;
			}
		}
	}
}
