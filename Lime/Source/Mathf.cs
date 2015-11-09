using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	/// <summary>
	/// Математические функции для чисел типа float (аналогично классу Math)
	/// </summary>
	public static class Mathf
	{
		public static System.Random RandomGenerator = new System.Random();

		public const float ZeroTolerance = 1e-6f; // Value a 8x higher than 1.19209290E-07F

		/// <summary>
		/// Возвращает число Пи
		/// </summary>
		public const float Pi = 3.141592653f;

		/// <summary>
		/// Возвращает Пи * 2
		/// </summary>
		public const float TwoPi = 2 * 3.141592653f;

		/// <summary>
		/// Возвращает Пи / 2
		/// </summary>
		public const float HalfPi = 3.141592653f / 2;

		/// <summary>
		/// Для перевода градусов в радианы умножайте на это число
		/// </summary>
		public const float DegToRad = Pi / 180;

		[Obsolete("Use DegToRad instead", true)]
		public const float DegreesToRadians = DegToRad;

		/// <summary>
		/// Для перевода радиан в градусы умножайте на это число
		/// </summary>
		public const float RadToDeg = 180 / Pi;

		[Obsolete("Use RadToDeg instead", true)]
		public const float RadiansToDegrees = RadToDeg;

		public static float Max(float x, float y)
		{
			return (x > y) ? x : y;
		}

		public static float Min(float x, float y)
		{
			return (x < y) ? x : y;
		}

		public static float Abs(float x)
		{
			return Math.Abs(x);
		}

		public static float Abs(Vector2 x)
		{
			return x.Length;
		}

		public static float Abs(Vector3 x)
		{
			return x.Length;
		}

		public static int Sign(float x)
		{
			return Math.Sign(x);
		}

		public static float Cos(float radians)
		{
			return (float)Math.Cos(radians);
		}

		public static float Sin(float radians)
		{
			return (float)Math.Sin(radians);
		}

		public static float Atan2(Vector2 v)
		{
			return (float)Math.Atan2(v.Y, v.X);
		}

		public static int Wrap(int x, int lowerBound, int upperBound)
		{
			int range = upperBound - lowerBound + 1;
			x = ((x - lowerBound) % range);
			if (x < 0) {
				return upperBound + 1 + x;
			} else {
				return lowerBound + x;
			}
		}

		public static float Wrap360(float angle)
		{
			if ((angle >= 360.0f) || (angle < 0.0f)) {
				angle -= (float)Math.Floor(angle * (1.0f / 360.0f)) * 360.0f;
			}
			return angle;
		}

		public static float Sqr(float x)
		{
			return x * x;
		}

		public static float Sqrt(float x)
		{
			return (float)Math.Sqrt(x);
		}

		public static float Dist2(Vector2 a, Vector2 b)
		{
			return Sqr(a.X - b.X) + Sqr(a.Y - b.Y);
		}

		public static float Pow(float x, float y)
		{
			return (float)Math.Pow(x, y);
		}

		public static float Lerp(float amount, float value1, float value2)
		{
			return value1 + (value2 - value1) * amount;
		}

		public static Vector2 Lerp(float amount, Vector2 value1, Vector2 value2)
		{
			return value1 + (value2 - value1) * amount;
		}

		public static float RandomFloat(this System.Random rng, float min, float max)
		{
			return rng.RandomFloat() * (max - min) + min;
		}

		public static float RandomFloat(float min, float max)
		{
			return RandomGenerator.RandomFloat(min, max);
		}

		public static bool RandomBool(this System.Random rng)
		{
			return rng.RandomInt(2) == 0;
		}

		public static bool RandomBool()
		{
			return RandomGenerator.RandomBool();
		}

		public static int RandomInt(this System.Random rng, int min, int max)
		{
			return rng.RandomInt(max - min + 1) + min;
		}

		public static int RandomInt(int min, int max)
		{
			return RandomGenerator.RandomInt(min, max);
		}

		public static T RandomOf<T>(this System.Random rng, params T[] objects)
		{
			return objects[rng.RandomInt(objects.Length)];
		}

		public static T RandomOf<T>(params T[] objects)
		{
			return RandomGenerator.RandomOf(objects);
		}

		/// <summary>
		/// Перечисляет элементы коллекции в случайном порядке
		/// </summary>
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng = null)
		{
			if (rng == null)
				rng = RandomGenerator;
			var elements = source.ToArray();
			for (int i = elements.Length; i > 0; i--) {
				int j = rng.Next(i);
				yield return elements[j];
				elements[j] = elements[i - 1];
			}
		}

		public static int RandomInt(this System.Random rng, int maxValue)
		{
			return rng.Next(maxValue);
		}

		public static int RandomInt(int maxValue)
		{
			return RandomGenerator.RandomInt(maxValue);
		}

		public static float RandomFloat(this System.Random rng)
		{
			return (float)rng.NextDouble();
		}

		public static float RandomFloat()
		{
			return RandomGenerator.RandomFloat();
		}

		public static float NormalRandom(this System.Random rng, float median, float dispersion)
		{
			float x = 0;
			for (int i = 0; i < 12; ++i)
				x += rng.RandomFloat();
			x -= 6;
			return median + x * dispersion;
		}

		public static float NormalRandom(float median, float dispersion)
		{
			return RandomGenerator.NormalRandom(median, dispersion);
		}

		public static float UniformRandom(this System.Random rng, float median, float dispersion)
		{
			return median + (rng.RandomFloat() - 0.5f) * dispersion;
		}

		public static float UniformRandom(float median, float dispersion)
		{
			return RandomGenerator.UniformRandom(median, dispersion);
		}

		public static bool InRange(float x, float upper, float lower)
		{
			return lower <= x && x <= upper;
		}

		public static float Clamp(float value, float min, float max)
		{
			return (value < min) ? min : (value > max ? max : value);
		}

		public static int Clamp(int value, int min, int max)
		{
			return (value < min) ? min : (value > max ? max : value);
		}

		public static Vector2 HermiteSpline(float t, Vector2 p0, Vector2 m0, Vector2 p1, Vector2 m1)
		{
			return new Vector2(HermiteSpline(t, p0.X, m0.X, p1.X, m1.X),
				HermiteSpline(t, p0.Y, m0.Y, p1.Y, m1.Y));
		}

		public static float HermiteSpline(float t, float p0, float m0, float p1, float m1)
		{
			float t2 = t * t;
			float t3 = t2 * t;
			return (2.0f * t3 - 3.0f * t2 + 1.0f) * p0 + (t3 - 2.0f * t2 + t) * m0 +
				(-2.0f * t3 + 3.0f * t2) * p1 + (t3 - t2) * m1;
		}

		public static Vector2 CatmullRomSpline(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
		{
			return new Vector2(
				CatmullRomSpline(t, p0.X, p1.X, p2.X, p3.X),
				CatmullRomSpline(t, p0.Y, p1.Y, p2.Y, p3.Y)
			);
		}

		public static Vector3 CatmullRomSpline(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
		{
			return new Vector3(
				CatmullRomSpline(t, p0.X, p1.X, p2.X, p3.X),
				CatmullRomSpline(t, p0.Y, p1.Y, p2.Y, p3.Y),
				CatmullRomSpline(t, p0.Z, p1.Z, p2.Z, p3.Z)
			);
		}

		public static float CatmullRomSpline(float t, float p0, float p1, float p2, float p3)
		{
			float t2 = t * t;
			float t3 = t2 * t;
			return p1 + 0.5f * (
				(p2 - p0) * t +
				(2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t2 +
				(3.0f * p1 - p0 - 3.0f * p2 + p3) * t3);
		}

		[Obsolete("Use Vector2.CosSinRough(float) instead", true)]
		public static Vector2 CosSin(float radians)
		{
			return Vector2.HeadingRad(radians);
		}
	}
}
