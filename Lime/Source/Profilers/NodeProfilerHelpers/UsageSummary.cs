using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lime.Source.Profilers.NodeProfilerHelpers
{
	internal sealed class UsageSummary : NodeComponent
	{
#pragma warning disable CS0649
		public long RenderUsage;
		public long UpdateUsage;
#pragma warning restore CS0649
	}
}
