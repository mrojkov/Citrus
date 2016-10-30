using System;

namespace Lime
{
	[TangerineClass(allowChildren: true)]
	public class BorderedFrame : Frame
	{
		public BorderedFrame()
		{
			Theme.Current.Apply(this);
		}
	}
}

