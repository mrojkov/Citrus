using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ProtoBuf;

namespace Tangerine
{
	[ProtoContract]
	public class Preferences
	{
		public int TimelineRowHeight = 30;
		public int TimelineColWidth = 10;
		public int InspectorRowHeight = 30;
		public int InspectorDefaultWidth = 250;
		public int TimelineDefaultHeight = 250;

		public static readonly Preferences Instance = new Preferences();
	}
}
