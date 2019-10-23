using System;
using Lime;
using System.Collections.Generic;

namespace Orange
{
	public class RectAllocator
	{
		List<IntRectangle> rects = new List<IntRectangle>();

		int totalSquare;
		int allocatedSquare;

		public double GetPackRate() { return allocatedSquare / (double)totalSquare; }

		public RectAllocator(Size size)
		{
			totalSquare = size.Width * size.Height;
			rects.Add(new IntRectangle(0, 0, size.Width, size.Height));
		}

		public bool Allocate(Size size, out IntRectangle rect)
		{
			int j = -1;
			IntRectangle r;
			int spareSquare = Int32.MaxValue;
			for (int i = 0; i < rects.Count; i++) {
				r = rects[i];
				if (r.Width >= size.Width && r.Height >= size.Height) {
					int z = r.Width * r.Height - size.Width * size.Height;
					if (z < spareSquare) {
						j = i;
						spareSquare = z;
					}
				}
			}
			if (j < 0) {
				rect = IntRectangle.Empty;
				return false;
			}
			// Split the rest, minimizing the sum of parts perimeters.
			r = rects[j];
			rect = new IntRectangle(r.A.X, r.A.Y, r.A.X + size.Width, r.A.Y + size.Height);
			int a = 2 * r.Width + r.Height - size.Width;
			int b = 2 * r.Height + r.Width - size.Height;
			if (a < b) {
				rects[j] = new IntRectangle(r.A.X, r.A.Y + size.Height, r.B.X, r.B.Y);
				rects.Add(new IntRectangle(r.A.X + size.Width, r.A.Y, r.B.X, r.A.Y + size.Height));
			} else {
				rects[j] = new IntRectangle(r.A.X, r.A.Y + size.Height, r.A.X + size.Width, r.B.Y);
				rects.Add(new IntRectangle(r.A.X + size.Width, r.A.Y, r.B.X, r.B.Y));
			}
			allocatedSquare += size.Width * size.Height;
			return true;
		}
	}
}

