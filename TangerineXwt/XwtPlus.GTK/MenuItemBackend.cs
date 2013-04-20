using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XwtPlus.GtkBackend
{
	public class MenuItemBackend : Xwt.GtkBackend.MenuItemBackend, IMenuItemBackend
	{
		object IMenuItemBackend.GetNativeMenuItem()
		{
			return MenuItem;
		}
	}
}
