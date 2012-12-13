using Lime;
using ProtoBuf;
using System.IO;
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
		public string Tools = "";

		[ProtoMember(3)]
		public bool Enabled = true;

		BareEventHandler clicked;

		private void Update(float delta) {
			if (Enabled && Visible && Input.WasMouseReleased() && HitTest(Input.MousePosition)) {
				Input.ConsumeKeyEvent(Key.Mouse0, true);
				clicked();
			}
		}

		public BareEventHandler Clicked
		{
			set
			{
				if( clicked != null)
					Updated -= Update;
				clicked = value;
				if (clicked != null)
					Updated += Update;
			}
		}

	}
}