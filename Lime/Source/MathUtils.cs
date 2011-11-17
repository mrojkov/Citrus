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

		static bool sinTableReady = false;
		static float [] CosTab0 = new float [256];
		static float [] SinTab0 = new float [256];
		static float [] CosTab1 = new float [256];
		static float [] SinTab1 = new float [256];

		static void InitSinTable ()
		{
			float t1 = 2 * Pi / 256;
			float t2 = t1 / 256;
			for (int i = 0; i < 256; i++) {
				CosTab0 [i] = (float)Math.Cos (i * t1);
				SinTab0 [i] = (float)Math.Sin (i * t1);
				CosTab1 [i] = (float)Math.Cos (i * t2);
				SinTab1 [i] = (float)Math.Sin (i * t2);
			}
		}

		public static void FastSinCos (float x, out float sin, out float cos)
		{
			if (!sinTableReady) {
				sinTableReady = true;
				InitSinTable ();
			}
			const float t = 65536 / (2 * Pi);
			int index = (int)(x * t) & 65535;
			int a = index >> 8;
			int b = index & 255;
			var sa = SinTab0 [a];
			var ca = CosTab0 [a];
			var sb = SinTab1 [b];
			var cb = CosTab1 [b];
			sin = sa * cb + ca * sb;
			cos = ca * cb - sa * sb;
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