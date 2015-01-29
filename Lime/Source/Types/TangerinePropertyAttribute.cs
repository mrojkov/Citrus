using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class TangerinePropertyAttribute : Attribute
	{
		public int KeyColor;

		public static readonly TangerinePropertyAttribute Null = new TangerinePropertyAttribute(0);

		public TangerinePropertyAttribute(int keyColor)
		{
			this.KeyColor = keyColor;
		}
	}
}
