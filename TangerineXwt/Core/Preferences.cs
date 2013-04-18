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
		public double TimelineRowHeight = 25;
		public double TimelineColWidth = 10;
		public double InspectorRowHeight = 30;
		public double InspectorDefaultWidth = 250;
		public double TimelineDefaultHeight = 250;
		public double TimelineDefaultPanviewHeight = 50;
		public double TimelineRulerHeight = 30;
		public double TimelineDefaultRollWidth = 150;

		public static readonly Preferences Instance = new Preferences();
	}
}
