using Lime;
using ProtoBuf;
using System.IO;
using System;
using System.Collections.Generic;

namespace Kumquat
{
	[ProtoContract]
	public class Area : Clickable
	{
		[ProtoMember(1)]
		public string Tool = "";

		public Area()
		{
			Updated += OnUpdated;
		}

		private void OnUpdated(float delta) {
			if (Clicked != null && Enabled && Visible) {
				if (Input.WasMouseReleased() && HitTest(Input.MousePosition)) {
					Input.ConsumeKeyEvent(Key.Mouse0, true);
					Clicked();
				}
			}
		}

		public BareEventHandler Clicked;
	}
}