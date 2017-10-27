using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public partial class ScrollView
	{
		class VelocityMeter
		{
			struct Sample
			{
				public float Position;
				public DateTime Time;
			}

			const float MeasureTimeInterval = 0.15f;

			private readonly List<Sample> samples = new List<Sample>();

			public void AddSample(float position)
			{
				var timeStamp = DateTime.Now;
				if (samples.Count > 0 && samples[samples.Count - 1].Time == timeStamp) {
					return;
				}
				samples.Add(new Sample { Position = position, Time = timeStamp });
				samples.RemoveAll(s => (timeStamp - s.Time).TotalSeconds > MeasureTimeInterval);
			}

			public float CalcVelocity()
			{
				if (samples.Count < 2) {
					return 0;
				}
				var s0 = samples.First();
				var s1 = samples.Last();
				var delta = (float)(s1.Time - s0.Time).TotalSeconds;
				return (s1.Position - s0.Position) / delta;
			}
		}
	}
}