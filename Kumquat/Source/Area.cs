using Lime;
using ProtoBuf;
using System.IO;
using System;
using System.Collections.Generic;

namespace Kumquat
{
	[ProtoContract]
	[ProtoInclude(100, typeof(ExitArea))]
	[ProtoInclude(101, typeof(Tool))]
	public class Area : Frame
	{
		[ProtoMember(1)]
		public string CursorName = "";

		[ProtoMember(2)]
		public string Tool = "";

		[ProtoMember(3)]
		public bool Enabled = true;

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