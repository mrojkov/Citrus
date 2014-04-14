using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime.PopupMenu
{
	public class MenuItem
	{
		public static float Height = 40;

		public Frame Frame = new Frame() { Tag = "$MenuItem.cs" };
		public Menu Menu;

		public bool Visible = true;
	}
}
