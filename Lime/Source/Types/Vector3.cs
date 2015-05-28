using System;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Представляет точку с в трехмерном пространстве
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough]
	[ProtoContract]
	public struct Vector3 : IEquatable<Vector3>
	{
		[ProtoMember(1)]
		public float X;

		[ProtoMember(2)]
		public float Y;

		[ProtoMember(3)]
		public float Z;

		/// <summary>
		/// Возвращает вектор (0, 0, 0)
		/// </summary>
		public static readonly Vector3 Zero = new Vector3(0, 0, 0);

		/// <summary>
		/// Возвращает вектор (1, 1, 1)
		/// </summary>
		public static readonly Vector3 One = new Vector3(1, 1, 1);

		/// <summary>
		/// Возвращает вектор (0.5, 0.5, 0.5)
		/// </summary>
		public static readonly Vector3 Half = new Vector3(0.5f, 0.5f, 0.5f);

		public Vector3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// Преобразует Vector3 в Vector2. Координата Z не учитывается
		/// </summary>
		public static explicit operator Vector2(Vector3 v)
		{
			return new Vector2(v.X, v.Y);
		}

		bool IEquatable<Vector3>.Equals(Vector3 rhs)
		{
			return X == rhs.X && Y == rhs.Y && Z == rhs.Z;
		}

		/// <summary>
		/// Возвращает результат линейной интерполяции двух векторов
		/// </summary>
		/// <param name="t">Значение интерполяции [0, 1]</param>
		/// <param name="a">Первый вектор</param>
		/// <param name="b">Второй вектор</param>
		public static Vector3 Lerp(float t, Vector3 a, Vector3 b)
		{
			Vector3 r = new Vector3();
			r.X = (b.X - a.X) * t + a.X;
			r.Y = (b.Y - a.Y) * t + a.Y;
			r.Z = (b.Z - a.Z) * t + a.Z;
			return r;
		}

		/// <summary>
		/// Покомпонентное умножение векторов
		/// </summary>
		public static Vector3 operator *(Vector3 lhs, Vector3 rhs)
		{
			return new Vector3(lhs.X * rhs.X, lhs.Y * rhs.Y, lhs.Z * rhs.Z);
		}

		/// <summary>
		/// Покомпонентное деление векторов
		/// </summary>
		public static Vector3 operator /(Vector3 lhs, Vector3 rhs)
		{
			return new Vector3(lhs.X / rhs.X, lhs.Y / rhs.Y, lhs.Z / rhs.Z);
		}

		/// <summary>
		/// Делит каждый компонент вектора на число
		/// </summary>
		public static Vector3 operator /(Vector3 lhs, float rhs)
		{
			return new Vector3(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs);
		}

		/// <summary>
		/// Покомпонентное умножение векторов (аналогично операции умножения)
		/// </summary>
		public static Vector3 Scale(Vector3 lhs, Vector3 rhs)
		{
			return new Vector3(lhs.X * rhs.X, lhs.Y * rhs.Y, lhs.Z * rhs.Z);
		}

		/// <summary>
		/// Умножает каждый компонент вектора на число
		/// </summary>
		public static Vector3 operator *(float lhs, Vector3 rhs)
		{
			return new Vector3(lhs * rhs.X, lhs * rhs.Y, lhs * rhs.Z);
		}

		/// <summary>
		/// Умножает каждый компонент вектора на число
		/// </summary>
		public static Vector3 operator *(Vector3 lhs, float rhs)
		{
			return new Vector3(rhs * lhs.X, rhs * lhs.Y, rhs * lhs.Z);
		}

		/// <summary>
		/// Покомпонентное сложение векторов
		/// </summary>
		public static Vector3 operator +(Vector3 lhs, Vector3 rhs)
		{
			return new Vector3(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
		}

		/// <summary>
		/// Покомпонентная разность векторов
		/// </summary>
		public static Vector3 operator -(Vector3 lhs, Vector3 rhs)
		{
			return new Vector3(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);
		}

		/// <summary>
		/// Меняет знак у каждого компонента вектора
		/// </summary>
		public static Vector3 operator -(Vector3 v)
		{
			return new Vector3(-v.X, -v.Y, -v.Z);
		}

		/// <summary>
		/// Скалярное умножение векторов (операция dot)
		/// </summary>
		public static float DotProduct(Vector3 lhs, Vector3 rhs)
		{
			return lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z;
		}

		/// <summary>
		/// Векторное умножение (операция cross)
		/// </summary>
		public static Vector3 CrossProduct(Vector3 lhs, Vector3 rhs)
		{
			return new Vector3(
				lhs.Y * rhs.Z - lhs.Z * rhs.Y,
				lhs.Z * rhs.X - lhs.X * rhs.Z,
				lhs.X * rhs.Y - lhs.Y * rhs.X
			);
		}

		/// <summary>
		/// Возвращает нормализованный вектор
		/// </summary>
		public static Vector3 Normalize(Vector3 value)
		{
			float length = value.Length;
			if (length > 0) {
				value.X /= length;
				value.Y /= length;
				value.Z /= length;
			}
			return value;
		}

		/// <summary>
		/// Возвращает длину вектора (модуль вектора)
		/// </summary>
		public float Length
		{
			get { return (float)Math.Sqrt(X * X + Y * Y + Z * Z); }
		}

		/// <summary>
		/// Возвращает длину вектора (модуль), возведенную в квадрат
		/// </summary>
		public float SquaredLength
		{
			get { return X * X + Y * Y + Z * Z; }
		}

		public override string ToString()
		{
			return String.Format("{0}, {1}, {2}", X, Y, Z);
		}
	}
}