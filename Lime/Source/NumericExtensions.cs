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

		public static float Max(this float a, float b)
		{
			return a > b ? a : b;
		}

		public static float Min(this float a, float b)
		{
			return a < b ? a : b;
		}

		public static float Max(this int a, int b)
		{
			return a > b ? a : b;
		}

		public static float Min(this int a, int b)
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

		public static int Truncate(this float x)
		{
			return (int)Math.Truncate(x);
		}

		public static int Round(this float x)
		{
			return (int)(x + ((x > 0) ? 0.5f : -0.5f));
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

		/// <summary>
		/// Применяет функцию локализации к указаному числу, возвращает результат в виде строки.
		/// На разных языках числа записываются в разном формате (особенно касается знака, отделяющего целую и дробную часть)
		/// </summary>
		public static string Localize(this int value)
		{
			string result = value.ToString("N0");
			if (
				string.IsNullOrEmpty(AssetsBundle.CurrentLanguage) 
				|| AssetsBundle.CurrentLanguage == "EN"
				|| AssetsBundle.CurrentLanguage == "JP"
				|| AssetsBundle.CurrentLanguage == "KR"
				|| AssetsBundle.CurrentLanguage == "CN"
			) {
				return result;
			} else if (AssetsBundle.CurrentLanguage == "BR") {
				return result.Replace(',', '.'); // заменяем запятые на точки
			} else {
				return result.Replace(',', (char)160); // заменяем запятые на неразрывные пробелы
			}
		}
	}
}
