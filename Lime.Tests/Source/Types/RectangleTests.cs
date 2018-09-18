using NUnit.Framework;

namespace Lime.Tests.Source.Types
{
	[TestFixture]
	public class RectangleTests
	{
		[Test]
		public void IntRectangleCastTest()
		{
			Assert.That((IntRectangle)Rectangle.Empty, Is.EqualTo(IntRectangle.Empty));
			Assert.That((IntRectangle)new Rectangle(0, 0, 1, 1), Is.EqualTo(new IntRectangle(0, 0, 1, 1)));
			Assert.That((IntRectangle)new Rectangle(Vector2.Zero, Vector2.One), Is.EqualTo(new IntRectangle(IntVector2.Zero, IntVector2.One)));
			Assert.That((IntRectangle)new Rectangle(0, 0, 1.5f, 1.5f), Is.EqualTo(new IntRectangle(IntVector2.Zero, IntVector2.One)));
		}

		[Test]
		public void EqualsTest()
		{
			var rectangleOne = new Rectangle(Vector2.Zero, Vector2.One);
			Assert.That(rectangleOne.Equals(new Rectangle(0, 0, 1, 1)));
			Assert.That(rectangleOne.Equals((object)new Rectangle(0, 0, 1, 1)));
			Assert.That(new Rectangle(0, 0, 1, 1) == rectangleOne);
			Assert.That(Rectangle.Empty != rectangleOne);
			Assert.That(rectangleOne.GetHashCode(), Is.EqualTo(rectangleOne.GetHashCode()));
		}

		[Test]
		public void SizeTest()
		{
			var rectangle = new Rectangle(Vector2.Zero, Vector2.One);
			Assert.That(rectangle.Size, Is.EqualTo(new Vector2(1, 1)));
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
			var rectangle = new Rectangle(Vector2.Zero, Vector2.One);
			Assert.That(new Vector2(rectangle.Right, rectangle.Bottom), Is.EqualTo(rectangle.B));
			Assert.That(new Vector2(rectangle.Left, rectangle.Top), Is.EqualTo(rectangle.A));
			var expectedCenter = rectangle.A + new Vector2(rectangle.Width / 2, rectangle.Height / 2);
			Assert.That(rectangle.Center, Is.EqualTo(expectedCenter));
			rectangle.Left++;
			rectangle.Top++;
			rectangle.Right++;
			rectangle.Bottom++;
			Assert.That(new Vector2(rectangle.Right, rectangle.Bottom), Is.EqualTo(rectangle.B));
			Assert.That(new Vector2(rectangle.Left, rectangle.Top), Is.EqualTo(rectangle.A));
			expectedCenter = rectangle.A + new Vector2(rectangle.Width / 2, rectangle.Height / 2);
			Assert.That(rectangle.Center, Is.EqualTo(expectedCenter));
		}

		[Test]
		public void NormalizedTest()
		{
			var rectangle = new Rectangle(Vector2.One, Vector2.Zero);
			Assert.That(rectangle.Width < 0);
			Assert.That(rectangle.Height < 0);
			rectangle = rectangle.Normalized;
			Assert.That(rectangle.Width > 0);
			Assert.That(rectangle.Height > 0);
		}

		[Test]
		public void ContainsTest()
		{
			var rectangle = new Rectangle(Vector2.Zero, Vector2.One);
			Assert.That(rectangle.Contains(rectangle.A));
			Assert.That(rectangle.Contains(Vector2.Half));
			Assert.That(rectangle.Contains(rectangle.B), Is.False);
			Assert.That(rectangle.Contains(rectangle.B + Vector2.One), Is.False);
		}

		[Test]
		public void IntersectTest()
		{
			var intersection = Rectangle.Intersect(new Rectangle(0, 0, 2, 2), new Rectangle(1, 1, 3, 3));
			var expectedIntersection = new Rectangle(1, 1, 2, 2);
			Assert.That(intersection, Is.EqualTo(expectedIntersection));
			intersection = Rectangle.Intersect(new Rectangle(0, 0, 3, 3), new Rectangle(1, 1, 2, 2));
			expectedIntersection = new Rectangle(1, 1, 2, 2);
			Assert.That(intersection, Is.EqualTo(expectedIntersection));
		}

		[Test]
		public void BoundsTest()
		{
			var bounds = Rectangle.Bounds(new Rectangle(0, 0, 2, 2), new Rectangle(1, 1, 3, 3));
			var expectedBounds = new Rectangle(0, 0, 3, 3);
			Assert.That(bounds, Is.EqualTo(expectedBounds));
			bounds = Rectangle.Bounds(new Rectangle(0, 0, 3, 3), new Rectangle(1, 1, 2, 2));
			expectedBounds = new Rectangle(0, 0, 3, 3);
			Assert.That(bounds, Is.EqualTo(expectedBounds));
		}

		[Test]
		public void IncludePointTest()
		{
			var rectangle = new Rectangle(0, 0, 1, 1).IncludingPoint(new Vector2(2, 2));
			var expextedRectangle = new Rectangle(0, 0, 2, 2);
			Assert.That(rectangle, Is.EqualTo(expextedRectangle));
			rectangle = new Rectangle(0, 0, 2, 2).IncludingPoint(new Vector2(1, 1));
			expextedRectangle = new Rectangle(0, 0, 2, 2);
			Assert.That(rectangle, Is.EqualTo(expextedRectangle));
		}

		[Test]
		public void ToStringTest()
		{
			Assert.That(Rectangle.Empty.ToString(), Is.EqualTo("0, 0, 0, 0"));
			Assert.That(new Rectangle(0.5f, 0.5f, 0.5f, 0.5f).ToString(), Is.EqualTo("0,5, 0,5, 0,5, 0,5"));
			Assert.That(new Rectangle(Vector2.Zero, Vector2.One).ToString(), Is.EqualTo("0, 0, 1, 1"));
		}

		[Test]
		public void TransformTest()
		{
			var rectangle = new Rectangle(Vector2.Zero, Vector2.One);
			rectangle = rectangle.Transform(Matrix32.Scaling(Vector2.Half));
			var expectedRectangle = new Rectangle(Vector2.Zero, Vector2.Half);
			Assert.That(rectangle, Is.EqualTo(expectedRectangle));
		}
	}
}
