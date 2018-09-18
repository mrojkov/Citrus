using NUnit.Framework;

namespace Lime.Tests.Source.Types
{
	[TestFixture]
	public class SizeTests
	{
		[Test]
		public void Vector2CastTest()
		{
			Assert.That((Vector2)new Size(0, 0), Is.EqualTo(Vector2.Zero));
			Assert.That((Vector2)new Size(1, 1), Is.EqualTo(Vector2.One));
		}

		[Test]
		public void IntVector2CastTest()
		{
			Assert.That((IntVector2)new Size(0, 0), Is.EqualTo(IntVector2.Zero));
			Assert.That((IntVector2)new Size(1, 1), Is.EqualTo(IntVector2.One));
		}

		[Test]
		public void EqualsTest()
		{
			Assert.That(new Size(0, 0).Equals(new Size(0, 0)));
			Assert.That(new Size(0, 0).Equals((object)new Size(0, 0)));
			Assert.That(new Size(0, 0) == new Size(0, 0));
			Assert.That(new Size(0, 0) != new Size(1, 1));
			Assert.That(new Size(0, 0).GetHashCode(), Is.EqualTo(new Size(0, 0).GetHashCode()));
		}
		
		[Test]
		public void ToStringTest()
		{
			Assert.That(new Size(0, 0).ToString(), Is.EqualTo("0, 0"));
			Assert.That(new Size(1, 1).ToString(), Is.EqualTo("1, 1"));
		}
	}
}
