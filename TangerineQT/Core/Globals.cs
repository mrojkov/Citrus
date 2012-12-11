using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine
{
	public static class The
	{
		public static Timeline Timeline { get { return Timeline.Instance; } }
		public static Document Document { get { return Document.Instance; } }
	}
}
