using System;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	/// <summary>
	/// Representation of solid sphere.
	/// </summary>
	public struct BoundingSphere : IEquatable<BoundingSphere>
	{
		[YuzuMember]
		public Vector3 Center;

		[YuzuMember]
		public float Radius;

		public BoundingSphere(Vector3 center, float radius)
		{
			Center = center;
			Radius = radius;
		}

		/// <summary>
		/// Test if a sphere is fully inside, outside, or just intersecting the sphere.
		/// </summary>
		public ContainmentType Contains(ref BoundingSphere sphere)
		{
			var sqDistance = (sphere.Center - Center).SqrLength;
			if (sqDistance > (sphere.Radius + Radius) * (sphere.Radius + Radius)) {
				return ContainmentType.Disjoint;
			}
			if (sqDistance <= (Radius - sphere.Radius) * (Radius - sphere.Radius)) {
				return ContainmentType.Contains;
			}
			return ContainmentType.Intersects;
		}

		/// <summary>
		/// Test if a vector is fully inside, outside, or just intersecting the sphere.
		/// </summary>
		public ContainmentType Contains(ref Vector3 point)
		{
			float sqRadius = Radius * Radius;
			float sqDistance = (point - Center).SqrLength;
			if (sqDistance > sqRadius) {
				return ContainmentType.Disjoint;
			}
			if (sqDistance < sqRadius) {
				return ContainmentType.Contains;
			}
			return ContainmentType.Intersects;
		}

		/// <summary>
		/// Creates the smallest sphere that can contain a specified list of points.
		/// </summary>
		public static BoundingSphere CreateFromPoints(IEnumerable<Vector3> points)
		{
			if (points == null) {
				throw new ArgumentNullException("points");
			}

			// From "Real-Time Collision Detection" (Page 89)
			var minx = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			var maxx = -minx;
			var miny = minx;
			var maxy = -minx;
			var minz = minx;
			var maxz = -minx;

			// Find the most extreme points along the principle axis.
			var numPoints = 0;
			foreach (var pt in points) {
				++numPoints;
				if (pt.X < minx.X)
					minx = pt;
				if (pt.X > maxx.X)
					maxx = pt;
				if (pt.Y < miny.Y)
					miny = pt;
				if (pt.Y > maxy.Y)
					maxy = pt;
				if (pt.Z < minz.Z)
					minz = pt;
				if (pt.Z > maxz.Z)
					maxz = pt;
			}

			if (numPoints == 0) {
				throw new ArgumentException("You should have at least one point in points.");
			}

			var sqDistX = (maxx - minx).SqrLength;
			var sqDistY = (maxy - miny).SqrLength;
			var sqDistZ = (maxz - minz).SqrLength;

			// Pick the pair of most distant points.
			var min = minx;
			var max = maxx;
			if (sqDistY > sqDistX && sqDistY > sqDistZ) {
				max = maxy;
				min = miny;
			}
			if (sqDistZ > sqDistX && sqDistZ > sqDistY) {
				max = maxz;
				min = minz;
			}

			var center = (min + max) * 0.5f;
			var radius = (max - center).Length;

			// Test every point and expand the sphere.
			// The current bounding sphere is just a good approximation and may not enclose all points.
			// From: Mathematics for 3D Game Programming and Computer Graphics, Eric Lengyel, Third Edition.
			// Page 218
			float sqRadius = radius * radius;
			foreach (var pt in points) {
				var diff = (pt - center);
				float sqDist = diff.SqrLength;
				if (sqDist > sqRadius) {
					float distance = (float)Math.Sqrt(sqDist);
					var direction = diff / distance;
					var G = center - radius * direction;
					center = (G + pt) / 2;
					radius = (pt - center).Length;
					sqRadius = radius * radius;
				}
			}

			return new BoundingSphere(center, radius);
		}

		public bool Equals(BoundingSphere other)
		{
			return Center == other.Center && Radius == other.Radius;
		}

		public override bool Equals(object obj)
		{
			return obj is BoundingSphere && Equals((BoundingSphere)obj);
		}

		public override int GetHashCode()
		{
			return Center.GetHashCode() + Radius.GetHashCode();
		}

		/// <summary>
		/// Creates a new sphere that contains a transformation of translation and scale
		/// from this sphere by the specified Matrix.
		/// </summary>
		public BoundingSphere Transform(Matrix44 matrix)
		{
			return new BoundingSphere
			{
				Center = matrix.TransformVector(Center),
				Radius =
					Radius *
					((float)
						Math.Sqrt(Math.Max(((matrix.M11 * matrix.M11) + (matrix.M12 * matrix.M12)) + (matrix.M13 * matrix.M13),
							Math.Max(((matrix.M21 * matrix.M21) + (matrix.M22 * matrix.M22)) + (matrix.M23 * matrix.M23),
								((matrix.M31 * matrix.M31) + (matrix.M32 * matrix.M32)) + (matrix.M33 * matrix.M33)))))
			};
		}

		public static bool operator ==(BoundingSphere a, BoundingSphere b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(BoundingSphere a, BoundingSphere b)
		{
			return !a.Equals(b);
		}
	}
}