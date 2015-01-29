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
				samples.Add(new Sample() { Position = position, Time = DateTime.Now });
				samples.RemoveAll(s => (DateTime.Now - s.Time).TotalSeconds > MeasureTimeInterval);
			}

			public float CalcVelocity()
			{
				if (samples.Count < 2) {
					return 0;
				}
				var s0 = samples.First();
				var s1 = samples.Last();
				var delta = (float)(s1.Time - s0.Time).TotalSeconds;
				if (delta < float.Epsilon) {
					return float.MaxValue;
				} else {
					return (s1.Position - s0.Position) / delta;
				}
			}
		}
	}
}