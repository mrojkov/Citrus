using System;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Прямоугольник
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough]
	[ProtoContract]
	public struct Rectangle : IEquatable<Rectangle>
	{
		[ProtoMember(1)]
		public Vector2 A;

		[ProtoMember(2)]
		public Vector2 B;

		public static readonly Rectangle Empty = new Rectangle();
		
		public Rectangle(float left, float top, float right, float bottom)
		{
			A.X = left;
			A.Y = top;
			B.X = right;
			B.Y = bottom;
		}

		public Rectangle(Vector2 a, Vector2 b)
		{
			A = a;
			B = b;
		}

		public static explicit operator IntRectangle(Rectangle r)
		{
			return new IntRectangle((int)r.Left, (int)r.Top, (int)r.Right, (int)r.Bottom);
		}

		public override bool Equals(object obj)
		{
			var rhs = (Rectangle)obj;
			return A.Equals(rhs.A) && B.Equals(rhs.B);
		}

		public bool Equals(Rectangle other)
		{
			return A.Equals(other.A) && B.Equals(other.B);
		}

		public static bool operator ==(Rectangle lhs, Rectangle rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Rectangle lhs, Rectangle rhs)
		{
			return !lhs.Equals(rhs);
		}

		/// <summary>
		/// Ширина прямоугольника
		/// </summary>
		public float Width
		{
			get { return B.X - A.X; }
			set { B.X = A.X + value; }
		}

		/// <summary>
		/// Высота прямоугольника
		/// </summary>
		public float Height
		{
			get { return B.Y - A.Y; }
			set { B.Y = A.Y + value; }
		}

		public float Left { get { return A.X; } set { A.X = value; } }
		public float Top { get { return A.Y; } set { A.Y = value; } }
		public float Right { get { return B.X; } set { B.X = value; } }
		public float Bottom { get { return B.Y; } set { B.Y = value; } }
		public Vector2 Center { get { return (A + B) / 2; } }

		/// <summary>
		/// Если координаты левого верхнего (A) и правого нижнего угла (B) перепутаны местами,
		/// то ширина и высота будут отрицательными. Этот метод меняет координаты местами,
		/// чтобы ширина и высота были всегда положительными
		/// </summary>
		public void Normalize()
		{
			if (A.X > B.X) {
				Toolbox.Swap(ref A.X, ref B.X);
			}
			if (A.Y > B.Y) {
				Toolbox.Swap(ref A.Y, ref B.Y);
			}
		}

		/// <summary>
		/// Возвращает true, если указанная точка попадает в область прямоугольника
		/// </summary>
		public bool Contains(Vector2 v)
		{
			return (v.X >= A.X) && (v.Y >= A.Y) && (v.X < B.X) && (v.Y < B.Y);
		}

		/// <summary>
		/// Возвращает размеры прямоугольника в виде вектора
		/// </summary>
		public Vector2 Size { 
			get { return B - A; }
		}

		/// <summary>
		/// Возвращает прямоугольник, построенный по области пересечения указанный прямоугольников.
		/// Если прямоугольники не пересекаются, возвращает пустой прямоугольник (IntRectangle.Empty)
		/// </summary>
		public static Rectangle Intersect(Rectangle a, Rectangle b)
		{
			float x0 = Math.Max(a.A.X, b.A.X);
			float x1 = Math.Min(a.B.X, b.B.X);
			float y0 = Math.Max(a.A.Y, b.A.Y);
			float y1 = Math.Min(a.B.Y, b.B.Y);
			if (x1 >= x0 && y1 >= y0) {
				return new Rectangle(x0, y0, x1, y1);
			} else {
				return Empty;
			}
		}

		/// <summary>
		/// Возвращает прямоугольник, охватывающий оба указанных прямоугольника
		/// </summary>
		public static Rectangle Bounds(Rectangle a, Rectangle b)
		{
			return new Rectangle(
				Mathf.Min(a.Left, b.Left),
				Mathf.Min(a.Top, b.Top),
				Mathf.Max(a.Right, b.Right),
				Mathf.Max(a.Bottom, b.Bottom)
			);
		}

		/// <summary>
		/// Изменяет границы прямоугольника таким образом, чтобы точка попадала в его область.
		/// Если точка уже в области фигуры, то ничего не делает
		/// </summary>
		public void IncludePoint(Vector2 v)
		{
			Left = Mathf.Min(v.X, Left);
			Right = Mathf.Max(v.X, Right);
			Top = Mathf.Min(v.Y, Top);
			Bottom = Mathf.Max(v.Y, Bottom);
		}

		public override string ToString()
		{
			return String.Format("{0}, {1}, {2}, {3}", A.X, A.Y, B.X, B.Y);
		}

		public override int GetHashCode()
		{
			return A.GetHashCode() ^ B.GetHashCode();
		}

		/// <summary>
		/// Применяет матрицу трансформации
		/// </summary>
		public Rectangle Transform(Matrix32 m)
		{
			return new Rectangle(A * m, B * m);
		}
	}
}