using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public static class Input
	{
		public static Matrix32 ScreenToWorldTransform = Matrix32.Identity;
		public static Mouse Mouse = new Mouse ();
		public static Keyboard Keyboard = new Keyboard ();
	}
}
