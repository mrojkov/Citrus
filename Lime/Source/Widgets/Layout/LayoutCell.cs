using System;
using Yuzu;

namespace Lime
{
	public class LayoutCell
	{
		[YuzuMember]
		public Alignment Alignment;
		[YuzuMember]
		public int ColSpan = 1;
		[YuzuMember]
		public int RowSpan = 1;
		[YuzuMember]
		public Vector2 Stretch = Vector2.One;
		public float StretchX { get { return Stretch.X; } set { Stretch.X = value; } }
		public float StretchY { get { return Stretch.Y; } set { Stretch.Y = value; } }
		[YuzuMember]
		public bool Ignore { get; set; }
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
		[YuzuMember]
		public HAlignment X;
		[YuzuMember]
		public VAlignment Y;

		public static readonly Alignment LeftTop = new Alignment { X = HAlignment.Left, Y = VAlignment.Top };
		public static readonly Alignment Center = new Alignment { X = HAlignment.Center, Y = VAlignment.Center };
		public static readonly Alignment LeftCenter = new Alignment { X = HAlignment.Left, Y = VAlignment.Center };
		public static readonly Alignment RightCenter = new Alignment { X = HAlignment.Right, Y = VAlignment.Center };
	}
}

