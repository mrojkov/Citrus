using System;
using Lime;
using ProtoBuf;
using System.IO;
using System.Collections.Generic;

namespace Kumquat
{
	[ProtoContract]
	public class Area : Frame
	{
		[ProtoMember(1)]
		public string CursorName = "";

		[ProtoMember(2)]
		public string Tools = "";
	}
}