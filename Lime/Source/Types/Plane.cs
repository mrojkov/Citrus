using System;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public struct Plane : IEquatable<Plane>
	{
		[ProtoMember(1)]
		public float D;

		[ProtoMember(2)]
		public Vector3 Normal;

		public Plane(Vector3 normal, float d)
		{
			Normal = normal;
			D = d;
		}

		public Plane(Vector3 a, Vector3 b, Vector3 c)
		{
			var ab = b - a;
			var ac = c - a;
			var cross = Vector3.CrossProduct(ab, ac);
			Normal = cross.Normalized;
			D = -Vector3.DotProduct(Normal, a);
		}

		public float Dot(Vector4 value)
		{
			return ((((this.Normal.X * value.X) + (this.Normal.Y * value.Y)) + (this.Normal.Z * value.Z)) + (this.D * value.W));
		}

		public void Dot(ref Vector4 value, out float result)
		{
			result = (((this.Normal.X * value.X) + (this.Normal.Y * value.Y)) + (this.Normal.Z * value.Z)) + (this.D * value.W);
		}

		public float DotCoordinate(Vector3 value)
		{
			return ((((this.Normal.X * value.X) + (this.Normal.Y * value.Y)) + (this.Normal.Z * value.Z)) + this.D);
		}

		public void DotCoordinate(ref Vector3 value, out float result)
		{
			result = (((this.Normal.X * value.X) + (this.Normal.Y * value.Y)) + (this.Normal.Z * value.Z)) + this.D;
		}

		public float DotNormal(Vector3 value)
		{
			return (((this.Normal.X * value.X) + (this.Normal.Y * value.Y)) + (this.Normal.Z * value.Z));
		}

		public void DotNormal(ref Vector3 value, out float result)
		{
			result = ((this.Normal.X * value.X) + (this.Normal.Y * value.Y)) + (this.Normal.Z * value.Z);
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
			return (other is Plane) ? this.Equals((Plane)other) : false;
		}

		public bool Equals(Plane other)
		{
			return ((Normal == other.Normal) && (D == other.D));
		}

		public override int GetHashCode()
		{
			return Normal.GetHashCode() ^ D.GetHashCode();
		}
	}
}