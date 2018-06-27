using System;
using System.Globalization;

namespace Lime
{
	public static class NumericExtensions
	{
		public static int Wrap(this int x, int lowerBound, int upperBound)
		{
			return Mathf.Wrap(x, lowerBound, upperBound);
		}

		public static float Max(this float a, float b)
		{
			return a > b ? a : b;
		}

		public static float Min(this float a, float b)
		{
			return a < b ? a : b;
		}

		public static int Max(this int a, int b)
		{
			return a > b ? a : b;
		}

		public static int Min(this int a, int b)
		{
			return a < b ? a : b;
		}

		public static float Abs(this float x)
		{
			return x < 0 ? -x : x;
		}

		public static int Abs(this int x)
		{
			return x < 0 ? -x : x;
		}

		public static long Abs(this long x)
		{
			return x < 0 ? -x : x;
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

		public static int Sign(this int x)
		{
			return Math.Sign(x);
		}

		public static int Sign(this long x)
		{
			return Math.Sign(x);
		}

		public static float Wrap360(this float x)
		{
			return Mathf.Wrap360(x);
		}

		public static bool InRange(this float x, float upper, float lower)
		{
			return Mathf.InRange(x, lower, upper);
		}

		public static float Clamp(this float value, float min, float max)
		{
			return Mathf.Clamp(value, min, max);
		}

		public static int Clamp(this int value, int min, int max)
		{
			return Mathf.Clamp(value, min, max);
		}

		public static double Clamp(this double value, double min, double max)
		{
			return Mathf.Clamp(value, min, max);
		}

		public static int Truncate(this float x)
		{
			return (int)Math.Truncate(x);
		}

		public static int Round(this float x)
		{
			return (int)(x + ((x > 0) ? 0.5f : -0.5f));
		}

		public static int Round(this double x)
		{
			return (int)(x + ((x > 0) ? 0.5d : -0.5d));
		}

		public static int Floor(this float x)
		{
			return (int)Math.Floor(x);
		}

		public static int Ceiling(this float x)
		{
			return (int)Math.Ceiling(x);
		}

		public static float Lerp(this float amount, float value1, float value2)
		{
			return Mathf.Lerp(amount, value1, value2);
		}

		private static readonly NumberFormatInfo brFormat = new NumberFormatInfo { NumberGroupSeparator = "." };
		private static readonly NumberFormatInfo defaultFormat = new NumberFormatInfo { NumberGroupSeparator = " " };

		public static string Localize(this int value)
		{
			if (value > -1000 && value < 1000) {
				return value.ToString();
			}

			switch (AssetBundle.CurrentLanguage) {
				case null:
				case "":
				case "EN":
				case "JP":
				case "KR":
				case "CN":
					return value.ToString("N0");
				case "BR":
					return value.ToString("N0", brFormat);
				default:
					return value.ToString("N0", defaultFormat);
			}
		}
	}
}
