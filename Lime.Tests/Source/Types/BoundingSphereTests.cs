using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Lime.Tests.Source.Types
{
	[TestFixture]
	public class BoundingSphereTests
	{
		private static BoundingSphere unitSphere = new BoundingSphere(Vector3.Zero, 1);

		[Test]
		public void EqualsTest()
		{	
			Assert.That(unitSphere.Equals(unitSphere));
			Assert.That(unitSphere.Equals((object)unitSphere));
			Assert.That(unitSphere == unitSphere);
			Assert.That(unitSphere != new BoundingSphere(Vector3.Zero, 0));
			Assert.That(unitSphere.GetHashCode(), Is.EqualTo(unitSphere.GetHashCode()));
		}

		[Test]
		public void ContainsBoundingSphereTest()
		{
			var disjoiningSphere = new BoundingSphere(new Vector3(3), 1);
			Assert.That(unitSphere.Contains(ref disjoiningSphere), Is.EqualTo(ContainmentType.Disjoint));
			var containingSphere = new BoundingSphere(Vector3.Zero, 0.5f);
			Assert.That(unitSphere.Contains(ref containingSphere), Is.EqualTo(ContainmentType.Contains));
			var intersectingSphere = new BoundingSphere(Vector3.One, 1);
			Assert.That(unitSphere.Contains(ref intersectingSphere), Is.EqualTo(ContainmentType.Intersects));
		}

		[Test]
		public void ContainsVectorTest()
		{
			var disjoiningVector = new Vector3(3);
			Assert.That(unitSphere.Contains(ref disjoiningVector), Is.EqualTo(ContainmentType.Disjoint));
			var containingVector = Vector3.Zero;
			Assert.That(unitSphere.Contains(ref containingVector), Is.EqualTo(ContainmentType.Contains));
			var intersectingVector = new Vector3(1, 0, 0);
			Assert.That(unitSphere.Contains(ref intersectingVector), Is.EqualTo(ContainmentType.Intersects));
		}

		[Test]
		public void CreateFromPointsTest()
		{
			Assert.Catch<ArgumentNullException>(() => BoundingSphere.CreateFromPoints(null));
			var actualPoints = new List<Vector3> ();
			Assert.Catch<ArgumentException>(() => BoundingSphere.CreateFromPoints(actualPoints));
			actualPoints = new List<Vector3> { Vector3.Zero };
			var actual = BoundingSphere.CreateFromPoints(actualPoints);
			Assert.That(actual, Is.EqualTo(new BoundingSphere(Vector3.Zero, 0)));
			actualPoints = new List<Vector3> {
				new Vector3(1, 0, 0),
				new Vector3(-1, 0, 0),
			};
			actual = BoundingSphere.CreateFromPoints(actualPoints);
            Assert.That(actual, Is.EqualTo(unitSphere));
			actualPoints = new List<Vector3> {
				new Vector3(0, 1, 0),
				new Vector3(0, -1, 0),
			};
			actual = BoundingSphere.CreateFromPoints(actualPoints);
			Assert.That(actual, Is.EqualTo(unitSphere));
			actualPoints = new List<Vector3> {
				new Vector3(0, 0, 1),
				new Vector3(0, 0, -1)
			};
			actual = BoundingSphere.CreateFromPoints(actualPoints);
			Assert.That(actual, Is.EqualTo(unitSphere));
		}

		[Test]
		public void TransformTest()
		{
			// TODO: Add more tests
			var actual = unitSphere.Transform(Matrix44.CreateTranslation(Vector3.One));
			var expected = new BoundingSphere(Vector3.One, 1);
			Assert.That(actual, Is.EqualTo(expected));
		}
	}
}
