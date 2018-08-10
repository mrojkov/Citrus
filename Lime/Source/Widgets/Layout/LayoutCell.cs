using System;
using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class LayoutCellComponent : NodeComponent
	{
		[YuzuMember]
		public LayoutCell LayoutCell { get; set; }
	}

	public class LayoutCell
	{
		[TangerineIgnore]
		[YuzuMember]
		public Alignment Alignment { get; set; }

		[TangerineInspect]
		public HAlignment HorizontalAlignment
		{
			get => Alignment.X;
			set { Alignment = new Alignment { X = value, Y = Alignment.Y }; }
		}

		[TangerineInspect]
		public VAlignment VerticalAlignment
		{
			get => Alignment.Y;
			set { Alignment = new Alignment { X = Alignment.X, Y = value }; }
		}

		[YuzuMember]
		public int ColSpan { get; set; } = 1;

		[YuzuMember]
		public int RowSpan { get; set; } = 1;

		[YuzuMember]
		public Vector2 Stretch { get; set; } = Vector2.One;
		public float StretchX { get { return Stretch.X; } set { Stretch = new Vector2(value, Stretch.Y); } }
		public float StretchY { get { return Stretch.Y; } set { Stretch = new Vector2(Stretch.X, value); } }

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

	[YuzuCompact]
	public struct Alignment
	{
		[YuzuMember("0")]
		public HAlignment X { get; set; }

		[YuzuMember("1")]
		public VAlignment Y { get; set; }

		public static readonly Alignment LeftTop = new Alignment { X = HAlignment.Left, Y = VAlignment.Top };
		public static readonly Alignment Center = new Alignment { X = HAlignment.Center, Y = VAlignment.Center };
		public static readonly Alignment LeftCenter = new Alignment { X = HAlignment.Left, Y = VAlignment.Center };
		public static readonly Alignment RightCenter = new Alignment { X = HAlignment.Right, Y = VAlignment.Center };
	}
}

