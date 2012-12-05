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

		public ExitArea()
		{
			Clicked = () => {
				var location = GameScreen.Instance.CurrentLocation;
				if (location != null) {
					location.ExitAreaClick(this);
					Input.ConsumeAllKeyEvents(true);
				}
			};
		}

	}
}