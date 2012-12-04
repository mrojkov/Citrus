using System;
using Lime;
using ProtoBuf;
using System.IO;
using System.Collections.Generic;

namespace Lime
{
	[ProtoContract]
	public class ExitArea : Area
	{
		[ProtoMember(1)]
		public string ExitTo = "";
	}
}