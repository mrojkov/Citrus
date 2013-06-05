using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public static class NumericExtensions
	{
		public static int Wrap(this int x, int lowerBound, int upperBound)
		{
			return Mathf.Wrap(x, lowerBound, upperBound);
		}

		public static float Abs(this float x)
		{
			return Math.Abs(x);
		}

		public static float Sqr(this float x)
		{
			return x * x;
		}

		public static float Pow(this float x, float y)
		{
			return (float)Math.Pow(x, y);
		}

		public static float Sqrt(this float x)
		{
			return Mathf.Sqrt(x);
		}

		public static float Sign(this float x)
		{
			return Math.Sign(x);
		}

		public static float Wrap360(this float x)
		{
			return Mathf.Wrap360(x);
		}

		public static float Clamp(this float value, float min, float max)
		{
			return Mathf.Clamp(value, min, max);
		}

		public static int Clamp(this int value, int min, int max)
		{
			return Mathf.Clamp(value, min, max);
		}

		public static int Truncate(this float x)
		{
			return (int)x;
		}

		public static int Round(this float x)
		{
			return (int)Math.Round(x);
		}
	}
}
