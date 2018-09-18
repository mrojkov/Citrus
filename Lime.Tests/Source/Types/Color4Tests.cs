using NUnit.Framework;

namespace Lime.Tests.Source.Types
{
	[TestFixture]
	class Color4Tests
	{
		[Test]
		public void ConstructorTest()
		{
			var color1 = new Color4(255, 127, 25, 0);
			var color2 = new Color4(0x00197FFF);
			var color3 = Color4.FromFloats(1, 0.5f, 0.1F, 0);
			Assert.That(color1.A, Is.EqualTo(0).And.EqualTo(color2.A).And.EqualTo(color3.A));
			Assert.That(color1.B, Is.EqualTo(25).And.EqualTo(color2.B).And.EqualTo(color3.B));
			Assert.That(color1.G, Is.EqualTo(127).And.EqualTo(color2.G).And.EqualTo(color3.G));
			Assert.That(color1.R, Is.EqualTo(255).And.EqualTo(color2.R).And.EqualTo(color3.R));
			Assert.That(color1.ABGR, Is.EqualTo(0x00197FFF).And.EqualTo(color2.ABGR).And.EqualTo(color3.ABGR));
		}

		[Test]
		public void EqualsTest()
		{
			Assert.That(Color4.White.Equals(Color4.White));
		}

		[Test]
		public void MultiplicationTest()
		{
			Assert.That(Color4.White * Color4.Black, Is.EqualTo(Color4.Black));
			Assert.That(Color4.Black * Color4.Green, Is.EqualTo(Color4.Black));
			Assert.That(Color4.Blue * Color4.White, Is.EqualTo(Color4.Blue));
			Assert.That(Color4.Gray * Color4.Gray, Is.EqualTo(Color4.DarkGray));
		}

		[Test]
		public void PremulAlphaTest()
		{
			Assert.That(Color4.PremulAlpha(Color4.White), Is.EqualTo(Color4.White));
			Assert.That(Color4.PremulAlpha(Color4.Black), Is.EqualTo(Color4.Black));
			var halfTransparentWhite = Color4.White;
			halfTransparentWhite.A = 128;
			var halfTransparentGray = Color4.Gray;
			halfTransparentGray.A = 128;
			Assert.That(Color4.PremulAlpha(halfTransparentWhite), Is.EqualTo(halfTransparentGray));
		}

		[Test]
		public void LerpTest()
		{
			Assert.That(Color4.Lerp(0, Color4.Orange, Color4.Black), Is.EqualTo(Color4.Orange));
			Assert.That(Color4.Lerp(0.5f, Color4.Gray, Color4.Black), Is.EqualTo(Color4.DarkGray));
			Assert.That(Color4.Lerp(1, Color4.Black, Color4.Black), Is.EqualTo(Color4.Black));
		}

		[Test]
		public void ToStringTest()
		{
			Assert.That(Color4.White.ToString(), Is.EqualTo("#FF.FF.FF.FF"));
			Assert.That(Color4.Black.ToString(), Is.EqualTo("#00.00.00.FF"));
			Assert.That(Color4.Orange.ToString(), Is.EqualTo("#FF.80.00.FF"));
		}
	}
}
