using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Yuzu;

namespace Lime
{
	/// <summary>
	/// Representation of 3x2 transformation matrix.
	/// </summary>
	[YuzuCompact]
	[System.Diagnostics.DebuggerStepThrough]
	[StructLayout(LayoutKind.Explicit)]
	public struct Matrix32 : IEquatable<Matrix32>
	{
		[FieldOffset(0)]
		[YuzuMember("0")]
		public Vector2 U;

		[FieldOffset(8)]
		[YuzuMember("1")]
		public Vector2 V;

		[FieldOffset(16)]
		[YuzuMember("2")]
		public Vector2 T;

		[FieldOffset(0)]
		public float UX;

		[FieldOffset(4)]
		public float UY;

		[FieldOffset(8)]
		public float VX;

		[FieldOffset(12)]
		public float VY;

		[FieldOffset(16)]
		public float TX;

		[FieldOffset(20)]
		public float TY;

		/// <summary>
		/// Returns the identity matrix. It doesn't move the points at all.
		/// </summary>
		public static readonly Matrix32 Identity = new Matrix32(new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, 0));

		public Matrix32(Vector2 u, Vector2 v, Vector2 t)
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

		public bool Equals(Matrix32 rhs)
		{
			return UX == rhs.UX && UY == rhs.UY && VX == rhs.VX && VY == rhs.VY && TX == rhs.TX && TY == rhs.TY;
		}

		public float CalcDeterminant()
		{
			return U.X * V.Y - U.Y * V.X;
		}

		public Matrix32 CalcInversed()
		{
			var m = new Matrix32();
			float invDet = 1.0f / CalcDeterminant();
			m.UX = invDet * VY; m.UY = -invDet * UY;
			m.VX = -invDet * VX; m.VY = invDet * UX;
			m.TX = invDet * (VX * TY - VY * TX);
			m.TY = invDet * (UY * TX - UX * TY);
			return m;
		}

		/// <summary>
		/// Returns rough rotation matrix.
		/// </summary>
		public static Matrix32 RotationRough(float radians)
		{
			var cs = Vector2.CosSinRough(radians);
			return new Matrix32 {
				UX = cs.X,
				UY = cs.Y,
				VX = -cs.Y,
				VY = cs.X
			};
		}

		/// <summary>
		/// Returns rotation matrix.
		/// </summary>
		public static Matrix32 Rotation(float radians)
		{
			var cs = Vector2.CosSin(radians);
			return new Matrix32 {
				UX = cs.X,
				UY = cs.Y,
				VX = -cs.Y,
				VY = cs.X
			};
		}

		/// <summary>
		/// Returns scaling matrix.
		/// </summary>
		public static Matrix32 Scaling(Vector2 scaling)
		{
			var m = Identity;
			m.UX = scaling.X;
			m.VY = scaling.Y;
			return m;
		}

		/// <summary>
		/// Returns scaling matrix.
		/// </summary>
		public static Matrix32 Scaling(float x, float y)
		{
			return Scaling(new Vector2(x, y));
		}

		/// <summary>
		/// Returns translation matrix.
		/// </summary>
		public static Matrix32 Translation(Vector2 translation)
		{
			var m = Identity;
			m.T = translation;
			return m;
		}

		/// <summary>
		/// Returns translation matrix.
		/// </summary>
		public static Matrix32 Translation(float x, float y)
		{
			return Translation(new Vector2(x, y));
		}

		/// <summary>
		/// Returns the transformation matrix.
		/// </summary>
		/// <param name="center">Center of rotation and scaling.</param>
		/// <param name="rotation">Rotation (in radians).</param>
		public static Matrix32 Transformation(Vector2 center, Vector2 scaling, float rotation, Vector2 translation)
		{
			return Transformation(center, scaling, Vector2.CosSin(rotation), translation);
		}

		/// <summary>
		/// Returns the rough transformation matrix.
		/// </summary>
		/// <param name="center">Center of rotation and scaling.</param>
		/// <param name="rotation">Rotation (in radians).</param>
		public static Matrix32 TransformationRough(Vector2 center, Vector2 scaling, float rotation, Vector2 translation)
		{
			return Transformation(center, scaling, Vector2.CosSinRough(rotation), translation);
		}

		/// <summary>
		/// Returns the transformation matrix.
		/// </summary>
		/// <param name="center">Center of rotation and scaling.</param>
		/// <param name="direction">Unit vector of abscissa.</param>
		private static Matrix32 Transformation(Vector2 center, Vector2 scaling, Vector2 direction, Vector2 translation)
		{
			var m = new Matrix32();
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
		public static Matrix32 operator *(Matrix32 a, Matrix32 b)
		{
			return new Matrix32 {
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
		public static void Multiply(ref Matrix32 a, ref Matrix32 b, out Matrix32 result)
		{
			result = new Matrix32 {
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
		public static Vector2 operator *(Matrix32 a, Vector2 b)
		{
			return new Vector2 {
				X = b.X * a.UX + b.Y * a.VX + a.TX,
				Y = b.X * a.UY + b.Y * a.VY + a.TY
			};
		}

		public Vector2 TransformVector(Vector2 a)
		{
			return new Vector2 {
				X = a.X * UX + a.Y * VX + TX,
				Y = a.X * UY + a.Y * VY + TY
			};
		}

		public Vector2 TransformVector(float x, float y)
		{
			return new Vector2 {
				X = x * UX + y * VX + TX,
				Y = x * UY + y * VY + TY
			};
		}

		public Transform2 ToTransform2()
		{
			var vSign = Math.Sign(Vector2.CrossProduct(U, V));
			return new Transform2 {
				Translation = T,
				Scale = new Vector2(U.Length, V.Length * vSign),
				Rotation = U.Atan2Deg
			};
		}

		public static Vector2 operator *(Vector2 a, Matrix32 b)
		{
			return b.TransformVector(a);
		}

		public static explicit operator Matrix44(Matrix32 m)
		{
			var r = Matrix44.Identity;
			r.M41 = m.TX;
			r.M42 = m.TY;
			r.M11 = m.UX;
			r.M12 = m.UY;
			r.M21 = m.VX;
			r.M22 = m.VY;
			return r;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix32"/> that contains
		/// linear interpolation of the specified matrices.
		/// </summary>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <param name="value1">The first matrix.</param>
		/// <param name="value2">The second matrix.</param>
		public static Matrix32 Lerp(float amount, Matrix32 value1, Matrix32 value2)
		{
			var result = new Matrix32();
			result.T = Mathf.Lerp(amount, value1.T, value2.T);
			result.U = Mathf.Lerp(amount, value1.U, value2.U);
			result.V = Mathf.Lerp(amount, value1.V, value2.V);
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

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = 1568519108;
				hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(U);
				hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(V);
				hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(T);
				return hashCode;
			}
		}
	}
}
