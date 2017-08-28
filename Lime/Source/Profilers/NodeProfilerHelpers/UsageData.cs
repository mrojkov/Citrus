using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lime.Source.Profilers.NodeProfilerHelpers
{
	internal sealed class UsageData : NodeComponent
	{
		public long RenderTicks;
		public long UpdateTicks;

		public void Clear()
		{
			RenderTicks = 0;
			UpdateTicks = 0;
		}
	}
}
