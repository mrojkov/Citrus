using System;

namespace Lime
{
	// See https://easings.net

	public enum EasingFunction
	{
		Linear,
		Quadratic,
		Cubic,
		Quartic,
		Quintic,
		Sine,
		Circular,
		Exponential,
		Elastic,
		Back,
		Bounce,
	}

	public enum EasingType
	{
		In,
		Out,
		InOut
	}

	public static class Easing
	{
		public static float Interpolate(float p, EasingFunction function, EasingType type)
		{
			switch (function) {
				default:
				case EasingFunction.Linear: return Linear(p);
				case EasingFunction.Quadratic: return QuadraticEase(p, type);
				case EasingFunction.Cubic: return CubicEase(p, type);
				case EasingFunction.Quartic: return QuarticEase(p, type);
				case EasingFunction.Quintic: return QuinticEase(p, type);
				case EasingFunction.Sine: return SineEase(p, type);
				case EasingFunction.Circular: return CircularEase(p, type);
				case EasingFunction.Exponential: return ExponentialEase(p, type);
				case EasingFunction.Elastic: return ElasticEase(p, type);
				case EasingFunction.Back: return BackEase(p, type);
				case EasingFunction.Bounce: return BounceEase(p, type);
			}
		}

		public static float Linear(float p)
		{
			return p;
		}

		public static float QuadraticEase(float p, EasingType type)
		{
			if (type == EasingType.In) {
				return QuadraticEaseIn(p);
			} else if (type == EasingType.Out) {
				return QuadraticEaseOut(p);
			} else {
				return QuadraticEaseInOut(p);
			}
		}

		public static float QuadraticEaseIn(float p)
		{
			return p * p;
		}

		public static float QuadraticEaseOut(float p)
		{
			return -(p * (p - 2));
		}

		public static float QuadraticEaseInOut(float p)
		{
			if (p < 0.5f) {
				return 2 * p * p;
			} else {
				return (-2 * p * p) + (4 * p) - 1;
			}
		}

		public static float CubicEase(float p, EasingType type)
		{
			if (type == EasingType.In) {
				return CubicEaseIn(p);
			} else if (type == EasingType.Out) {
				return CubicEaseOut(p);
			} else {
				return CubicEaseInOut(p);
			}
		}

		public static float CubicEaseIn(float p)
		{
			return p * p * p;
		}

		public static float CubicEaseOut(float p)
		{
			float f = (p - 1);
			return f * f * f + 1;
		}

		public static float CubicEaseInOut(float p)
		{
			if (p < 0.5f) {
				return 4 * p * p * p;
			} else {
				float f = ((2 * p) - 2);
				return 0.5f * f * f * f + 1;
			}
		}

		public static float QuarticEase(float p, EasingType type)
		{
			if (type == EasingType.In) {
				return QuarticEaseIn(p);
			} else if (type == EasingType.Out) {
				return QuarticEaseOut(p);
			} else {
				return QuarticEaseInOut(p);
			}
		}

		public static float QuarticEaseIn(float p)
		{
			return p * p * p * p;
		}

		public static float QuarticEaseOut(float p)
		{
			float f = (p - 1);
			return f * f * f * (1 - p) + 1;
		}

		public static float QuarticEaseInOut(float p)
		{
			if (p < 0.5f) {
				return 8 * p * p * p * p;
			} else {
				float f = (p - 1);
				return -8 * f * f * f * f + 1;
			}
		}

		public static float QuinticEase(float p, EasingType type)
		{
			if (type == EasingType.In) {
				return QuinticEaseIn(p);
			} else if (type == EasingType.Out) {
				return QuinticEaseOut(p);
			} else {
				return QuinticEaseInOut(p);
			}
		}

		public static float QuinticEaseIn(float p)
		{
			return p * p * p * p * p;
		}

		public static float QuinticEaseOut(float p)
		{
			float f = (p - 1);
			return f * f * f * f * f + 1;
		}

		public static float QuinticEaseInOut(float p)
		{
			if (p < 0.5f) {
				return 16 * p * p * p * p * p;
			} else {
				float f = ((2 * p) - 2);
				return 0.5f * f * f * f * f * f + 1;
			}
		}

		public static float SineEase(float p, EasingType type)
		{
			if (type == EasingType.In) {
				return SineEaseIn(p);
			} else if (type == EasingType.Out) {
				return SineEaseOut(p);
			} else {
				return SineEaseInOut(p);
			}
		}

		public static float SineEaseIn(float p)
		{
			return Mathf.Sin((p - 1) * Mathf.HalfPi) + 1;
		}

		public static float SineEaseOut(float p)
		{
			return Mathf.Sin(p * Mathf.HalfPi);
		}

		public static float SineEaseInOut(float p)
		{
			return 0.5f * (1 - Mathf.Cos(p * Mathf.Pi));
		}

		public static float CircularEase(float p, EasingType type)
		{
			if (type == EasingType.In) {
				return CircularEaseIn(p);
			} else if (type == EasingType.Out) {
				return CircularEaseOut(p);
			} else {
				return CircularEaseInOut(p);
			}
		}

		public static float CircularEaseIn(float p)
		{
			return 1 - Mathf.Sqrt(1 - (p * p));
		}

		public static float CircularEaseOut(float p)
		{
			return Mathf.Sqrt((2 - p) * p);
		}

		public static float CircularEaseInOut(float p)
		{
			if (p < 0.5f) {
				return 0.5f * (1 - Mathf.Sqrt(1 - 4 * (p * p)));
			} else {
				return 0.5f * (Mathf.Sqrt(-((2 * p) - 3) * ((2 * p) - 1)) + 1);
			}
		}

		public static float ExponentialEase(float p, EasingType type)
		{
			if (type == EasingType.In) {
				return ExponentialEaseIn(p);
			} else if (type == EasingType.Out) {
				return ExponentialEaseOut(p);
			} else {
				return ExponentialEaseInOut(p);
			}
		}

		public static float ExponentialEaseIn(float p)
		{
			return (p == 0.0f) ? p : Mathf.Pow(2, 10 * (p - 1));
		}

		public static float ExponentialEaseOut(float p)
		{
			return (p == 1.0f) ? p : 1 - Mathf.Pow(2, -10 * p);
		}

		public static float ExponentialEaseInOut(float p)
		{
			if (p == 0.0 || p == 1.0) return p;

			if (p < 0.5f) {
				return 0.5f * Mathf.Pow(2, (20 * p) - 10);
			} else {
				return -0.5f * Mathf.Pow(2, (-20 * p) + 10) + 1;
			}
		}

		public static float ElasticEase(float p, EasingType type)
		{
			if (type == EasingType.In) {
				return ElasticEaseIn(p);
			} else if (type == EasingType.Out) {
				return ElasticEaseOut(p);
			} else {
				return ElasticEaseInOut(p);
			}
		}

		public static float ElasticEaseIn(float p)
		{
			return (float)(Math.Sin(13 * Mathf.HalfPi * p) * Math.Pow(2, 10 * (p - 1)));
		}

		public static float ElasticEaseOut(float p)
		{
			return Mathf.Sin(-13 * Mathf.HalfPi * (p + 1)) * Mathf.Pow(2, -10 * p) + 1;
		}

		public static float ElasticEaseInOut(float p)
		{
			if (p < 0.5f) {
				return 0.5f * Mathf.Sin(13 * Mathf.HalfPi * (2 * p)) * Mathf.Pow(2, 10 * ((2 * p) - 1));
			} else {
				return 0.5f * (Mathf.Sin(-13 * Mathf.HalfPi * ((2 * p - 1) + 1)) * Mathf.Pow(2, -10 * (2 * p - 1)) + 2);
			}
		}

		public static float BackEase(float p, EasingType type)
		{
			if (type == EasingType.In) {
				return BackEaseIn(p);
			} else if (type == EasingType.Out) {
				return BackEaseOut(p);
			} else {
				return BackEaseInOut(p);
			}
		}

		public static float BackEaseIn(float p)
		{
			return p * p * p - p * Mathf.Sin(p * Mathf.Pi);
		}

		public static float BackEaseOut(float p)
		{
			float f = (1 - p);
			return 1 - (f * f * f - f * Mathf.Sin(f * Mathf.Pi));
		}

		public static float BackEaseInOut(float p)
		{
			if (p < 0.5f) {
				float f = 2 * p;
				return 0.5f * (f * f * f - f * Mathf.Sin(f * Mathf.Pi));
			} else {
				float f = (1 - (2 * p - 1));
				return 0.5f * (1 - (f * f * f - f * Mathf.Sin(f * Mathf.Pi))) + 0.5f;
			}
		}

		public static float BounceEase(float p, EasingType type)
		{
			if (type == EasingType.In) {
				return BounceEaseIn(p);
			} else if (type == EasingType.Out) {
				return BounceEaseOut(p);
			} else {
				return BounceEaseInOut(p);
			}
		}

		public static float BounceEaseIn(float p)
		{
			return 1 - BounceEaseOut(1 - p);
		}

		public static float BounceEaseOut(float p)
		{
			if (p < 4 / 11.0f) {
				return (121 * p * p) / 16.0f;
			} else if (p < 8 / 11.0f) {
				return (363 / 40.0f * p * p) - (99 / 10.0f * p) + 17 / 5.0f;
			} else if (p < 9 / 10.0f) {
				return (4356 / 361.0f * p * p) - (35442 / 1805.0f * p) + 16061 / 1805.0f;
			} else {
				return (54 / 5.0f * p * p) - (513 / 25.0f * p) + 268 / 25.0f;
			}
		}

		public static float BounceEaseInOut(float p)
		{
			if (p < 0.5f) {
				return 0.5f * BounceEaseIn(p * 2);
			} else {
				return 0.5f * BounceEaseOut(p * 2 - 1) + 0.5f;
			}
		}
	}


}
