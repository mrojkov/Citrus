using System;
using System.Globalization;
using NUnit.Framework;

namespace Lime.Tests.Source.Types
{
	[TestFixture]
	public class Vector2Tests
	{
		[Test]
		public void IntVector2CastTest()
		{
			Assert.That((IntVector2)Vector2.Zero, Is.EqualTo(IntVector2.Zero));
			Assert.That((IntVector2)new Vector2(0.5f), Is.EqualTo(IntVector2.Zero));
			Assert.That((IntVector2)new Vector2(1.7f, 2.2f), Is.EqualTo(new IntVector2(1, 2)));
		}

		[Test]
		public void Vector3CastTest()
		{
			Assert.That((Vector3)Vector2.Zero, Is.EqualTo(Vector3.Zero));
			Assert.That((Vector3)new Vector2(0.5f), Is.EqualTo(new Vector3(0.5f, 0.5f, 0)));
			Assert.That((Vector3)new Vector2(1.7f, 2.2f), Is.EqualTo(new Vector3(1.7f, 2.2f, 0)));
		}

		[Test]
		public void SizeCastTest()
		{
			Assert.That((Size)Vector2.Zero, Is.EqualTo(new Size(0, 0)));
			Assert.That((Size)new Vector2(0.5f), Is.EqualTo(new Size(0, 0)));
			Assert.That((Size)new Vector2(1.7f, 2.2f), Is.EqualTo(new Size(1, 2)));
		}

		[Test]
		public void EqualsTest()
		{
			Assert.That(Vector2.Half.Equals(Vector2.Half));
			Assert.That(Vector2.Half.Equals((object)Vector2.Half));
			Assert.That(Vector2.Half == Vector2.Half);
			Assert.That(Vector2.Zero != Vector2.One);
			Assert.That(Vector2.Half.GetHashCode(), Is.EqualTo(Vector2.Half.GetHashCode()));
		}

		[Test]
		public void AngleDegTest()
		{
			Assert.That(Vector2.AngleDeg(Vector2.Right, Vector2.Right), Is.EqualTo(0));
			Assert.That(Vector2.AngleDeg(Vector2.Up, Vector2.Down), Is.EqualTo(180));
			Assert.That(Vector2.AngleDeg(Vector2.Down, Vector2.Up), Is.EqualTo(-180));
		}

		[Test]
		public void AngleRadTest()
		{
			Assert.That(Vector2.AngleRad(Vector2.Right, Vector2.Right), Is.EqualTo(0));
			Assert.That(Vector2.AngleRad(Vector2.Up, Vector2.Down), Is.EqualTo(Mathf.Pi));
			Assert.That(Vector2.AngleRad(Vector2.Down, Vector2.Up), Is.EqualTo(-Mathf.Pi));
		}

		[Test]
		public void LerpTest()
		{
			Assert.That(Vector2.Lerp(0, Vector2.Zero, Vector2.One), Is.EqualTo(Vector2.Zero));
			Assert.That(Vector2.Lerp(0.5f, Vector2.Zero, Vector2.One), Is.EqualTo(Vector2.Half));
			Assert.That(Vector2.Lerp(1, Vector2.Zero, Vector2.One), Is.EqualTo(Vector2.One));
		}

		[Test]
		public void DistanceTest()
		{
			Assert.That(Vector2.Distance(Vector2.Zero, Vector2.Zero), Is.EqualTo(Vector2.Zero.Length));
			Assert.That(Vector2.Distance(Vector2.Zero, Vector2.Half), Is.EqualTo(Vector2.Half.Length));
			Assert.That(Vector2.Distance(Vector2.Zero, Vector2.One), Is.EqualTo(Vector2.One.Length));
		}

		[Test]
		public void OperationsTest()
		{
			Assert.That(Vector2.One * Vector2.Zero, Is.EqualTo(Vector2.Zero));
			Assert.That(Vector2.One - Vector2.One, Is.EqualTo(Vector2.Zero));
			Assert.That(0.5f * (Vector2.One * 2), Is.EqualTo(Vector2.One));
			Assert.That(Vector2.Half / Vector2.Half, Is.EqualTo(Vector2.One));
			Assert.That(Vector2.One / 2, Is.EqualTo(Vector2.Half));
			Assert.That(Vector2.Half + Vector2.Half, Is.EqualTo(Vector2.One));
			Assert.That(-Vector2.One, Is.EqualTo(new Vector2(-1)));
			Assert.That(Vector2.DotProduct(Vector2.Half, Vector2.Half), Is.EqualTo(0.5f));
			Assert.That(Vector2.CrossProduct(Vector2.Half, Vector2.Half), Is.EqualTo(0));
		}

		private NUnit.Framework.Constraints.EqualConstraint IsApproximately(Vector2 expected)
		{
			return Is.EqualTo(expected).Within(0.0001f);
		}

		[Test]
		public void CosSinRoughTest()
		{
			Assert.That(Vector2.CosSinRough(0), IsApproximately(Vector2.Right));
			Assert.That(Vector2.CosSinRough(Mathf.Pi), IsApproximately(Vector2.Left));
			Assert.That(Vector2.CosSinRough(Mathf.HalfPi), IsApproximately(Vector2.Down));
		}

		[Test]
		public void RotateDegTest()
		{
			Assert.That(Vector2.RotateDeg(Vector2.Right, 0), IsApproximately(Vector2.Right));
			Assert.That(Vector2.RotateDeg(Vector2.Right, 180), IsApproximately(Vector2.Left));
			Assert.That(Vector2.RotateDeg(Vector2.Right, 90), IsApproximately(Vector2.Down));
		}

		[Test]
		public void RotateRadTest()
		{
			Assert.That(Vector2.RotateRad(Vector2.Right, 0), IsApproximately(Vector2.Right));
			Assert.That(Vector2.RotateRad(Vector2.Right, Mathf.Pi), IsApproximately(Vector2.Left));
			Assert.That(Vector2.RotateRad(Vector2.Right, Mathf.HalfPi), IsApproximately(Vector2.Down));
		}

		[Test]
		public void Atan2DegTest()
		{
			Assert.That(Vector2.Right.Atan2Deg, Is.EqualTo(0));
			Assert.That(Vector2.Left.Atan2Deg, Is.EqualTo(180));
			Assert.That(Vector2.Down.Atan2Deg, Is.EqualTo(90));
		}

		[Test]
		public void Atan2RadTest()
		{
			Assert.That(Vector2.Right.Atan2Rad, Is.EqualTo(0));
			Assert.That(Vector2.Left.Atan2Rad, Is.EqualTo(Mathf.Pi));
			Assert.That(Vector2.Down.Atan2Rad, Is.EqualTo(Mathf.HalfPi));
		}

		[Test]
		public void LengthTest()
		{
			Assert.That(Vector2.Zero.Length, Is.EqualTo(0));
			Assert.That(Vector2.Half.Length, Is.EqualTo(Mathf.Sqrt(0.5f)));
			Assert.That(Vector2.One.Length, Is.EqualTo(Mathf.Sqrt(2)));
			Assert.That(Vector2.Zero.SqrLength, Is.EqualTo(0));
			Assert.That(Vector2.Half.SqrLength, Is.EqualTo(0.5f));
			Assert.That(Vector2.One.SqrLength, Is.EqualTo(2));
		}

		[Test]
		public void NormalizedTest()
		{
			var unitVector = new Vector2(1 / Mathf.Sqrt(2));
			Assert.That(Vector2.Zero.Normalized, Is.EqualTo(Vector2.Zero));
			Assert.That(Vector2.One.Normalized, Is.EqualTo(unitVector));
			Assert.That(Vector2.Half.Normalized, Is.EqualTo(unitVector));
		}
		
		[Test]
		public void ToStringTest()
		{
			Assert.That(Vector2.Zero.ToString(CultureInfo.InvariantCulture), Is.EqualTo("0, 0"));
			Assert.That(Vector2.Half.ToString(CultureInfo.InvariantCulture), Is.EqualTo("0.5, 0.5"));
			Assert.That(Vector2.One.ToString(CultureInfo.InvariantCulture), Is.EqualTo("1, 1"));
		}

		[Test]
		public void TryParseTest()
		{
			Vector2 vector;
			Assert.That(Vector2.TryParse("", out vector), Is.False);
			Assert.That(Vector2.TryParse("1, 2, 3", out vector), Is.False);
			Assert.That(Vector2.TryParse("1, ", out vector), Is.False);
			Assert.That(Vector2.TryParse("1.5, 1.5", out vector), Is.True);
			Assert.That(vector, Is.EqualTo(new Vector2(1.5f)));
		}

		[Test]
		public void ParseTest()
		{
			Assert.Catch<ArgumentNullException>(() => Vector2.Parse(null));
			Assert.Catch<FormatException>(() => Vector2.Parse(""));
			Assert.Catch<FormatException>(() => Vector2.Parse("1, 2, 3"));
			Assert.Catch<FormatException>(() => Vector2.Parse("1, "));
			Assert.That(Vector2.Parse("1.5, 1.5"), Is.EqualTo(new Vector2(1.5f)));
		}
	}
}
