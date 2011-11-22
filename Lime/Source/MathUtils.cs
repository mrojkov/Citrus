using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public static partial class Utils
	{
		public static readonly Random RandomGenerator = new Random ();
		public const float Pi = 3.141592653f;
		public const float DegreesToRadians = Pi / 180;
		public const float RadiansToDegrees = 180 / Pi;

		static bool sinTableBuilt = false;
		static Vector2 [] sinTable0 = new Vector2 [256];
		static Vector2 [] sinTable1 = new Vector2 [256];

		static void InitSinTable ()
		{
			float t1 = 2 * Pi / 256;
			float t2 = t1 / 256;
			for (int i = 0; i < 256; i++) {
				sinTable0 [i] = new Vector2 ((float)Math.Cos (i * t1), (float)Math.Sin (i * t1));
				sinTable1 [i] = new Vector2 ((float)Math.Cos (i * t2), (float)Math.Sin (i * t2));
			}
		}

		public static Vector2 CosSin (float x)
		{
			if (!sinTableBuilt) {
				sinTableBuilt = true;
				InitSinTable ();
			}
			const float t = 65536 / (2 * Pi);
			int index = (int)(x * t) & 65535;
			var a = sinTable0 [index >> 8];
			var b = sinTable1 [index & 255];
			Vector2 result;
			result.X = a.X * b.X - a.Y * b.Y;
			result.Y = a.Y * b.X + a.X * b.Y;
			return result;
		}

		public static int Random (int maxValue)
		{
			return RandomGenerator.Next (maxValue);
		}

		public static bool RandomFlag ()
		{
			return RandomGenerator.Next (2) == 0;
		}

		public static float Random ()
		{
			return (float)RandomGenerator.NextDouble ();
		}
		
		public static bool IsPowerOf2 (int value)
		{
			return value == 1 || (value & (value - 1)) == 0;
		}

		public static int NearestPowerOf2 (int value)
		{
			if (!IsPowerOf2 (value)) {
				int i = 1;
				while (i < value)
					i *= 2;
				return i;
			}
			return value;
		}
		
		public static float ClipAboutZero (float value, float eps = 0.0001f)
		{
			if (value > -eps && value < eps)
				return eps < 0 ? -eps : eps;
			else
				return value;
		}

		public static float Clamp (float value, float min, float max)
		{
			return (value < min) ? min : (value > max ? max : value);
		}

		public static int Clamp (int value, int min, int max)
		{
			return (value < min) ? min : (value > max ? max : value);
		}

		public static Vector2 HermiteSpline (float t, Vector2 p0, Vector2 m0, Vector2 p1, Vector2 m1)
		{
			float t2 = t * t;
			float t3 = t2 * t;
			return (2.0f * t3 - 3.0f * t2 + 1.0f) * p0 + (t3 - 2.0f * t2 + t) * m0 +
				(-2.0f * t3 + 3.0f * t2) * p1 + (t3 - t2) * m1;
		}

		public static Vector2 CatmullRomSpline (float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
		{
			float t2 = t * t;
			float t3 = t2 * t;
			return p1 + 0.5f * (
				(p2 - p0) * t +
				(2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t2 +
				(3.0f * p1 - p0 - 3.0f * p2 + p3) * t3);
		}
	}
}