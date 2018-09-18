using NUnit.Framework;

namespace Lime.Tests.Source.Types
{
	[TestFixture]
	class IntVector2Tests
	{
		[Test]
		public void Vector2CastTest()
		{
			Assert.That((Vector2)IntVector2.Zero, Is.EqualTo(Vector2.Zero));
			Assert.That((Vector2)new IntVector2(1, 2), Is.EqualTo(new Vector2(1, 2)));
			Assert.That(IntVector2.Zero.ToVector2(), Is.EqualTo(Vector2.Zero));
		}

		[Test]
		public void SizeCastTest()
		{
			Assert.That((Size)IntVector2.Zero, Is.EqualTo(new Size(0, 0)));
			Assert.That((Size)new IntVector2(1, 2), Is.EqualTo(new Size(1, 2)));
		}

		[Test]
		public void EqualsTest()
		{
			Assert.That(IntVector2.One.Equals(IntVector2.One));
			Assert.That(IntVector2.One.Equals((object)IntVector2.One));
			Assert.That(IntVector2.One == IntVector2.One);
			Assert.That(IntVector2.Zero != IntVector2.One);
			Assert.That(IntVector2.One.GetHashCode(), Is.EqualTo(IntVector2.One.GetHashCode()));
		}

		[Test]
		public void OperationsTest()
		{
			Assert.That(IntVector2.One - IntVector2.One, Is.EqualTo(IntVector2.Zero));
			Assert.That(1 * (IntVector2.One * 2), Is.EqualTo(new IntVector2(2, 2)));
			Assert.That(new IntVector2(2, 2) / 2, Is.EqualTo(IntVector2.One));
			Assert.That(IntVector2.One + IntVector2.One, Is.EqualTo(new IntVector2(2, 2)));
			Assert.That(-IntVector2.One, Is.EqualTo(new IntVector2(-1, -1)));
		}
	}
}
