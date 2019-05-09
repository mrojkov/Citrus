using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Lime
{
	public static class NumericExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Wrap(this int x, int lowerBound, int upperBound)
		{
			return Mathf.Wrap(x, lowerBound, upperBound);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Max(this float a, float b)
		{
			return a > b ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Min(this float a, float b)
		{
			return a < b ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Max(this int a, int b)
		{
			return a > b ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Min(this int a, int b)
		{
			return a < b ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Abs(this float x)
		{
			return x < 0 ? -x : x;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Abs(this int x)
		{
			return x < 0 ? -x : x;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Abs(this long x)
		{
			return x < 0 ? -x : x;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sqr(this float x)
		{
			return x * x;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Pow(this float x, float y)
		{
			return Mathf.Pow(x, y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sqrt(this float x)
		{
			return Mathf.Sqrt(x);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Sign(this float x)
		{
			return Math.Sign(x);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Sign(this int x)
		{
			return Math.Sign(x);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Sign(this long x)
		{
			return Math.Sign(x);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Wrap360(this float x)
		{
			return Mathf.Wrap360(x);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool InRange(this float x, float upper, float lower)
		{
			return Mathf.InRange(x, lower, upper);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Clamp(this float value, float min, float max)
		{
			return Mathf.Clamp(value, min, max);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Clamp(this int value, int min, int max)
		{
			return Mathf.Clamp(value, min, max);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double Clamp(this double value, double min, double max)
		{
			return Mathf.Clamp(value, min, max);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Truncate(this float x)
		{
			return (int)Math.Truncate(x);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Round(this float x)
		{
			return (int)(x + ((x > 0) ? 0.5f : -0.5f));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Round(this double x)
		{
			return (int)(x + ((x > 0) ? 0.5d : -0.5d));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Floor(this float x)
		{
			return (int)Math.Floor(x);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Ceiling(this float x)
		{
			return (int)Math.Ceiling(x);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Lerp(this float amount, float value1, float value2)
		{
			return Mathf.Lerp(amount, value1, value2);
		}

		private static readonly NumberFormatInfo brFormat = new NumberFormatInfo { NumberGroupSeparator = "." };
		private static readonly NumberFormatInfo defaultFormat = new NumberFormatInfo
		{
			// no-break space separator
			NumberGroupSeparator = "\u00A0",
		};

		public static string Localize(this int value, string format = "N0")
		{
			if (Application.CurrentCultureInfo != null) {
				return value.ToString(format, Application.CurrentCultureInfo.NumberFormat);
			}

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
				case "TW":
					return value.ToString(format);
				case "BR":
					return value.ToString(format, brFormat);
				default:
					return value.ToString(format, defaultFormat);
			}
		}

		public static string Localize(this float value, string format = "")
		{
			if (Application.CurrentCultureInfo != null) {
				return value.ToString(format, Application.CurrentCultureInfo.NumberFormat);
			} else {
				return value.ToString(format);
			}
		}
	}
}
