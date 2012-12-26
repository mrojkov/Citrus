using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;
using ProtoBuf;

namespace Kumquat
{
	[ProtoContract]
	[ProtoInclude(101, typeof(Tool))]
	[ProtoInclude(102, typeof(Area))]
	[ProtoInclude(103, typeof(ExitArea))]
	public class Clickable : Frame
	{
		[ProtoMember(1)]
		public bool Enabled = true;

		[ProtoMember(2)]
		public string CursorName = "";
	}
}