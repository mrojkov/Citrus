using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Lime
{
	public static partial class Mathf
	{
		public static System.Random RandomGenerator = new System.Random();

		public const float ZeroTolerance = 1e-6f; // Value a 8x higher than 1.19209290E-07F

		public const float Pi = 3.141592653f;
		public const float TwoPi = 2 * 3.141592653f;
		public const float HalfPi = 3.141592653f / 2;
		public const float DegToRad = Pi / 180;
		public const float RadToDeg = 180 / Pi;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Max(float x, float y) => (x > y) ? x : y;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Min(float x, float y) => (x < y) ? x : y;

		/// <summary>
		/// Returns absolute value of <paramref name="v"/>. Equals to -<paramref name="v"/> if <paramref name="v"/> less than 0 or <paramref name="v"/> otherwise.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Abs(float v) => Math.Abs(v);

		/// <summary>
		///  Returns <see cref="Vector2"/> with absolute values of corresponding components of <paramref name="v"/>
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static Vector2 Abs(Vector2 v) => new Vector2(Abs(v.X), Abs(v.Y));

		/// <summary>
		///  Returns <see cref="Vector3"/> with absolute values of corresponding components of <paramref name="v"/>
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static Vector3 Abs(Vector3 v) => new Vector3(Abs(v.X), Abs(v.Y), Abs(v.Z));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Sign(float x) => Math.Sign(x);

		public static Vector2 Sign(Vector2 x) => new Vector2(Sign(x.X), Sign(x.Y));

#if iOS
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Cos(float radians) => MathF.Cos(radians);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sin(float radians) => MathF.Sin(radians);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Atan2(Vector2 v) => MathF.Atan2(v.Y, v.X);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Atan2(float y, float x) => MathF.Atan2(y, x);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Asin(float v) => MathF.Asin(v);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Acos(float v) => MathF.Acos(v);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sqrt(float x) => MathF.Sqrt(x);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Exp(float x) => MathF.Exp(x);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Log(float x) => MathF.Log(x);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Pow(float x, float y) => MathF.Pow(x, y);
#else
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Cos(float radians) => (float)Math.Cos(radians);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sin(float radians) => (float)Math.Sin(radians);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Atan2(Vector2 v) => (float)Math.Atan2(v.Y, v.X);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Asin(float v) => (float)Math.Asin(v);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Acos(float v) => (float)Math.Acos(v);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sqrt(float x) => (float)Math.Sqrt(x);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Exp(float x) => (float)Math.Exp(x);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Log(float x) => (float)Math.Log(x);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Pow(float x, float y) => (float)Math.Pow(x, y);
#endif
		/// <summary>
		/// Gauss error function. The maximum error is below 1.5 × 10-7.
		/// </summary>
		public static double Erf(double x)
		{
			// https://en.wikipedia.org/wiki/Error_function
			// https://www.johndcook.com/blog/2009/01/19/stand-alone-error-function-erf/
			// formula 7.1.26 from A&S (https://amzn.to/2ES26NK).
			const double a1 = 0.254829592;
			const double a2 = -0.284496736;
			const double a3 = 1.421413741;
			const double a4 = -1.453152027;
			const double a5 = 1.061405429;
			const double p = 0.3275911;
			var sign = Math.Sign(x);
			x = Math.Abs(x);
			var t = 1.0 / (1.0 + p * x);
			var y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);
			return sign * y;
		}

		public static float Erf(float x) => (float)Erf((double)x);

		public static int Wrap(int x, int lowerBound, int upperBound)
		{
			int range = upperBound - lowerBound + 1;
			x = ((x - lowerBound) % range);
			return x < 0 ? upperBound + 1 + x : lowerBound + x;
		}

		public static float Wrap(float x, float lowerBound, float upperBound)
		{
			if (x < lowerBound) {
				return upperBound - (lowerBound - x) % (upperBound - lowerBound);
			}
			return lowerBound + (x - lowerBound) % (upperBound - lowerBound);
		}

		public static float Wrap360(float angle)
		{
			if ((angle >= 360.0f) || (angle < 0.0f)) {
				angle -= (float)Math.Floor(angle * (1.0f / 360.0f)) * 360.0f;
			}
			return angle;
		}

		public static float Wrap180(float angle)
		{
			angle = (angle + 180) % 360;
			if (angle < 0) {
				angle += 360;
			}
			return angle - 180;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sqr(float x) => x * x;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe float FastInverseSqrt(float x)
		{
			var half = x * 0.5F;
			var i = *(long*)&x;
			i = 0x5f3759df - (i >> 1);
			x = *(float*)&i;
			// Duplicate this line to increase result accuracy
			x = x * (1.5f - (half * x * x));
			x = x * (1.5f - (half * x * x));
			return x;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Lerp(float amount, float value1, float value2) => value1 + (value2 - value1) * amount;

		public static Vector2 Lerp(float amount, Vector2 value1, Vector2 value2) => value1 + (value2 - value1) * amount;

		public static Vector3 Lerp(float amount, Vector3 value1, Vector3 value2) => value1 + (value2 - value1) * amount;

		/// <summary>
		/// Returns a random floating-point number that is within a specified range and produced by specified generator.
		/// </summary>
		/// <param name="rng">The instance of a random generator.</param>
		/// <param name="min">The inclusive lower bound of the random number returned.</param>
		/// <param name="max">The exclusive upper bound of the random number returned.</param>
		/// <returns>A single-precision floating point number that is greater than or equal to min,
		/// and less than max.</returns>
		public static float RandomFloat(this System.Random rng, float min, float max) => rng.RandomFloat() * (max - min) + min;

		public static double RandomDouble(this System.Random rng, double min, double max) => rng.NextDouble() * (max - min) + min;

		public static float CalcDistanceToSegment(Vector2 start, Vector2 end, Vector2 point)
		{
			var d = end - start;
			if (d.Length < ZeroTolerance) {
				return (point - start).Length;
			}
			var dir = d.Normalized;
			var f = Vector2.DotProduct(dir, point - start);
			var offset = Mathf.Clamp(f / d.Length, 0f, 1f);
			var projPoint = start + offset * d;
			return (projPoint - point).Length;
		}

		/// <summary>
		/// Returns a random floating-point number that is within a specified range.
		/// </summary>
		/// <param name="min">The inclusive lower bound of the random number returned.</param>
		/// <param name="max">The exclusive upper bound of the random number returned.</param>
		/// <returns>A single-precision floating point number that is greater than or equal to min,
		/// and less than max.</returns>
		public static float RandomFloat(float min, float max) => RandomGenerator.RandomFloat(min, max);

		public static double RandomDouble(double min, double max) => RandomGenerator.RandomDouble(min, max);

		public static bool RandomBool(this System.Random rng) => rng.RandomInt(2) == 0;

		public static bool RandomBool() => RandomGenerator.RandomBool();

		/// <summary>
		/// Returns a random integer that is within a specified range and produced by specified generator.
		/// </summary>
		/// <param name="rng">The instance of a random generator.</param>
		/// <param name="min">The inclusive lower bound of the random number returned.</param>
		/// <param name="max">The inclusive upper bound of the random number returned.
		/// max must be greater than or equal to (min - 1).</param>
		/// <returns>A 32-bit signed integer greater than or equal to min and not greater than max;
		/// that is, the range of return values includes both min and max.
		/// If max equals min or (min - 1), min is returned.</returns>
		public static int RandomInt(this System.Random rng, int min, int max) => rng.RandomInt(max - min + 1) + min;

		/// <summary>
		/// Returns a random integer that is within a specified range.
		/// </summary>
		/// <param name="min">The inclusive lower bound of the random number returned.</param>
		/// <param name="max">The inclusive upper bound of the random number returned.
		/// max must be greater than or equal to (min - 1).</param>
		/// <returns>A 32-bit signed integer greater than or equal to min and not greater than max;
		/// that is, the range of return values includes both min and max.
		/// If max equals min or (min - 1), min is returned.</returns>
		public static int RandomInt(int min, int max) => RandomGenerator.RandomInt(min, max);

		public static T RandomOf<T>(this System.Random rng, params T[] objects) => objects[rng.RandomInt(objects.Length)];

		public static T RandomOf<T>(params T[] objects) => RandomGenerator.RandomOf(objects);

		public static T RandomOf<T>(this System.Random rng, ICollection<T> objects)
		{
			return objects.ElementAt(rng.Next(objects.Count));
		}

		public static T RandomItem<T>(this ICollection<T> objects)
		{
			return RandomOf(RandomGenerator, objects);
		}

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

		/// <summary>
		/// Returns a non-negative random integer that is less than the specified maximum and
		/// produced by specified generator.
		/// </summary>
		/// <param name="rng">The instance of a random generator.</param>
		/// <param name="maxValue">The exclusive upper bound of the random number to be generated.
		/// maxValue must be greater than or equal to 0.</param>
		/// <returns>A 32-bit signed integer that is greater than or equal to 0, and less than maxValue;
		/// that is, the range of return values ordinarily includes 0 but not maxValue.
		/// However, if maxValue equals 0, maxValue is returned.</returns>
		public static int RandomInt(this System.Random rng, int maxValue) => rng.Next(maxValue);

		/// <summary>
		/// Returns a non-negative random integer that is less than the specified maximum.
		/// </summary>
		/// <param name="maxValue">The exclusive upper bound of the random number to be generated.
		/// maxValue must be greater than or equal to 0.</param>
		/// <returns>A 32-bit signed integer that is greater than or equal to 0, and less than maxValue;
		/// that is, the range of return values ordinarily includes 0 but not maxValue.
		/// However, if maxValue equals 0, maxValue is returned.</returns>
		public static int RandomInt(int maxValue) => RandomGenerator.RandomInt(maxValue);

		/// <summary>
		/// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0 and
		/// produced by specified generator.
		/// </summary>
		/// <param name="rng">The instance of a random generator.</param>
		/// <returns>A single-precision floating point number that is greater than or equal to 0.0,
		/// and less than 1.0.</returns>
		/// <remarks>The actual upper bound of the random number returned by this method is 0.99999999999999978.
		/// </remarks>
		public static float RandomFloat(this System.Random rng) => (float)rng.NextDouble();

		/// <summary>
		/// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
		/// </summary>
		/// <returns>A single-precision floating point number that is greater than or equal to 0.0,
		/// and less than 1.0.</returns>
		/// <remarks>The actual upper bound of the random number returned by this method is 0.99999999999999978.
		/// </remarks>
		public static float RandomFloat() => RandomGenerator.RandomFloat();

		public static float NormalRandom(this System.Random rng, float median, float dispersion)
		{
			if (dispersion == 0.0f) {
				return median;
			}
			return median + dispersion *
				Sqrt(-2.0f * Log(rng.RandomFloat())) *
				Sin(2.0f * Pi * rng.RandomFloat());
		}

		public static float NormalRandom(float median, float dispersion) => RandomGenerator.NormalRandom(median, dispersion);

		public static float UniformRandom(this System.Random rng, float median, float dispersion) => median + (rng.RandomFloat() - 0.5f) * dispersion;

		public static float UniformRandom(float median, float dispersion) => RandomGenerator.UniformRandom(median, dispersion);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool InRange(float x, float upper, float lower) => lower <= x && x <= upper;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Clamp(float value, float min, float max) => (value < min) ? min : (value > max ? max : value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Clamp(int value, int min, int max) => (value < min) ? min : (value > max ? max : value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double Clamp(double value, double min, double max) => (value < min) ? min : (value > max ? max : value);

		public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max) => new Vector2(
			Clamp(value.X, min.X, max.X),
			Clamp(value.Y, min.Y, max.Y)
		);

		public static Vector2 HermiteSpline(float t, Vector2 p0, Vector2 m0, Vector2 p1, Vector2 m1) => new Vector2(
			HermiteSpline(t, p0.X, m0.X, p1.X, m1.X),
			HermiteSpline(t, p0.Y, m0.Y, p1.Y, m1.Y));

		public static Vector2 HermiteSplineDerivative(float t, Vector2 p0, Vector2 m0, Vector2 p1, Vector2 m1) => new Vector2(
			HermiteSplineDerivative(t, p0.X, m0.X, p1.X, m1.X),
			HermiteSplineDerivative(t, p0.Y, m0.Y, p1.Y, m1.Y));

		public static Vector3 HermiteSpline(float t, Vector3 p0, Vector3 m0, Vector3 p1, Vector3 m1) => new Vector3(
			HermiteSpline(t, p0.X, m0.X, p1.X, m1.X),
			HermiteSpline(t, p0.Y, m0.Y, p1.Y, m1.Y),
			HermiteSpline(t, p0.Z, m0.Z, p1.Z, m1.Z));

		public static float HermiteSpline(float t, float p0, float m0, float p1, float m1)
		{
			float t2 = t * t;
			float t3 = t2 * t;
			return (2.0f * t3 - 3.0f * t2 + 1.0f) * p0 + (t3 - 2.0f * t2 + t) * m0 +
				(-2.0f * t3 + 3.0f * t2) * p1 + (t3 - t2) * m1;
		}

		public static float HermiteSplineDerivative(float t, float p0, float m0, float p1, float m1)
		{
			float t2 = t * t;
			return (6 * t2 - 6 * t) * p0 +
				(3 * t2 - 4 * t + 1) * m0 +
				(-6 * t2 + 6 * t) * p1 +
				(3 * t2 - 2 * t) * m1;
		}

		public static Vector2 CatmullRomSpline(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3) => new Vector2(
			CatmullRomSpline(t, p0.X, p1.X, p2.X, p3.X),
			CatmullRomSpline(t, p0.Y, p1.Y, p2.Y, p3.Y)
		);

		public static Vector3 CatmullRomSpline(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) => new Vector3(
			CatmullRomSpline(t, p0.X, p1.X, p2.X, p3.X),
			CatmullRomSpline(t, p0.Y, p1.Y, p2.Y, p3.Y),
			CatmullRomSpline(t, p0.Z, p1.Z, p2.Z, p3.Z)
		);

		public static float CatmullRomSpline(float t, float p0, float p1, float p2, float p3)
		{
			float t2 = t * t;
			float t3 = t2 * t;
			return p1 + 0.5f * (
				(p2 - p0) * t +
				(2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t2 +
				(3.0f * p1 - p0 - 3.0f * p2 + p3) * t3);
		}

		public static Vector3 BezierSpline(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) => new Vector3 {
			X = BezierSpline(t, p0.X, p1.X, p2.X, p3.X),
			Y = BezierSpline(t, p0.Y, p1.Y, p2.Y, p3.Y),
			Z = BezierSpline(t, p0.Z, p1.Z, p2.Z, p3.Z),
		};

		public static Vector2 BezierSpline(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3) => new Vector2 {
			X = BezierSpline(t, p0.X, p1.X, p2.X, p3.X),
			Y = BezierSpline(t, p0.Y, p1.Y, p2.Y, p3.Y)
		};

		public static float BezierSpline(float t, float p0, float p1, float p2, float p3)
		{
			var oneMinusT = 1 - t;
			var oneMinusT2 = oneMinusT * oneMinusT;
			var oneMinusT3 = oneMinusT2 * oneMinusT;
			var t2 = t * t;
			var t3 = t2 * t;
			return oneMinusT3 * p0 + 3 * t * oneMinusT2 * p1 + 3 * t2 * oneMinusT * p2 + t3 * p3;
		}

		public static Vector3 BezierTangent(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) => new Vector3(
			BezierTangent(t, p0.X, p1.X, p2.X, p3.X),
			BezierTangent(t, p0.Y, p1.Y, p2.Y, p3.Y),
			BezierTangent(t, p0.Z, p1.Z, p2.Z, p3.Z));

		public static Vector2 BezierTangent(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3) => new Vector2(
			BezierTangent(t, p0.X, p1.X, p2.X, p3.X),
			BezierTangent(t, p0.Y, p1.Y, p2.Y, p3.Y));

		public static float BezierTangent(float t, float p0, float p1, float p2, float p3)
		{
			var oneMinusT = 1 - t;
			var oneMinusT2 = oneMinusT * oneMinusT;
			var t2 = t * t;
			return
				-3 * oneMinusT2 * p0 +
				3 * oneMinusT2 * p1 -
				6 * t * oneMinusT * p1 -
				3 * t2 * p2 +
				6 * t * oneMinusT * p2 +
				3 * t2 * p3;
		}
	}
}
