using System;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	public static class Toolbox
	{
		public static void Swap<T>(ref T lhs, ref T rhs)
		{
			T temp;
			temp = lhs;
			lhs = rhs;
			rhs = temp;
		}
	}
}