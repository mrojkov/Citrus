using System;
using XwtPlus;

namespace XwtPlus.GtkBackend
{
	public class GtkEngine : Xwt.GtkBackend.GtkEngine
	{
		public override void InitializeApplication ()
		{
			base.InitializeApplication ();
			RegisterBackend<ICommandBackend, CommandBackend> ();
			RegisterBackend<IMenuItemBackend, MenuItemBackend>();
			RegisterBackend<IUtilsBackend, UtilsBackend>();
		}
	}
}

