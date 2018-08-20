using System;
using Yuzu;

namespace Lime
{
	[AllowedComponentOwnerTypes(typeof(Widget))]
	[TangerineRegisterComponent]
	public class LayoutCell : NodeComponent
	{
		public new Widget Owner { get => (Widget)base.Owner; set { base.Owner = value; } }

		[TangerineIgnore]
		[YuzuMember]
		public Alignment Alignment
		{
			get => alignment;
			set
			{
				alignment = value;
				Owner?.InvalidateParentConstraintsAndArrangement();
			}
		}

		private Alignment alignment;

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
		public int ColSpan
		{
			get => colSpan;
			set {
				colSpan = value;
				Owner?.InvalidateParentConstraintsAndArrangement();
			}
		}

		private int colSpan = 1;

		[YuzuMember]
		public int RowSpan
		{
			get => rowSpan;
			set {
				rowSpan = value;
				Owner?.InvalidateParentConstraintsAndArrangement();
			}
		}

		private int rowSpan = 1;

		[YuzuMember]
		public Vector2 Stretch
		{
			get => stretch;
			set {
				stretch = value;
				Owner?.InvalidateParentConstraintsAndArrangement();
			}
		}

		private Vector2 stretch = Vector2.One;

		public float StretchX { get { return Stretch.X; } set { Stretch = new Vector2(value, Stretch.Y); } }
		public float StretchY { get { return Stretch.Y; } set { Stretch = new Vector2(Stretch.X, value); } }

		[YuzuMember]
		public bool Ignore
		{
			get => ignore;
			set {
				ignore = value;
				Owner?.InvalidateParentConstraintsAndArrangement();
			}
		}

		private bool ignore;

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

		protected override void OnOwnerChanged(Node oldOwner)
		{
			base.OnOwnerChanged(oldOwner);
			Owner?.InvalidateParentConstraintsAndArrangement();
			(oldOwner as Widget)?.InvalidateParentConstraintsAndArrangement();
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

