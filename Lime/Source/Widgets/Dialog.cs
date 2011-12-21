namespace Lime
{
	public class Dialog : Frame
	{
		public override void Update (int delta)
		{
			base.Update (delta);
			if (worldShown) {
				Input.ConsumeAllKeyEvents (true);
				Input.HideMouseAway (true);
			}
		}
	}
}
