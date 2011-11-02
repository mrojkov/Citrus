using System;
using Lime;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class SplineGear : Node
	{
		[ProtoMember(1)]
		public string WidgetId { get; set; }

		[ProtoMember(2)]
		public string SplineId { get; set; }

		[ProtoMember(3)]
		public float SplineOffset { get; set; }

		public SplineGear ()
		{
		}
	}
}