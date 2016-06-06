using System;

namespace Lime
{
	public class LayoutCell
	{
		public Alignment Alignment;
		public int ColSpan = 1;
		public int RowSpan = 1;
		public Vector2 Stretch = Vector2.One;
		public float StretchX { get { return Stretch.X; } set { Stretch.X = value; } }
		public float StretchY { get { return Stretch.Y; } set { Stretch.Y = value; } }
		public static readonly LayoutCell Default = new LayoutCell();

		public LayoutCell() { }
		public LayoutCell(Alignment alignment) { Alignment = alignment; }
		public LayoutCell(Alignment alignment, float stretchX) : this(alignment, stretchX, 1) { }
		public LayoutCell(Alignment alignment, float stretchX, float stretchY)
		{
			Alignment = alignment;
			StretchX = stretchX;
			StretchY = stretchY;
		}
	}

	public struct Alignment
	{
		public HAlignment X;
		public VAlignment Y;

		public static readonly Alignment Center = new Alignment { X = HAlignment.Center, Y = VAlignment.Center };
		public static readonly Alignment LeftCenter = new Alignment { X = HAlignment.Left, Y = VAlignment.Center };
		public static readonly Alignment RightCenter = new Alignment { X = HAlignment.Right, Y = VAlignment.Center };
	}
}

