using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	public static class The
	{
		public static Workspace Workspace { get { return Workspace.Instance; } }
		public static UserInterface UI { get { return UserInterface.Instance; } }
		public static MenuController MenuController { get { return MenuController.Instance; } }
	}
}
