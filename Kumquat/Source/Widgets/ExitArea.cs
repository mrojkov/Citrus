using System;
using Lime;
using ProtoBuf;
using System.IO;
using System.Collections.Generic;

namespace Kumquat
{
	[ProtoContract]
	public class ExitArea : Area
	{
		[ProtoMember(1)]
		public string ExitTo = "";

	}
}