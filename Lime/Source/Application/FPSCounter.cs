using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Lime
{
	class FPSCounter
	{
		private Stopwatch sw = new Stopwatch();
		private int countedFrames;

		public float FPS { get; private set; }

		public void Refresh()
		{
			if (!sw.IsRunning) {
				sw.Start();
			}
			countedFrames++;
			if (sw.Elapsed.TotalSeconds > 1) {
				FPS = (float)(countedFrames / sw.Elapsed.TotalSeconds);
				countedFrames = 0;
				sw.Restart();
			}
		}
	}
}
