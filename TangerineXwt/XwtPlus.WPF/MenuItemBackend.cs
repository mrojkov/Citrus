using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XwtPlus.WPFBackend
{
	public class MenuItemBackend : Xwt.WPFBackend.MenuItemBackend, IMenuItemBackend
	{
		object IMenuItemBackend.GetNativeMenuItem()
		{
			return MenuItem;
		}
	}
}
