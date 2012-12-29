using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public enum VideoAction
	{
		[ProtoEnum]
		Play,
		[ProtoEnum]
		Stop
	}

	public class Video : Widget
	{
		// Todo: implementation
	}
}
