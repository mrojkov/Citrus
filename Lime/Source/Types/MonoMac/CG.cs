#if MONOMAC
using System;
using System.Drawing;

namespace Lime
{
	public class CGRect
	{
		private RectangleF rect;

		public CGRect(RectangleF rect)
		{
			this.rect = rect;
		}

		public CGRect(int x, int y, int width, int height)
		{
			this.rect = new RectangleF(x, y, width, height);
		}

		public CGRect(float x, float y, float width, float height)
		{
			this.rect = new RectangleF(x, y, width, height);
		}

		public static implicit operator RectangleF(CGRect rect)
		{
			return rect.rect;
		}
	}

	public class CGSize
	{
		private SizeF size;

		public CGSize(int width, int height)
		{
			this.size = new SizeF(width, height);
		}

		public static implicit operator SizeF(CGSize size)
		{
			return size.size;
		}
	}

	public class CGPoint
	{
		private PointF point;

		public CGPoint(int x, int y)
		{
			this.point = new PointF(x, y);
		}

		public static implicit operator PointF(CGPoint point)
		{
			return point.point;
		}
	}
}
#endif
