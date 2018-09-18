using NUnit.Framework;

namespace Lime.Tests.Source.Types
{
	[TestFixture]
	class IntRectangleTests
	{
		[Test]
		public void RectangleCastTest()
		{
			Assert.That((Rectangle)IntRectangle.Empty, Is.EqualTo(Rectangle.Empty));
			Assert.That((Rectangle)new IntRectangle(0, 0, 1, 1), 
				Is.EqualTo(new Rectangle(0, 0, 1, 1)));
			Assert.That((Rectangle)new IntRectangle(IntVector2.Zero, IntVector2.One), 
				Is.EqualTo(new Rectangle(Vector2.Zero, Vector2.One)));
		}

		[Test]
		public void WindowRectCastTest()
		{
			Assert.That((WindowRect)IntRectangle.Empty, Is.EqualTo(new WindowRect()));
			Assert.That((WindowRect)new IntRectangle(0, 0, 1, 1), 
				Is.EqualTo(new WindowRect {X = 0, Y = 0, Height = 1, Width = 1}));
			Assert.That((WindowRect)new IntRectangle(IntVector2.Zero, IntVector2.One), 
				Is.EqualTo(new WindowRect { X = 0, Y = 0, Height = 1, Width = 1 }));
		}

		[Test]
		public void EqualsTest()
		{
			var rectangleOne = new IntRectangle(IntVector2.Zero, IntVector2.One);
			Assert.That(rectangleOne.Equals(new IntRectangle(0, 0, 1, 1)));
			Assert.That(rectangleOne.Equals((object)new IntRectangle(0, 0, 1, 1)));
			Assert.That(new IntRectangle(0, 0, 1, 1) == rectangleOne);
			Assert.That(IntRectangle.Empty != rectangleOne);
			Assert.That(rectangleOne.GetHashCode(), Is.EqualTo(rectangleOne.GetHashCode()));
		}

		[Test]
		public void NormalizedTest()
		{
			var rectangle = new IntRectangle(IntVector2.One, IntVector2.Zero);
			Assert.That(rectangle.Width < 0);
			Assert.That(rectangle.Height < 0);
			rectangle = rectangle.Normalized;
			Assert.That(rectangle.Width > 0);
			Assert.That(rectangle.Height > 0);
		}

		[Test]
		public void SizeTest()
		{
			var rectangle = new IntRectangle(IntVector2.Zero, IntVector2.One);
			Assert.That(rectangle.Size, Is.EqualTo(new IntVector2(1, 1)));
			Assert.That(rectangle.Width, Is.EqualTo(1));
			Assert.That(rectangle.Height, Is.EqualTo(1));
			rectangle.Width++;
			rectangle.Height++;
			Assert.That(rectangle.Width, Is.EqualTo(2));
			Assert.That(rectangle.Height, Is.EqualTo(2));
		}

		[Test]
		public void SidesTest()
		{
			var rectangle = new IntRectangle(IntVector2.Zero, IntVector2.One);
			Assert.That(new IntVector2(rectangle.Right, rectangle.Bottom), Is.EqualTo(rectangle.B));
			Assert.That(new IntVector2(rectangle.Left, rectangle.Top), Is.EqualTo(rectangle.A));
			var expectedCenter = rectangle.A + new IntVector2(rectangle.Width / 2, rectangle.Height / 2);
			Assert.That(rectangle.Center, Is.EqualTo(expectedCenter));
			rectangle.Left++;
			rectangle.Top++;
			rectangle.Right++;
			rectangle.Bottom++;
			Assert.That(new IntVector2(rectangle.Right, rectangle.Bottom), Is.EqualTo(rectangle.B));
			Assert.That(new IntVector2(rectangle.Left, rectangle.Top), Is.EqualTo(rectangle.A));
			expectedCenter = rectangle.A + new IntVector2(rectangle.Width / 2, rectangle.Height / 2);
			Assert.That(rectangle.Center, Is.EqualTo(expectedCenter));
		}

		[Test]
		public void ContainsTest()
		{
			var rectangle = new IntRectangle(IntVector2.Zero, IntVector2.One);
			Assert.That(rectangle.Contains(rectangle.A));
			Assert.That(rectangle.Contains(rectangle.B), Is.False);
			Assert.That(rectangle.Contains(rectangle.B + IntVector2.One), Is.False);
		}

		[Test]
		public void IntersectTest()
		{
			var intersection = IntRectangle.Intersect(new IntRectangle(0, 0, 2, 2), new IntRectangle(1, 1, 3, 3));
			var expectedIntersection = new IntRectangle(1, 1, 2, 2);
			Assert.That(intersection, Is.EqualTo(expectedIntersection));
			intersection = IntRectangle.Intersect(new IntRectangle(0, 0, 3, 3), new IntRectangle(1, 1, 2, 2));
			expectedIntersection = new IntRectangle(1, 1, 2, 2);
			Assert.That(intersection, Is.EqualTo(expectedIntersection));
		}

		[Test]
		public void OffsetByTest()
		{
			var rectangle = new IntRectangle(0, 0, 1, 1).OffsetBy(new IntVector2(2, 2));
			var expextedRectangle = new IntRectangle(2, 2, 3, 3);
			Assert.That(rectangle, Is.EqualTo(expextedRectangle));
		}

		[Test]
		public void ToStringTest()
		{
			Assert.That(IntRectangle.Empty.ToString(), Is.EqualTo("0, 0, 0, 0"));
			Assert.That(new IntRectangle(1, 1, 2, 2).ToString(), Is.EqualTo("1, 1, 2, 2"));
			Assert.That(new IntRectangle(IntVector2.Zero, IntVector2.One).ToString(), Is.EqualTo("0, 0, 1, 1"));
		}
	}
}
