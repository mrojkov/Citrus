using System;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Representation of 3x2 transformation matrix.
	/// </summary>
	[ProtoContract]
	[System.Diagnostics.DebuggerStepThrough]
	public struct Matrix32 : IEquatable<Matrix32>
	{
		[ProtoMember(1)]
		public Vector2 U;

		[ProtoMember(2)]
		public Vector2 V;

		[ProtoMember(3)]
		public Vector2 T;

		/// <summary>
		/// Returns the identity matrix. It doesn't move the points at all.
		/// </summary>
		public static readonly Matrix32 Identity = new Matrix32(new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, 0));

		public Matrix32(Vector2 u, Vector2 v, Vector2 t)
		{
			U = u;
			V = v;
			T = t;
		}

		public bool Equals(Matrix32 rhs)
		{
			return U.Equals(rhs.U) && V.Equals(rhs.V) && T.Equals(rhs.T);
		}

		// TODO: Transform to Determinant property?
		public float CalcDeterminant()
		{
			return U.X * V.Y - U.Y * V.X;
		}

		// TODO: Transform to Inversed property?
		public Matrix32 CalcInversed()
		{
			Matrix32 m;
			float invDet = 1.0f / CalcDeterminant();
			m.U.X = invDet * V.Y; m.U.Y = -invDet * U.Y;
			m.V.X = -invDet * V.X; m.V.Y = invDet * U.X;
			m.T.X = invDet * (V.X * T.Y - V.Y * T.X);
			m.T.Y = invDet * (U.Y * T.X - U.X * T.Y);
			return m;
		}

		/// <summary>
		/// Returns rotation matrix.
		/// </summary>
		public static Matrix32 Rotation(float radians)
		{
			var cs = Mathf.CosSin(radians);
			Matrix32 m;
			m.U.X = cs.X;
			m.U.Y = cs.Y;
			m.V.X = -cs.Y;
			m.V.Y = cs.X;
			m.T.X = 0;
			m.T.Y = 0;
			return m;
		}

		/// <summary>
		/// Returns scaling matrix.
		/// </summary>
		public static Matrix32 Scaling(Vector2 scaling)
		{
			var m = Identity;
			m.U.X = scaling.X;
			m.V.Y = scaling.Y;
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
		/// Returns transformation matrix.
		/// </summary>
		/// <param name="center">Center of rotation and scaling.</param>
		/// <param name="rotation">Rotation (in radians).</param>
		public static Matrix32 Transformation(Vector2 center, Vector2 scaling, float rotation, Vector2 translation)
		{
			var cs = Mathf.CosSin(rotation);
			Matrix32 m;
			m.U.X = scaling.X * cs.X;
			m.U.Y = scaling.X * cs.Y;
			m.V.X = -scaling.Y * cs.Y;
			m.V.Y = scaling.Y * cs.X;
			m.T.X = -center.X * m.U.X - center.Y * m.V.X + center.X + translation.X;
			m.T.Y = -center.X * m.U.Y - center.Y * m.V.Y + center.Y + translation.Y;
			return m;
		}

		/// <summary>
		/// Multiplication of matrixes. 
		/// Combines transformations in result (this operation is non-communicative).
		/// </summary>
		public static Matrix32 operator *(Matrix32 a, Matrix32 b)
		{
			Matrix32 m;
			m.U.X = a.U.X * b.U.X + a.U.Y * b.V.X;
			m.U.Y = a.U.X * b.U.Y + a.U.Y * b.V.Y;
			m.V.X = a.V.X * b.U.X + a.V.Y * b.V.X;
			m.V.Y = a.V.X * b.U.Y + a.V.Y * b.V.Y;
			m.T.X = a.T.X * b.U.X + a.T.Y * b.V.X + b.T.X;
			m.T.Y = a.T.X * b.U.Y + a.T.Y * b.V.Y + b.T.Y;
			return m;
		}

		/// <summary>
		/// Multiplication of matrixes. 
		/// Combines transformations in result (this operation is non-communicative).
		/// </summary>
		public static void Multiply(ref Matrix32 a, ref Matrix32 b, out Matrix32 result)
		{
			result.U.X = a.U.X * b.U.X + a.U.Y * b.V.X;
			result.U.Y = a.U.X * b.U.Y + a.U.Y * b.V.Y;
			result.V.X = a.V.X * b.U.X + a.V.Y * b.V.X;
			result.V.Y = a.V.X * b.U.Y + a.V.Y * b.V.Y;
			result.T.X = a.T.X * b.U.X + a.T.Y * b.V.X + b.T.X;
			result.T.Y = a.T.X * b.U.Y + a.T.Y * b.V.Y + b.T.Y;
		}

		/// <summary>
		/// Multiplication of 3x2 matrix with 2x1 matrix (represented by a vector).
		/// </summary>
		public static Vector2 operator *(Matrix32 a, Vector2 b)
		{
			Vector2 v;
			v.X = b.X * a.U.X + b.Y * a.V.X + a.T.X;
			v.Y = b.X * a.U.Y + b.Y * a.V.Y + a.T.Y;
			return v;
		}

		public Vector2 TransformVector(Vector2 a)
		{
			Vector2 v;
			v.X = a.X * U.X + a.Y * V.X + T.X;
			v.Y = a.X * U.Y + a.Y * V.Y + T.Y;
			return v;
		}
		
		public Vector2 TransformVector(float x, float y)
		{
			Vector2 v;
			v.X = x * U.X + y * V.X + T.X;
			v.Y = x * U.Y + y * V.Y + T.Y;
			return v;
		}
		
		public static Vector2 operator *(Vector2 a, Matrix32 b)
		{
			return b.TransformVector(a);
		}

		public static explicit operator Matrix44(Matrix32 m)
		{
			var r = Matrix44.Identity;
			r.M41 = m.T.X;
			r.M42 = m.T.Y;
			r.M11 = m.U.X;
			r.M12 = m.U.Y;
			r.M21 = m.V.X;
			r.M22 = m.V.Y;
			return r;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix32"/> that contains 
		/// linear interpolation of the specified matrixes.
		/// </summary>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <param name="value1">The first matrix.</param>
		/// <param name="value2">The second matrix.</param>
		public static Matrix32 Lerp(float amount, Matrix32 value1, Matrix32 value2)
		{
			Matrix32 result;
			result.T = Mathf.Lerp(amount, value1.T, value2.T);
			result.U = Mathf.Lerp(amount, value1.U, value2.U);
			result.V = Mathf.Lerp(amount, value1.V, value2.V);
			return result;
		}

		// TODO: Transform to IsIdentity property?
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
	}
}