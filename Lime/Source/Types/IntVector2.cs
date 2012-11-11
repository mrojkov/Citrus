using System;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	[System.Diagnostics.DebuggerStepThrough]
	public struct IntVector2 : IEquatable<IntVector2>
	{
		[ProtoMember(1)]
		public int X;
		
		[ProtoMember(2)]
		public int Y;
		
		public static readonly IntVector2 Zero = new IntVector2(0, 0);
		public static readonly IntVector2 One = new IntVector2(1, 1);

		public IntVector2(int x, int y)
		{
			X = x;
			Y = y;
		}

		public static explicit operator Size(IntVector2 v)
		{
			return new Size(v.X, v.Y);
		}

		public static explicit operator Vector2(IntVector2 v)
		{
			return new Vector2((float)v.X, (float)v.Y);
		}

		bool IEquatable<IntVector2>.Equals(IntVector2 rhs)
		{
			return X == rhs.X && Y == rhs.Y;
		}

		public override bool Equals(object o)
		{
			IntVector2 rhs = (IntVector2)o;
			return X == rhs.X && Y == rhs.Y;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}

		public static bool operator ==(IntVector2 lhs, IntVector2 rhs)
		{
			return lhs.X == rhs.X && lhs.Y == rhs.Y;
		}

		public static bool operator !=(IntVector2 lhs, IntVector2 rhs)
		{
			return lhs.X != rhs.X || lhs.Y != rhs.Y;
		}

		public static IntVector2 operator +(IntVector2 lhs, IntVector2 rhs)
		{
			return new IntVector2(lhs.X + rhs.X, lhs.Y + rhs.Y);
		}

		public static IntVector2 operator -(IntVector2 lhs, IntVector2 rhs)
		{
			return new IntVector2(lhs.X - rhs.X, lhs.Y - rhs.Y);
		}

		public static IntVector2 operator -(IntVector2 v)
		{
			return new IntVector2(-v.X, -v.Y);
		}	

		public override string ToString()
		{
			return String.Format("{0}, {1}", X, Y);
		}

        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }
	}
}