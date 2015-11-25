using System;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Representation of endless ray.
	/// </summary>
	[ProtoContract]
	public struct Ray : IEquatable<Ray>
	{
		[ProtoMember(1)]
		public Vector3 Direction;

		[ProtoMember(2)]
		public Vector3 Position;

		public Ray(Vector3 position, Vector3 direction)
		{
			Position = position;
			Direction = direction;
		}

		public override bool Equals(object obj)
		{
			return obj is Ray && Equals((Ray)obj);
		}


		public bool Equals(Ray other)
		{
			return Position.Equals(other.Position) && Direction.Equals(other.Direction);
		}


		public override int GetHashCode()
		{
			return Position.GetHashCode() ^ Direction.GetHashCode();
		}

		/// <summary>
		/// Calculates the distance between the center of this ray and border of sphere. 
		/// </summary>
		/// <param name="sphere">Sphere to check intersection for.</param>
		/// <returns>
		/// The distance between the center of this ray and border of sphere.
		/// Returns 0.0f if ray is inside of sphere. 
		/// Returns null if ray is pointed away from sphere.
		/// </returns>
		public float? Intersects(ref BoundingSphere sphere)
		{
			// Find the vector between where the ray starts the the sphere's centre
			var difference = sphere.Center - Position;
			float differenceLengthSquared = difference.SqrLength;
			float sphereRadiusSquared = sphere.Radius * sphere.Radius;

			// If the distance between the ray start and the sphere's centre is less than
			// the radius of the sphere, it means we've intersected. N.B. checking the LengthSquared is faster.
			if (differenceLengthSquared < sphereRadiusSquared) {
				return 0.0f;
			}

			float distanceAlongRay = Vector3.DotProduct(Direction, difference);

			// If the ray is pointing away from the sphere then we don't ever intersect
			if (distanceAlongRay < 0) {
				return null;
			}

			// Next we kinda use Pythagoras to check if we are within the bounds of the sphere
			// if x = radius of sphere
			// if y = distance between ray position and sphere centre
			// if z = the distance we've travelled along the ray
			// if x^2 + z^2 - y^2 < 0, we do not intersect
			float dist = sphereRadiusSquared + distanceAlongRay * distanceAlongRay - differenceLengthSquared;
			return dist < 0 ? null : distanceAlongRay - (float?)Math.Sqrt(dist);
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