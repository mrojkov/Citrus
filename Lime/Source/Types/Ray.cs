using System;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public struct Ray : IEquatable<Ray>
	{
		[ProtoMember(1)]
		public Vector3 Direction;

		[ProtoMember(2)]
		public Vector3 Position;

		public Ray(Vector3 position, Vector3 direction)
		{
			this.Position = position;
			this.Direction = direction;
		}

		public override bool Equals(object obj)
		{
			return Equals((Ray)obj);
		}


		public bool Equals(Ray other)
		{
			return this.Position.Equals(other.Position) && this.Direction.Equals(other.Direction);
		}


		public override int GetHashCode()
		{
			return Position.GetHashCode() ^ Direction.GetHashCode();
		}

		public float? Intersects(BoundingSphere sphere)
		{
			float? result;
			Intersects(ref sphere, out result);
			return result;
		}

		public void Intersects(ref BoundingSphere sphere, out float? result)
		{
			// Find the vector between where the ray starts the the sphere's centre
			Vector3 difference = sphere.Center - this.Position;
			float differenceLengthSquared = difference.SquaredLength;
			float sphereRadiusSquared = sphere.Radius * sphere.Radius;

			// If the distance between the ray start and the sphere's centre is less than
			// the radius of the sphere, it means we've intersected. N.B. checking the LengthSquared is faster.
			if (differenceLengthSquared < sphereRadiusSquared) {
				result = 0.0f;
				return;
			}

			float distanceAlongRay = Vector3.DotProduct(Direction, difference);

			// If the ray is pointing away from the sphere then we don't ever intersect
			if (distanceAlongRay < 0) {
				result = null;
				return;
			}

			// Next we kinda use Pythagoras to check if we are within the bounds of the sphere
			// if x = radius of sphere
			// if y = distance between ray position and sphere centre
			// if z = the distance we've travelled along the ray
			// if x^2 + z^2 - y^2 < 0, we do not intersect
			float dist = sphereRadiusSquared + distanceAlongRay * distanceAlongRay - differenceLengthSquared;
			result = (dist < 0) ? null : distanceAlongRay - (float?)Math.Sqrt(dist);
		}


		public static bool operator !=(Ray a, Ray b)
		{
			return !a.Equals(b);
		}


		public static bool operator ==(Ray a, Ray b)
		{
			return a.Equals(b);
		}
	}
}