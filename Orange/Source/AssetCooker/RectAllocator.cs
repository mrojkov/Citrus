using System;
using Lime;
using System.Collections.Generic;

namespace Orange
{
	public class RectAllocator
	{
		List<IntRectangle> rects = new List<IntRectangle>();
		public Size Size;

		int totalSquare;
		int allocatedSquare;

		public double GetPackRate() { return allocatedSquare / (double)totalSquare; }

		public RectAllocator(Size size)
		{
			Size = size;
			totalSquare = size.Width * size.Height;
			rects.Add(new IntRectangle(0, 0, size.Width, size.Height));
		}

		public struct Padding
		{
			public int Left;
			public int Right;
			public int Top;
			public int Bottom;
		}

		public bool Allocate(Size size, int padding, out Padding effectivePadding, out IntRectangle rect)
		{
			int j = -1;
			IntRectangle r;
			effectivePadding = new Padding { Left = padding, Right = padding, Top = padding, Bottom = padding };
			int spareSquare = Int32.MaxValue;
			for (int i = 0; i < rects.Count; i++) {
				r = rects[i];
				// We don't have to pad from borders
				var leftPadding = r.Left == 0 ? 0 : padding;
				var topPadding = r.Top == 0 ? 0 : padding;
				var availableRightPadding = r.Width - (leftPadding + size.Width);
				var availableBottomPadding = r.Height - (topPadding + size.Height);
				if (
					r.Right != Size.Width && availableRightPadding < padding ||
					r.Bottom != Size.Height && availableBottomPadding < padding
				) {
					continue;
				}
				var currentEffectivePadding = new Padding {
					Left = leftPadding,
					Top = topPadding,
					Right = availableRightPadding.Clamp(0, padding),
					Bottom = availableBottomPadding.Clamp(0, padding)
				};
				if (
					r.Width >= currentEffectivePadding.Left + size.Width + currentEffectivePadding.Right
					&& r.Height >= currentEffectivePadding.Top + size.Height + currentEffectivePadding.Bottom
				) {
					effectivePadding = currentEffectivePadding;
					int z = r.Width * r.Height - (effectivePadding.Left + size.Width + effectivePadding.Right)
							* (effectivePadding.Top + size.Height + effectivePadding.Bottom);
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
			var effectiveWidth = effectivePadding.Left + size.Width + effectivePadding.Right;
			var effectiveHeight = effectivePadding.Top + size.Height + effectivePadding.Bottom;
			r = rects[j];
			rect = new IntRectangle(r.A.X, r.A.Y, r.A.X + effectiveWidth, r.A.Y + effectiveHeight);
			int a = 2 * r.Width + r.Height - effectiveWidth;
			int b = 2 * r.Height + r.Width - effectiveHeight;
			if (a < b) {
				rects[j] = new IntRectangle(r.A.X, r.A.Y + effectiveHeight, r.B.X, r.B.Y);
				rects.Add(new IntRectangle(r.A.X + effectiveWidth, r.A.Y, r.B.X, r.A.Y + effectiveHeight));
			} else {
				rects[j] = new IntRectangle(r.A.X, r.A.Y + effectiveHeight, r.A.X + effectiveWidth, r.B.Y);
				rects.Add(new IntRectangle(r.A.X + effectiveWidth, r.A.Y, r.B.X, r.B.Y));
			}
			allocatedSquare += effectiveWidth * effectiveHeight;
			return true;
		}
	}
}

