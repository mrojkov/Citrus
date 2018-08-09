using Yuzu;

namespace Lime
{
	public struct Thickness
	{
		[YuzuMember]
		public float Left { get { return LeftTop.X; } set { LeftTop.X = value; } }
		[YuzuMember]
		public float Top { get { return LeftTop.Y; } set { LeftTop.Y = value; } }
		[YuzuMember]
		public float Right { get { return RightBottom.X; } set { RightBottom.X = value; } }
		[YuzuMember]
		public float Bottom { get { return RightBottom.Y; } set { RightBottom.Y = value; } }

		public Vector2 LeftTop;
		public Vector2 RightBottom;

		public static Thickness Zero = new Thickness(0);

		public Thickness(float overall)
		{
			LeftTop.X = RightBottom.X = LeftTop.Y = RightBottom.Y = overall;
		}

		public Thickness(float horizontal, float vertical)
		{
			LeftTop.X = RightBottom.X = horizontal;
			LeftTop.Y = RightBottom.Y = vertical;
		}

		public static Vector2 operator + (Vector2 size, Thickness padding)
		{
			return size + padding.LeftTop + padding.RightBottom;
		}

		public static Vector2 operator - (Vector2 size, Thickness padding)
		{
			return size - padding.LeftTop - padding.RightBottom;
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}, {3}", Left, Top, Right, Bottom);
		}
	}
}
