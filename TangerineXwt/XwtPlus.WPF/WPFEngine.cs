using System;
using System.Windows;
using XwtPlus;

namespace XwtPlus.WPFBackend
{
	public class WPFEngine : Xwt.WPFBackend.WPFEngine
	{
		public override void InitializeApplication()
		{
			base.InitializeApplication();
			RegisterBackend<ICommandBackend, CommandBackend>();
			RegisterBackend<IMenuItemBackend, MenuItemBackend>();
		}
	}
}

