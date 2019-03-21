using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Code source: https://chromium.googlesource.com/chromium/src/+/master/ui/gfx/geometry/cubic_bezier.cc

namespace Lime
{
	public class CubicBezier
	{
		const double Epsilon = 1e-7;

		private double ax;
		private double bx;
		private double cx;
		private double ay;
		private double by;
		private double cy;
		private double startGradient;
		private double endGradient;
		private double rangeMin;
		private double rangeMax;

		public CubicBezier(double p1x, double p1y, double p2x, double p2y)
		{
			InitCoefficients(p1x, p1y, p2x, p2y);
			InitGradients(p1x, p1y, p2x, p2y);
			InitRange(p1y, p2y);
		}

		private void InitCoefficients(double p1x, double p1y, double p2x, double p2y)
		{
			// Calculate the polynomial coefficients, implicit first and last control
			// points are (0,0) and (1,1).
			cx = 3.0 * p1x;
			bx = 3.0 * (p2x - p1x) - cx;
			ax = 1.0 - cx - bx;
			cy = 3.0 * p1y;
			by = 3.0 * (p2y - p1y) - cy;
			ay = 1.0 - cy - by;
		}

		private void InitGradients(double p1x, double p1y, double p2x, double p2y)
		{
			// End-point gradients are used to calculate timing function results
			// outside the range [0, 1].
			//
			// There are three possibilities for the gradient at each end:
			// (1) the closest control point is not horizontally coincident with regard to
			//     (0, 0) or (1, 1). In this case the line between the end point and
			//     the control point is tangent to the bezier at the end point.
			// (2) the closest control point is coincident with the end point. In
			//     this case the line between the end point and the far control
			//     point is tangent to the bezier at the end point.
			// (3) the closest control point is horizontally coincident with the end
			//     point, but vertically distinct. In this case the gradient at the
			//     end point is Infinite. However, this causes issues when
			//     interpolating. As a result, we break down to a simple case of
			//     0 gradient under these conditions.
			if (p1x > 0) {
				startGradient = p1y / p1x;
			} else if (p1y == 0 && p2x > 0) {
				startGradient = p2y / p2x;
			} else {
				startGradient = 0;
			}

			if (p2x < 1) {
				endGradient = (p2y - 1) / (p2x - 1);
			} else if (p2x == 1 && p1x < 1) {
				endGradient = (p1y - 1) / (p1x - 1);
			} else {
				endGradient = 0;
			}
		}

		// This works by taking taking the derivative of the cubic bezier, on the y
		// axis. We can then solve for where the derivative is zero to find the min
		// and max distance along the line. We the have to solve those in terms of time
		// rather than distance on the x-axis
		private void InitRange(double p1y, double p2y)
		{
			rangeMin = 0;
			rangeMax = 1;
			if (0 <= p1y && p1y < 1 && 0 <= p2y && p2y <= 1) {
				return;
			}
			// Represent the function's derivative in the form at^2 + bt + c
			// as in sampleCurveDerivativeY.
			// (Technically this is (dy/dt)*(1/3), which is suitable for finding zeros
			// but does not actually give the slope of the curve.)
			var a = 3.0 * ay;
			var b = 2.0 * by;
			var c = cy;
			// Check if the derivative is constant.
			if (Math.Abs(a) < Epsilon && Math.Abs(b) < Epsilon) {
				return;
			}
			// Zeros of the function's derivative.
			double t1 = 0;
			double t2 = 0;
			if (Math.Abs(a) < Epsilon) {
				// The function's derivative is linear.
				t1 = -c / b;
			} else {
				// The function's derivative is a quadratic. We find the zeros of this
				// quadratic using the quadratic formula.
				var discriminant = b * b - 4 * a * c;
				if (discriminant < 0) {
					return;
				}
				var discriminantSqrt = Math.Sqrt(discriminant);
				t1 = (-b + discriminantSqrt) / (2 * a);
				t2 = (-b - discriminantSqrt) / (2 * a);
			}
			double sol1 = 0;
			double sol2 = 0;
			// If the solution is in the range [0,1] then we include it, otherwise we
			// ignore it.
			// An interesting fact about these beziers is that they are only
			// actually evaluated in [0,1]. After that we take the tangent at that point
			// and linearly project it out.
			if (0 < t1 && t1 < 1) {
				sol1 = SampleCurveY(t1);
			}
			if (0 < t2 && t2 < 1) {
				sol2 = SampleCurveY(t2);
			}
			rangeMin = Math.Min(Math.Min(rangeMin, sol1), sol2);
			rangeMax = Math.Max(Math.Max(rangeMax, sol1), sol2);
		}

		// Given an x value, find a parametric value it came from.
		// x must be in [0, 1] range. Doesn't use gradients.
		public double SolveCurveX(double x, double epsilon)
		{
			if (x < 0 || x > 1) {
				throw new ArgumentOutOfRangeException();
			}
			double t0;
			double t1;
			double t2;
			double x2;
			double d2;
			int i;
			// First try a few iterations of Newton's method -- normally very fast.
			for (t2 = x, i = 0; i < 8; i++) {
				x2 = SampleCurveX(t2) - x;
				if (Math.Abs(x2) < epsilon) {
					return t2;
				}
				d2 = SampleCurveDerivativeX(t2);
				if (Math.Abs(d2) < 1e-6) {
					break;
				}
				t2 -= x2 / d2;
			}
			// Fall back to the bisection method for reliability.
			t0 = 0.0;
			t1 = 1.0;
			t2 = x;
			while (t0 < t1) {
				x2 = SampleCurveX(t2);
				if (Math.Abs(x2 - x) < epsilon) {
					return t2;
				}
				if (x > x2) {
					t0 = t2;
				} else {
					t1 = t2;
				}
				t2 = (t1 - t0) * .5 + t0;
			}
			// Failure.
			return t2;
		}

		// Evaluates y at the given x. The epsilon parameter provides a hint as to the
		// required accuracy and is not guaranteed. Uses gradients if x is
		// out of [0, 1] range.
		public double SolveWithEpsilon(double x, double epsilon)
		{
			if (x < 0.0) {
				return 0.0 + startGradient * x;
			}
			if (x > 1.0) {
				return 1.0 + endGradient * (x - 1.0);
			}
			return SampleCurveY(SolveCurveX(x, epsilon));
		}

		public double SampleCurveX(double t)
		{
			// `ax t^3 + bx t^2 + cx t' expanded using Horner's rule.
			return ((ax * t + bx) * t + cx) * t;
		}

		public double SampleCurveY(double t)
		{
			return ((ay * t + by) * t + cy) * t;
		}

		private double SampleCurveDerivativeX(double t)
		{
			return (3.0 * ax * t + 2.0 * bx) * t + cx;
		}
	}
}
