using NUnit.Framework;

namespace Lime.Tests.Source.Types
{
	[TestFixture]
	public class RayTests
	{
		private static Ray unitRay = new Ray(Vector3.Zero, new Vector3(1, 0, 0));

		[Test]
		public void EqualsTest()
		{
			Assert.That(unitRay.Equals(unitRay));
			Assert.That(unitRay.Equals((object) unitRay));
			Assert.That(unitRay == unitRay);
			Assert.That(unitRay != new Ray(Vector3.Zero, Vector3.Zero));
			Assert.That(unitRay.GetHashCode(), Is.EqualTo(unitRay.GetHashCode()));
		}

		[Test]
		public void IntersectsTest()
		{
			var disjoiningSphere = new BoundingSphere(new Vector3(-2, 0, 0), 1);
			Assert.That(unitRay.Intersects(ref disjoiningSphere), Is.Null);
			var containingSphere = new BoundingSphere(Vector3.Zero, 1);
			Assert.That(unitRay.Intersects(ref containingSphere), Is.EqualTo(0));
			var intersectingSphere = new BoundingSphere(new Vector3(2, 0, 0), 1);
			Assert.That(unitRay.Intersects(ref intersectingSphere), Is.EqualTo(1));
		}
	}
}
