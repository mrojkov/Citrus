using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	public static class ClampExtensions
	{
		public static int Clamp(this int value, int min, int max)
		{
			return (value < min) ? min : (value > max ? max : value);
		}

		public static float Clamp(this float value, float min, float max)
		{
			return (value < min) ? min : (value > max ? max : value);
		}
	}
}
