using System;
using ProtoBuf;

namespace Lime
{
	[System.Diagnostics.DebuggerStepThrough]
	[ProtoContract]
	public struct IntRectangle : IEquatable<IntRectangle>
	{
		[ProtoMember(1)]
		public IntVector2 A;

		[ProtoMember(2)]
		public IntVector2 B;
		
		public IntRectangle (int left, int top, int right, int bottom)
		{
			A.X = left;
			A.Y = top;
			B.X = right;
			B.Y = bottom;
		}

		public IntRectangle (IntVector2 a, IntVector2 b)
		{
			A = a;
			B = b;
		}
		
		bool IEquatable<IntRectangle>.Equals (IntRectangle other)
		{
			return A.Equals (other.A) && B.Equals (other.B);
		}
		
		public int Width {
			get {
				return B.X - A.X;
			}
			set {
				B.X = A.X + value;
			}
		}

		public int Height {
			get {
				return B.Y - A.Y;
			}
			set {
				B.Y = A.Y + value;
			}
		}
		
		bool Contains (IntVector2 v)
		{
			return (v.X >= A.X) && (v.Y >= A.Y) && (v.X < B.X) && (v.Y < B.Y);
		}
		
		public override string ToString ()
		{
			return String.Format ("{0}, {1}, {2}, {3}", A.X, A.Y, B.X, B.Y);
		}
	}
}
