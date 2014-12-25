using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public static class Mathf
	{
		public static readonly System.Random RandomGenerator = new System.Random();
		public const float Pi = 3.141592653f;
		public const float TwoPi = 2 * 3.141592653f;
		public const float HalfPi = 3.141592653f / 2;
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

		public static float Lerp(float t, float a, float b)
		{
			return a + (b - a) * t;
		}

		public static Vector2 Lerp(float t, Vector2 a, Vector2 b)
		{
			return a + (b - a) * t;
		}

		public static float RandomFloat(float min, float max)
		{
			return RandomFloat() * (max - min) + min;
		}

		public static bool RandomBool()
		{
			return RandomInt(2) == 0;
		}

		public static int RandomInt(int min, int max)
		{
			return RandomInt(max - min + 1) + min;
		}

		public static T RandomOf<T>(params T[] objects)
		{
			return objects[RandomInt(objects.Length)];
		}

		public static int RandomInt(int maxValue)
		{
			return RandomGenerator.Next(maxValue);
		}

		public static float RandomFloat()
		{
			return (float)RandomGenerator.NextDouble();
		}

		public static float NormalRandom(float median, float dispersion)
		{
			float x = 0;
			for (int i = 0; i < 12; ++i)
				x += RandomFloat();
			x -= 6;
			return median + x * dispersion;
		}

		public static float UniformRandom(float median, float dispersion)
		{
			return median + (RandomFloat() - 0.5f) * dispersion;
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
			return new Vector2(CatmullRomSpline(t, p0.X, p1.X, p2.X, p3.X),
				CatmullRomSpline(t, p0.Y, p1.Y, p2.Y, p3.Y));
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

		public static Vector2 CosSin(float radians)
		{
			if (sinTable0 == null) {
				BuildSinTable();
			}
			const float t = 65536 / (2 * Pi);
			int index = (int)(radians * t) & 65535;
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