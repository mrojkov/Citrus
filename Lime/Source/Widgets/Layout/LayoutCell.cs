using System;
using Yuzu;

namespace Lime
{
	[AllowedComponentOwnerTypes(typeof(Widget))]
	[TangerineRegisterComponent]
	public class LayoutCell : NodeComponent
	{
		[TangerineIgnore]
		[YuzuMember]
		public Alignment Alignment
		{
			get => alignment;
			set
			{
				if (alignment != value) {
					alignment = value;
					InvalidateLayout();
				}
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
		public int ColumnSpan
		{
			get => columnSpan;
			set
			{
				if (columnSpan != value) {
					columnSpan = value;
					InvalidateLayout();
				}
			}
		}

		private int columnSpan = 1;

		[YuzuMember]
		public int RowSpan
		{
			get => rowSpan;
			set
			{
				if (rowSpan != value) {
					rowSpan = value;
					InvalidateLayout();
				}
			}
		}

		private int rowSpan = 1;

		[YuzuMember]
		public Vector2 Stretch
		{
			get => stretch;
			set
			{
				if (stretch != value) {
					stretch = value;
					InvalidateLayout();
				}
			}
		}

		private Vector2 stretch = Vector2.One;

		public float StretchX { get { return Stretch.X; } set { Stretch = new Vector2(value, Stretch.Y); } }
		public float StretchY { get { return Stretch.Y; } set { Stretch = new Vector2(Stretch.X, value); } }

		[YuzuMember]
		public bool Ignore
		{
			get => ignore;
			set
			{
				if (ignore != value) {
					ignore = value;
					InvalidateLayout();
				}
			}
		}

		private bool ignore;
		internal bool IsOwnedByLayout { get; set; }
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
			InvalidateLayout(oldOwner as Widget);
			InvalidateLayout(Owner as Widget);
		}

		private void InvalidateLayout(Widget owner = null)
		{
			owner = owner ?? (Widget)Owner;
			if (IsOwnedByLayout) {
				owner?.Layout.InvalidateConstraintsAndArrangement();
			} else {
				owner?.InvalidateParentConstraintsAndArrangement();
			}
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

		public static bool operator == (Alignment lhs, Alignment rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y;
		public static bool operator !=(Alignment lhs, Alignment rhs) => lhs.X != rhs.X || lhs.Y != rhs.Y;
	}
}

