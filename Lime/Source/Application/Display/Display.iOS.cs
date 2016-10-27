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

		public Vector2 Size => new Size(screen.Bounds.Width, screen.Bounds.Height);
	}
}
#endif
