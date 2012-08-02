using System;

namespace Lime
{
	public class TimeWaiter : Node
	{
		private float time;
		private BareEventHandler evt;

		public TimeWaiter(float time, BareEventHandler evt)
		{
			this.evt = evt;
			this.time = time;
		}

		public override void Update(int delta)
		{
			if ((time -= delta * 0.001f) <= 0) {
				evt();
				Unlink();
			}
		}

		public static TimeWaiter Create(Node parent, float time, BareEventHandler evt)
		{
			var trigger = new TimeWaiter(time, evt);
			parent.Nodes.Add(trigger);
			return trigger;
		}
	}
}
