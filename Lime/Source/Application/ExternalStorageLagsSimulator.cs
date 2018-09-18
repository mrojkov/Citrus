using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	class ExternalStorageLagsSimulator
	{
		static DateTime lastReadTime;

		internal static void SimulateReadDelay(string path, int dataLength)
		{
			const float readSpeed = 5000 * 1024;
			int readTime = (int)(1000L * dataLength / readSpeed);
			if (DateTime.Now - lastReadTime > new TimeSpan(0, 0, 1)) {
				readTime += 200;
				lastReadTime = DateTime.Now;
			}
			if (readTime > 10 && Application.CurrentThread.IsMain()) {
				Logger.Write("Lag {0} ms while reading {1}", readTime, path);
			}
			System.Threading.Thread.Sleep(readTime);
		}
	}
}
