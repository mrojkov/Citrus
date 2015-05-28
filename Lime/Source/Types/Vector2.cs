using System;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Представляет точку с в двумерном пространстве
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough]
	[ProtoContract]
	public struct Vector2 : IEquatable<Vector2>
	{
		[ProtoMember(1)]
		public float X;

		[ProtoMember(2)]
		public float Y;

		/// <summary>
		/// Возвращает вектор (0, 0)
		/// </summary>
		public static readonly Vector2 Zero = new Vector2(0, 0);

		/// <summary>
		/// Возвращает вектор (1, 1)
		/// </summary>
		public static readonly Vector2 One = new Vector2(1, 1);

		/// <summary>
		/// Возвращает вектор (0.5, 0.5)
		/// </summary>
		public static readonly Vector2 Half = new Vector2(0.5f, 0.5f);

		/// <summary>
		/// Возвращает вектор (0, -1)
		/// </summary>
		public static readonly Vector2 North = new Vector2(0, -1);

		/// <summary>
		/// Возвращает вектор (0, 1)
		/// </summary>
		public static readonly Vector2 South = new Vector2(0, 1);

		/// <summary>
		/// Возвращает вектор (1, 0)
		/// </summary>
		public static readonly Vector2 East = new Vector2(1, 0);

		/// <summary>
		/// Возвращает вектор (-1, 0)
		/// </summary>
		public static readonly Vector2 West = new Vector2(-1, 0);

		/// <summary>
		/// Возвращает вектор (Infinity, Infinity)
		/// </summary>
		public static readonly Vector2 PositiveInfinity = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

		/// <summary>
		/// Возвращает вектор (-Infinity, -Infinity)
		/// </summary>
		public static readonly Vector2 NegativeInfinity = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

		/// <summary>
		/// Возвращает вектор (0, -1)
		/// </summary>
		public static readonly Vector2 Up = new Vector2(0, -1);

		/// <summary>
		/// Возвращает вектор (0, 1)
		/// </summary>
		public static readonly Vector2 Down = new Vector2(0, 1);

		/// <summary>
		/// Возвращает вектор (-1, 0)
		/// </summary>
		public static readonly Vector2 Left = new Vector2(-1, 0);

		/// <summary>
		/// Возвращает вектор (1, 0)
		/// </summary>
		public static readonly Vector2 Right = new Vector2(1, 0);

		public Vector2(float x, float y)
		{
			X = x;
			Y = y;
		}

		public static explicit operator IntVector2(Vector2 v)
		{
			return new IntVector2((int)v.X, (int)v.Y);
		}

		/// <summary>
		/// Преобразует Vector2 в Vector3. Координата Z результирующего вектора будет 0
		/// </summary>
		public static explicit operator Vector3(Vector2 v)
		{
			return new Vector3(v.X, v.Y, 0);
		}

		public static explicit operator Size(Vector2 v)
		{
			return new Size((int)v.X, (int)v.Y);
		}

		/// <summary>
		/// Выполняет операцию сравнения. Сравнение выполняется без учеты погрешности дробных чисел
		/// </summary>
		public bool Equals(Vector2 rhs)
		{
			return X == rhs.X && Y == rhs.Y;
		}

		/// <summary>
		/// Выполняет операцию сравнения. Сравнение выполняется без учеты погрешности дробных чисел
		/// </summary>
		public override bool Equals(object o)
		{
			Vector2 rhs = (Vector2)o;
			return X == rhs.X && Y == rhs.Y;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}

		/// <summary>
		/// Выполняет операцию сравнения. Сравнение выполняется без учеты погрешности дробных чисел
		/// </summary>
		public static bool operator == (Vector2 lhs, Vector2 rhs)
		{
			return lhs.X == rhs.X && lhs.Y == rhs.Y;
		}

		/// <summary>
		/// Выполняет операцию проверки неравенства. Сравнение выполняется без учеты погрешности дробных чисел
		/// </summary>
		public static bool operator != (Vector2 lhs, Vector2 rhs)
		{
			return lhs.X != rhs.X || lhs.Y != rhs.Y;
		}

		/// <summary>
		/// Возвращает угол между векторами в градусах
		/// </summary>
		public static float AngleDeg(Vector2 a, Vector2 b)
		{
			return AngleRad(a, b) * Mathf.RadiansToDegrees;
		}

		/// <summary>
		/// Возвращает угол между векторами в радианах
		/// </summary>
		/// <returns></returns>
		public static float AngleRad(Vector2 a, Vector2 b)
		{
			float sin = a.X * b.Y - b.X * a.Y;
			float cos = a.X * b.X + a.Y * b.Y;
			return (float)Math.Atan2(sin, cos);
		}

		/// <summary>
		/// Возвращает результат линейной интерполяции двух векторов
		/// </summary>
		/// <param name="t">Значение интерполяции [0, 1]</param>
		/// <param name="a">Первый вектор</param>
		/// <param name="b">Второй вектор</param>
		public static Vector2 Lerp(float t, Vector2 a, Vector2 b)
		{
			Vector2 r = new Vector2();
			r.X = (b.X - a.X) * t + a.X;
			r.Y = (b.Y - a.Y) * t + a.Y;
			return r;
		}

		/// <summary>
		/// Возвращает расстояние между точками
		/// </summary>
		public static float Distance(Vector2 a, Vector2 b)
		{
			return (a - b).Length;
		}

		/// <summary>
		/// Покомпонентное умножение векторов
		/// </summary>
		public static Vector2 operator *(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.X * rhs.X, lhs.Y * rhs.Y);
		}

		/// <summary>
		/// Покомпонентное деление векторов
		/// </summary>
		public static Vector2 operator /(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.X / rhs.X, lhs.Y / rhs.Y);
		}

		/// <summary>
		/// Делит каждый компонент вектора на число
		/// </summary>
		public static Vector2 operator /(Vector2 lhs, float rhs)
		{
			return new Vector2(lhs.X / rhs, lhs.Y / rhs);
		}

		/// <summary>
		/// Умножает каждый компонент вектора на число
		/// </summary>
		public static Vector2 operator *(float lhs, Vector2 rhs)
		{
			return new Vector2(lhs * rhs.X, lhs * rhs.Y);
		}

		/// <summary>
		/// Умножает каждый компонент вектора на число
		/// </summary>
		public static Vector2 operator *(Vector2 lhs, float rhs)
		{
			return new Vector2(rhs * lhs.X, rhs * lhs.Y);
		}

		/// <summary>
		/// Покомпонентное сложение векторов
		/// </summary>
		public static Vector2 operator +(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.X + rhs.X, lhs.Y + rhs.Y);
		}

		/// <summary>
		/// Покомпонентная разность векторов
		/// </summary>
		public static Vector2 operator -(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.X - rhs.X, lhs.Y - rhs.Y);
		}

		/// <summary>
		/// Меняет знак у каждого компонента вектора
		/// </summary>
		public static Vector2 operator -(Vector2 v)
		{
			return new Vector2(-v.X, -v.Y);
		}

		/// <summary>
		/// Скалярное умножение векторов (операция dot)
		/// </summary>
		public static float DotProduct(Vector2 lhs, Vector2 rhs)
		{
			return lhs.X * rhs.X + lhs.Y * rhs.Y;
		}

		/// <summary>
		/// Векторное умножение (операция cross)
		/// </summary>
		public static float CrossProduct(Vector2 lhs, Vector2 rhs)
		{
			return lhs.X * rhs.Y - lhs.Y * rhs.X;
		}

		/// <summary>
		/// Нормализует вектор
		/// </summary>
        public void Normalize()
        {
            float length = this.Length;
            if (length > 0) {
                this.X /= length;
                this.Y /= length;
            }
        }

		/// <summary>
		/// Возвращает вектор направления
		/// </summary>
		/// <param name="degrees">Азимут в градусах (поворот по часовой стрелке)</param>
		public static Vector2 HeadingDeg(float degrees)
		{
			return Mathf.CosSin(degrees * Mathf.DegreesToRadians);
		}

		/// <summary>
		/// Возвращает вектор направления
		/// </summary>
		/// <param name="degrees">Азимут в радианах (поворот по часовой стрелке)</param>
		public static Vector2 HeadingRad(float radians)
		{
			return Mathf.CosSin(radians);
		}

		/// <summary>
		/// Поворачивает вектор вокруг точки (0, 0)
		/// </summary>
		public static Vector2 RotateDeg(Vector2 v, float degrees)
		{
			return RotateRad(v, degrees * Mathf.DegreesToRadians);
		}

		/// <summary>
		/// Поворачивает вектор вокруг точки (0, 0)
		/// </summary>
		public static Vector2 RotateRad(Vector2 v, float radians)
		{
			Vector2 cs = Mathf.CosSin(radians);
			Vector2 result;
			result.X = v.X * cs.X - v.Y * cs.Y;
			result.Y = v.X * cs.Y + v.Y * cs.X;
			return result;
		}

		/// <summary>
		/// Возвращает арктангенс текущего вектора в диапазоне (-Pi, Pi]
		/// </summary>
		public float Atan2Rad
		{
			get { return (float)Math.Atan2(Y, X); }
		}

		/// <summary>
		/// Возвращает арктангенс текущего вектора в диапазоне (-180, 180]
		/// </summary>
		public float Atan2Deg
		{
			get { return (float)Math.Atan2(Y, X) * Mathf.RadiansToDegrees; }
		}

		/// <summary>
		/// Возвращает длину вектора (его модуль)
		/// </summary>
		public float Length
		{
			get { return (float)Math.Sqrt(X * X + Y * Y); }
		}

		/// <summary>
		/// Возвращает нормализованное значение текущего вектора
		/// </summary>
		public Vector2 Normalized
		{
			get 
			{
				var v = new Vector2(X, Y);
				v.Normalize();
				return v;
			}
		}

		/// <summary>
		/// Возвращает квадрат длины вектора
		/// </summary>
		public float SqrLength
		{
			get { return X * X + Y * Y; }
		}

		public override string ToString()
		{
			return String.Format("{0}, {1}", X, Y);
		}

		/// <summary>
		/// Парсит вектор. Возвращает true, если операция прошла удачно
		/// </summary>
		/// <param name="s">Строка (вида "12, 34")</param>
		/// <param name="vector">Переменная, в которую будет записан результат</param>
		public static bool TryParse(string s, out Vector2 vector)
		{
			vector = Vector2.Zero;
			if (string.IsNullOrWhiteSpace(s)) {
				return false;
			}

			var parts = s.Split(new string[] {", "}, StringSplitOptions.None);
			if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1])) {
				return false;
			}

			return float.TryParse(parts[0], out vector.X) & float.TryParse(parts[1], out vector.Y);
		}

		/// <summary>
		/// Парсит вектор. Генерирует исключение, если операция прошла неудачно
		/// </summary>
		/// <param name="s">Строка (вида "12, 34")</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="FormatException"/>
		public static Vector2 Parse(string s)
		{
			if (s == null) {
				throw new ArgumentNullException();
			}
			var vector = Vector2.Zero;
			if (!TryParse(s, out vector)) {
				throw new FormatException();
			}
			return vector;
		}
	}
}