using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class TangerinePropertyAttribute : Attribute
	{
		public string ColorName;

		public TangerinePropertyAttribute(string colorName)
		{
			this.ColorName = colorName;
		}
	}
}
