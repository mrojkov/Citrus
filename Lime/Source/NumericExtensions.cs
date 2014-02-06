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

		public static int Sign(this float x)
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

		public static float Truncate(this float x)
		{
			return (float)Math.Truncate(x);
		}

		public static int Round(this float x)
		{
			return (int)(x + 0.5f);
		}

		public static float Floor(this float x)
		{
			return (float)Math.Floor(x);
		}

		public static float Ceiling(this float x)
		{
			return (float)Math.Ceiling(x);
		}
	}
}
