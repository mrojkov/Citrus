#if iOS
using System;

using UIKit;

namespace Lime
{
	internal class Display : IDisplay
	{
		private readonly UIScreen screen;
		
		public Display(UIScreen screen)
		{
			this.screen = screen;
		}

		public Vector2 Position => Vector2.Zero;
		public Vector2 Size => new Vector2((float)screen.Bounds.Width, (float)screen.Bounds.Height);
	}
}
#endif
