using System;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Прямоугольник, свойства которого заданы целыми числами
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough]
	[ProtoContract]
	public struct IntRectangle : IEquatable<IntRectangle>
	{
		[ProtoMember(1)]
		public IntVector2 A;

		[ProtoMember(2)]
		public IntVector2 B;

		public static readonly IntRectangle Empty = new IntRectangle();
		
		public IntRectangle(int left, int top, int right, int bottom)
		{
			A.X = left;
			A.Y = top;
			B.X = right;
			B.Y = bottom;
		}

		public IntRectangle(IntVector2 a, IntVector2 b)
		{
			A = a;
			B = b;
		}

		public static explicit operator Rectangle(IntRectangle r)
		{
			return new Rectangle(r.Left, r.Top, r.Right, r.Bottom);
		}

		public static explicit operator WindowRect(IntRectangle r)
		{
			return new WindowRect { X = r.Left, Y = r.Top, Width = r.Width, Height = r.Height };
		}

		/// <summary>
		/// Если координаты левого верхнего (A) и правого нижнего угла (B) перепутаны местами,
		/// то ширина и высота будут отрицательными. Этот метод меняет координаты местами,
		/// чтобы ширина и высота были всегда положительными
		/// </summary>
		public Lime.IntRectangle Normalize()
		{
			var rect = this;
			if (Width < 0) {
				rect.A.X = B.X;
				rect.B.X = A.X;
			}
			if (Height < 0) {
				rect.A.Y = B.Y;
				rect.B.Y = A.Y;
			}
			return rect;
		}
		
		/// <summary>
		/// Ширина прямоугольника
		/// </summary>
		public int Width {
			get { return B.X - A.X; }
			set { B.X = A.X + value; }
		}

		/// <summary>
		/// Высота прямоугольника
		/// </summary>
		public int Height {
			get { return B.Y - A.Y; }
			set { B.Y = A.Y + value; }
		}

		/// <summary>
		/// Возвращает размеры прямоугольника в виде вектора
		/// </summary>
		public IntVector2 Size { 
			get { return B - A; }
		}

		public int Left { get { return A.X; } set { A.X = value; } }
		public int Top { get { return A.Y; } set { A.Y = value; } }
		public int Right { get { return B.X; } set { B.X = value; } }
		public int Bottom { get { return B.Y; } set { B.Y = value; } }
		public IntVector2 Center { get { return new IntVector2((A.X + B.X) / 2, (A.Y + B.Y) / 2); } }

		/// <summary>
		/// Сдвигает прямоугольник на указанное значение
		/// </summary>
		public IntRectangle OffsetBy(IntVector2 ofs) { return new IntRectangle(A + ofs, B + ofs); }

		public static bool operator ==(IntRectangle lhs, IntRectangle rhs)
		{
			return lhs.A == rhs.A && lhs.B == rhs.B;
		}

		public static bool operator !=(IntRectangle lhs, IntRectangle rhs)
		{
			return lhs.A != rhs.A || lhs.B != rhs.B;
		}

		bool IEquatable<IntRectangle>.Equals(IntRectangle other)
		{
			return A.Equals(other.A) && B.Equals(other.B);
		}
		
		public override bool Equals(object o)
		{
			var rhs = (IntRectangle)o;
			return A == rhs.A && B == rhs.B;
		}

		public override int GetHashCode()
		{
			return A.GetHashCode() ^ B.GetHashCode();
		}

		/// <summary>
		/// Возвращает true, если указанная точка попадает в область прямоугольника
		/// </summary>
		public bool Contains(IntVector2 v)
		{
			return (v.X >= A.X) && (v.Y >= A.Y) && (v.X < B.X) && (v.Y < B.Y);
		}
		
		public override string ToString()
		{
			return String.Format("{0}, {1}, {2}, {3}", A.X, A.Y, B.X, B.Y);
		}

		/// <summary>
		/// Возвращает прямоугольник, построенный по области пересечения указанный прямоугольников.
		/// Если прямоугольники не пересекаются, возвращает пустой прямоугольник (IntRectangle.Empty)
		/// </summary>
		public static IntRectangle Intersect(IntRectangle a, IntRectangle b)
		{
			int x0 = Math.Max(a.A.X, b.A.X);
			int x1 = Math.Min(a.B.X, b.B.X);
			int y0 = Math.Max(a.A.Y, b.A.Y);
			int y1 = Math.Min(a.B.Y, b.B.Y);
			if (x1 >= x0 && y1 >= y0) {
				return new IntRectangle(x0, y0, x1, y1);
			} else {
				return Empty;
			}
		}
	}
}
