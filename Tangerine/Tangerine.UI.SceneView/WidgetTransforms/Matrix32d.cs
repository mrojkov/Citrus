using System;
using System.Runtime.InteropServices;
using Lime;
using Yuzu;

namespace Tangerine.UI.SceneView.WidgetTransforms
{
	/// <summary>
	/// Representation of 3x2 transformation matrix.
	/// </summary>
	[YuzuCompact]
	[System.Diagnostics.DebuggerStepThrough]
	[StructLayout(LayoutKind.Explicit)]
	public struct Matrix32d : IEquatable<Matrix32d>
	{
		[FieldOffset(0)]
		[YuzuMember("0")]
		public Vector2d U;

		[FieldOffset(16)]
		[YuzuMember("1")]
		public Vector2d V;

		[FieldOffset(32)]
		[YuzuMember("2")]
		public Vector2d T;

		[FieldOffset(0)]
		public double UX;

		[FieldOffset(8)]
		public double UY;

		[FieldOffset(16)]
		public double VX;

		[FieldOffset(24)]
		public double VY;

		[FieldOffset(32)]
		public double TX;

		[FieldOffset(40)]
		public double TY;

		/// <summary>
		/// Returns the identity matrix. It doesn't move the points at all.
		/// </summary>
		public static readonly Matrix32d Identity = new Matrix32d(new Vector2d(1, 0), new Vector2d(0, 1), new Vector2d(0, 0));

		public Matrix32d(Vector2d u, Vector2d v, Vector2d t)
		{
			UX = 0;
			UY = 0;
			VX = 0;
			VY = 0;
			TX = 0;
			TY = 0;
			U = u;
			V = v;
			T = t;
		}

		public bool Equals(Matrix32d rhs)
		{
			return UX == rhs.UX && UY == rhs.UY && VX == rhs.VX && VY == rhs.VY && TX == rhs.TX && TY == rhs.TY;
		}

		public double CalcDeterminant()
		{
			return U.X * V.Y - U.Y * V.X;
		}

		public Matrix32d CalcInversed()
		{
			var m = new Matrix32d();
			double invDet = 1.0 / CalcDeterminant();
			m.UX = invDet * VY; m.UY = -invDet * UY;
			m.VX = -invDet * VX; m.VY = invDet * UX;
			m.TX = invDet * (VX * TY - VY * TX);
			m.TY = invDet * (UY * TX - UX * TY);
			return m;
		}

		/// <summary>
		/// Returns rotation matrix.
		/// </summary>
		public static Matrix32d Rotation(double radians)
		{
			var cs = Vector2d.CosSin(radians);
			return new Matrix32d {
				UX = cs.X,
				UY = cs.Y,
				VX = -cs.Y,
				VY = cs.X
			};
		}

		/// <summary>
		/// Returns scaling matrix.
		/// </summary>
		public static Matrix32d Scaling(Vector2d scaling)
		{
			var m = Identity;
			m.UX = scaling.X;
			m.VY = scaling.Y;
			return m;
		}

		/// <summary>
		/// Returns scaling matrix.
		/// </summary>
		public static Matrix32d Scaling(double x, double y)
		{
			return Scaling(new Vector2d(x, y));
		}

		/// <summary>
		/// Returns translation matrix.
		/// </summary>
		public static Matrix32d Translation(Vector2d translation)
		{
			var m = Identity;
			m.T = translation;
			return m;
		}

		/// <summary>
		/// Returns translation matrix.
		/// </summary>
		public static Matrix32d Translation(double x, double y)
		{
			return Translation(new Vector2d(x, y));
		}

		/// <summary>
		/// Returns the transformation matrix.
		/// </summary>
		/// <param name="center">Center of rotation and scaling.</param>
		/// <param name="rotation">Rotation (in radians).</param>
		public static Matrix32d Transformation(Vector2d center, Vector2d scaling, double rotation, Vector2d translation)
		{
			return Transformation(center, scaling, Vector2d.CosSin(rotation), translation);
		}

		/// <summary>
		/// Returns the transformation matrix.
		/// </summary>
		/// <param name="center">Center of rotation and scaling.</param>
		/// <param name="direction">Unit vector of abscissa.</param>
		private static Matrix32d Transformation(Vector2d center, Vector2d scaling, Vector2d direction, Vector2d translation)
		{
			var m = new Matrix32d();
			m.UX = scaling.X * direction.X;
			m.UY = scaling.X * direction.Y;
			m.VX = -scaling.Y * direction.Y;
			m.VY = scaling.Y * direction.X;
			m.TX = -center.X * m.UX - center.Y * m.VX + center.X + translation.X;
			m.TY = -center.X * m.UY - center.Y * m.VY + center.Y + translation.Y;
			return m;
		}

		/// <summary>
		/// Multiplication of matrices.
		/// Combines transformations in result (this operation is non-communicative).
		/// </summary>
		public static Matrix32d operator *(Matrix32d a, Matrix32d b)
		{
			return new Matrix32d {
				UX = a.UX * b.UX + a.UY * b.VX,
				UY = a.UX * b.UY + a.UY * b.VY,
				VX = a.VX * b.UX + a.VY * b.VX,
				VY = a.VX * b.UY + a.VY * b.VY,
				TX = a.TX * b.UX + a.TY * b.VX + b.TX,
				TY = a.TX * b.UY + a.TY * b.VY + b.TY
			};
		}

		/// <summary>
		/// Multiplication of matrices.
		/// Combines transformations in result (this operation is non-communicative).
		/// </summary>
		public static void Multiply(ref Matrix32d a, ref Matrix32d b, out Matrix32d result)
		{
			result = new Matrix32d {
				UX = a.UX * b.UX + a.UY * b.VX,
				UY = a.UX * b.UY + a.UY * b.VY,
				VX = a.VX * b.UX + a.VY * b.VX,
				VY = a.VX * b.UY + a.VY * b.VY,
				TX = a.TX * b.UX + a.TY * b.VX + b.TX,
				TY = a.TX * b.UY + a.TY * b.VY + b.TY
			};
		}

		/// <summary>
		/// Multiplication of 3x2 matrix with 2x1 matrix (represented by a vector).
		/// </summary>
		public static Vector2d operator *(Matrix32d a, Vector2d b)
		{
			return new Vector2d {
				X = b.X * a.UX + b.Y * a.VX + a.TX,
				Y = b.X * a.UY + b.Y * a.VY + a.TY
			};
		}

		public Vector2d TransformVector(Vector2d a)
		{
			return new Vector2d {
				X = a.X * UX + a.Y * VX + TX,
				Y = a.X * UY + a.Y * VY + TY
			};
		}

		public Vector2d TransformVector(double x, double y)
		{
			return new Vector2d {
				X = x * UX + y * VX + TX,
				Y = x * UY + y * VY + TY
			};
		}

		public Transform2d ToTransform2()
		{
			var vSign = Math.Sign(Vector2d.CrossProduct(U, V));
			return new Transform2d {
				Translation = T,
				Scale = new Vector2d(U.Length, V.Length * vSign),
				Rotation = U.Atan2Deg
			};
		}

		public static Vector2d operator *(Vector2d a, Matrix32d b)
		{
			return b.TransformVector(a);
		}

		public static explicit operator Matrix44(Matrix32d m)
		{
			var r = Matrix44.Identity;
			r.M41 = (float) m.TX;
			r.M42 = (float) m.TY;
			r.M11 = (float) m.UX;
			r.M12 = (float) m.UY;
			r.M21 = (float) m.VX;
			r.M22 = (float) m.VY;
			return r;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix32d"/> that contains
		/// linear interpolation of the specified matrices.
		/// </summary>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <param name="value1">The first matrix.</param>
		/// <param name="value2">The second matrix.</param>
		public static Matrix32d Lerp(double amount, Matrix32d value1, Matrix32d value2)
		{
			var result = new Matrix32d();
			result.T = Mathd.Lerp(amount, value1.T, value2.T);
			result.U = Mathd.Lerp(amount, value1.U, value2.U);
			result.V = Mathd.Lerp(amount, value1.V, value2.V);
			return result;
		}

		/// <summary>
		/// Checks if this matrix is identity matrix.
		/// </summary>
		/// <seealso cref="Identity"/>
		public bool IsIdentity()
		{
			return Equals(Identity);
		}

		public override string ToString()
		{
			return string.Format("U: {0}; V: {1}; T: {2}", U, V, T);
		}

		public static explicit operator Matrix32d(Matrix32 value)
		{
			return new Matrix32d((Vector2d) value.U, (Vector2d) value.V, (Vector2d) value.T);
		}
	}
}
