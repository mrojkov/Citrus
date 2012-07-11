using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public static class Mathf
	{
		public static readonly Random RandomGenerator = new Random();
		public const float Pi = 3.141592653f;
		public const float Pi2 = 2 * 3.141592653f;
		public const float DegreesToRadians = Pi / 180;
		public const float RadiansToDegrees = 180 / Pi;

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

		public static float Cos(float x)
		{
			return (float)Math.Cos(x);
		}

		public static float Sin(float x)
		{
			return (float)Math.Sin(x);
		}

		public static float Sqr(float x)
		{
			return x * x;
		}

		public static float Sqrt(float x)
		{
			return (float)Math.Sqrt(x);
		}

		public static float Pow(float x, float y)
		{
			return (float)Math.Pow(x, y);
		}

		public static float Lerp(float t, float a, float b)
		{
			return a + (b - a) * t;
		}

		public static T Random<T>(params T[] objects)
		{
			return objects[Random(objects.Length)];
		}

		public static int Random(int maxValue)
		{
			return RandomGenerator.Next(maxValue);
		}

		public static float Random()
		{
			return (float)RandomGenerator.NextDouble();
		}

		public static float NormalRandom(float median, float dispersion)
		{
			float x = 0;
			for (int i = 0; i < 12; ++i)
				x += Random();
			x -= 6;
			return median + x * dispersion;
		}

		public static float UniformRandom(float median, float dispersion)
		{
			return median + (Random() - 0.5f) * dispersion;
		}

		public static float Clamp(float value, float min, float max)
		{
			return (value < min) ? min : (value > max ? max : value);
		}

		public static int Clamp(int value, int min, int max)
		{
			return (value < min) ? min : (value > max ? max : value);
		}

		public static void Clamp(ref float value, float min, float max)
		{
			value = (value < min) ? min : (value > max ? max : value);
		}

		public static void Clamp(ref int value, int min, int max)
		{
			value = (value < min) ? min : (value > max ? max : value);
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
			return new Vector2(CatmullRomSpline(t, p0.X, p1.X, p2.X, p3.X),
				CatmullRomSpline(t, p0.Y, p1.Y, p2.Y, p3.Y));
		}
		
		public static Vector2 CatmullRomJoinedSpline(float t, params Vector2[] points)
		{
			int segCount = points.Length - 3;
			if (segCount < 1) {
				throw new System.ArgumentException("Not enough spline knots");
			}
			while (t < 0) { t++; }
			while (t >= 1) { t--; }
			int curSeg = (int)(t * segCount);
			float k = t * segCount - curSeg;
			return CatmullRomSpline(k, points[curSeg], points[curSeg + 1], points[curSeg + 2], points[curSeg + 3]);
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

		public static Vector2 CosSin(float x)
		{
			if (sinTable0 == null) {
				BuildSinTable();
			}
			const float t = 65536 / (2 * Pi);
			int index = (int)(x * t) & 65535;
			var a = sinTable0[index >> 8];
			var b = sinTable1[index & 255];
			Vector2 result;
			result.X = a.X * b.X - a.Y * b.Y;
			result.Y = a.Y * b.X + a.X * b.Y;
			return result;
		}

		static Vector2[] sinTable0;
		static Vector2[] sinTable1;

		private static void BuildSinTable()
		{
			sinTable0 = new Vector2[256];
			sinTable1 = new Vector2[256];
			float t1 = 2 * Pi / 256;
			float t2 = t1 / 256;
			for (int i = 0; i < 256; i++) {
				sinTable0[i] = new Vector2((float)Math.Cos(i * t1), (float)Math.Sin(i * t1));
				sinTable1[i] = new Vector2((float)Math.Cos(i * t2), (float)Math.Sin(i * t2));
			}
		}
	}
}