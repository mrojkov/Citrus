using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XwtPlus
{
	public interface IMenuItemBackend : Xwt.Backends.IMenuItemBackend
	{
		object GetNativeMenuItem();
	}

	[Xwt.Backends.BackendType(typeof(IMenuItemBackend))]
	public class MenuItem : Xwt.MenuItem
	{
		public IMenuItemBackend Backend
		{
			get { return (IMenuItemBackend)BackendHost.Backend; }
		}
	}
}
