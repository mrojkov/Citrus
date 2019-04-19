using System;
using Yuzu;

namespace Lime
{
	public struct Plane : IEquatable<Plane>
	{
		[YuzuMember]
		public float D;

		[YuzuMember]
		public Vector3 Normal;

		public Plane Normalized
		{
			get {
				var magnitude = 1.0f / Normal.Length;
				return new Plane {
					Normal = Normal * magnitude,
					D = D * magnitude
				};
			}
		}

		public Plane(Vector3 normal, float d)
		{
			Normal = normal;
			D = d;
		}

		public static Plane FromTriangle(Vector3 a, Vector3 b, Vector3 c)
		{
			var ab = b - a;
			var ac = c - a;
			var cross = Vector3.CrossProduct(ab, ac);
			var normal = cross.Normalized;
			return new Plane {
				Normal = normal,
				D = -Vector3.DotProduct(normal, a)
			};
		}

		public float Dot(Vector4 value)
		{
			return Vector4.DotProduct(new Vector4(Normal, D), value);
		}

		public float DotCoordinate(Vector3 value)
		{
			return Dot(new Vector4(value, 1f));
		}

		public float DotNormal(Vector3 value)
		{
			return Vector3.DotProduct(Normal, value);
		}

		public Plane Transform(Matrix44 matrix)
		{
			var vector = matrix
				.CalcInverted()
				.Transpose()
				.TransformVector(new Vector4(Normal, D));
			return new Plane((Vector3)vector, vector.W);
		}

		public static bool operator !=(Plane plane1, Plane plane2)
		{
			return !plane1.Equals(plane2);
		}

		public static bool operator ==(Plane plane1, Plane plane2)
		{
			return plane1.Equals(plane2);
		}

		public override bool Equals(object other)
		{
			return other is Plane && this.Equals((Plane)other);
		}

		public bool Equals(Plane other)
		{
			return Normal == other.Normal && D == other.D;
		}

		public override int GetHashCode()
		{
			unchecked {
				return (D.GetHashCode() * 397) ^ Normal.GetHashCode();
			}
		}
	}
}
