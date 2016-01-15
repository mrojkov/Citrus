using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public struct ColorGradient
	{
		public Color4 A;
		public Color4 B;

		public ColorGradient(Color4 a)
		{
			A = B = a;
		}

		public ColorGradient(Color4 a, Color4 b)
		{
			A = a; B = b;
		}
	}
}
