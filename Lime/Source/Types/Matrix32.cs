using System;
using ProtoBuf;

namespace Lime
{
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

		public static readonly Matrix32 Identity = new Matrix32(new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, 0));

		public Matrix32(Vector2 u, Vector2 v, Vector2 t)
		{
			U = u;
			V = v;
			T = t;
		}

		bool IEquatable<Matrix32>.Equals(Matrix32 rhs)
		{
			return U.Equals(rhs.U) && V.Equals(rhs.V) && T.Equals(rhs.T);
		}

		public float CalcDeterminant()
		{
			return U.X * V.Y - U.Y * V.X;
		}

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

		public static Matrix32 Rotation(float radians)
		{
			Vector2 cs = Utils.CosSin(radians);
			Matrix32 m;
			m.U.X = cs.X;
			m.U.Y = cs.Y;
			m.V.X = -cs.Y;
			m.V.Y = cs.X;
			m.T.X = 0;
			m.T.Y = 0;
			return m;
		}

		public static Matrix32 Scaling(Vector2 scaling)
		{
			Matrix32 m = Identity;
			m.U.X = scaling.X;
			m.V.Y = scaling.Y;
			return m;
		}

		public static Matrix32 Translation(Vector2 translation)
		{
			Matrix32 m = Identity;
			m.T = translation;
			return m;
		}

		public static Matrix32 Transformation(Vector2 center, Vector2 scaling, float rotation, Vector2 translation)
		{
			Vector2 cs = Utils.CosSin(rotation);
			Matrix32 m;
			m.U.X = scaling.X * cs.X;
			m.U.Y = scaling.X * cs.Y;
			m.V.X = -scaling.Y * cs.Y;
			m.V.Y = scaling.Y * cs.X;
			m.T.X = -center.X * m.U.X - center.Y * m.V.X + center.X + translation.X;
			m.T.Y = -center.X * m.U.Y - center.Y * m.V.Y + center.Y + translation.Y;
			return m;
		}

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
		/*
		public static explicit operator OpenTK.Matrix4(Matrix32 m)
		{
			OpenTK.Matrix4 r = OpenTK.Matrix4.Identity;
			r.M41 = m.T.X;
			r.M42 = m.T.Y;
			r.M11 = m.U.X;
			r.M12 = m.U.Y;
			r.M21 = m.V.X;
			r.M22 = m.V.Y;
			return r;
		}
		*/
	}
}