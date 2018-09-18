using System.Globalization;
using NUnit.Framework;

namespace Lime.Tests.Source.Types
{
	[TestFixture]
	public class Vector3Tests
	{
		[Test]
		public void Vector2CastTest()
		{
			Assert.That((Vector2)Vector3.Zero, Is.EqualTo(Vector2.Zero));
			Assert.That((Vector2)Vector3.Half, Is.EqualTo(Vector2.Half));
			Assert.That((Vector2)new Vector3(1.7f, 2.2f, 3.5f), Is.EqualTo(new Vector2(1.7f, 2.2f)));
		}

		[Test]
		public void LerpTest()
		{
			Assert.That(Vector3.Lerp(0, Vector3.Zero, Vector3.One), Is.EqualTo(Vector3.Zero));
			Assert.That(Vector3.Lerp(0.5f, Vector3.Zero, Vector3.One), Is.EqualTo(Vector3.Half));
			Assert.That(Vector3.Lerp(1, Vector3.Zero, Vector3.One), Is.EqualTo(Vector3.One));
		}

		[Test]
		public void EqualsTest()
		{
			Assert.That(Vector3.Half.Equals(Vector3.Half));
			Assert.That(Vector3.Half.Equals((object)Vector3.Half));
			Assert.That(Vector3.Half == Vector3.Half);
			Assert.That(Vector3.Zero != Vector3.One);
			Assert.That(Vector3.Half.GetHashCode(), Is.EqualTo(Vector3.Half.GetHashCode()));
		}

		[Test]
		public void OperationsTest()
		{
			Assert.That(Vector3.One * Vector3.Zero, Is.EqualTo(Vector3.Zero));
			Assert.That(Vector3.One - Vector3.One, Is.EqualTo(Vector3.Zero));
			Assert.That(0.5f * (Vector3.One * 2), Is.EqualTo(Vector3.One));
			Assert.That(Vector3.Half / Vector3.Half, Is.EqualTo(Vector3.One));
			Assert.That(Vector3.One / 2, Is.EqualTo(Vector3.Half));
			Assert.That(Vector3.Half + Vector3.Half, Is.EqualTo(Vector3.One));
			Assert.That(-Vector3.One, Is.EqualTo(new Vector3(-1, -1, -1)));
			Assert.That(Vector3.DotProduct(Vector3.Half, Vector3.Half), Is.EqualTo(0.75f));
			Assert.That(Vector3.CrossProduct(Vector3.Half, Vector3.One), Is.EqualTo(Vector3.Zero));
		}

		[Test]
		public void NormalizedTest()
		{
			var unitVector = new Vector3(1 / Mathf.Sqrt(3));
			Assert.That(Vector3.Zero.Normalized, Is.EqualTo(Vector3.Zero));
			Assert.That(Vector3.One.Normalized, Is.EqualTo(unitVector));
			Assert.That(Vector3.Half.Normalized, Is.EqualTo(unitVector));
		}

		[Test]
		public void LengthTest()
		{
			Assert.That(Vector3.Zero.Length, Is.EqualTo(0));
			Assert.That(Vector3.Half.Length, Is.EqualTo(Mathf.Sqrt(0.75f)));
			Assert.That(Vector3.One.Length, Is.EqualTo(Mathf.Sqrt(3)));
			Assert.That(Vector3.Zero.SqrLength, Is.EqualTo(0));
			Assert.That(Vector3.Half.SqrLength, Is.EqualTo(0.75f));
			Assert.That(Vector3.One.SqrLength, Is.EqualTo(3));
		}

		// TODO: No Parse?
		[Test]
		public void ToStringTest()
		{
			Assert.That(Vector3.Zero.ToString(CultureInfo.InvariantCulture), Is.EqualTo("0, 0, 0"));
			Assert.That(Vector3.Half.ToString(CultureInfo.InvariantCulture), Is.EqualTo("0.5, 0.5, 0.5"));
			Assert.That(Vector3.One.ToString(CultureInfo.InvariantCulture), Is.EqualTo("1, 1, 1"));
		}
	}
}
