using System;
using Yuzu;

namespace Lime
{
	public interface ILayoutCell
	{
		Alignment Alignment { get; set; }
		HAlignment HorizontalAlignment { get; set; }
		VAlignment VerticalAlignment { get; set; }
		int ColumnSpan { get; set; }
		int RowSpan { get; set; }
		Vector2 Stretch { get; set; }
		float StretchX { get; set; }
		float StretchY { get; set; }
		bool Ignore { get; set; }
		ILayoutCell Clone();
	}

	public class DefaultLayoutCell : Animable, ILayoutCell
	{
		public new Layout Owner
		{
			get => (Layout)base.Owner;
			set
			{
				((Layout)base.Owner)?.InvalidateConstraintsAndArrangement();
				base.Owner = value;
				((Layout)base.Owner)?.InvalidateConstraintsAndArrangement();
			}
		}

		[TangerineInspect]
		[YuzuMember]
		public Alignment Alignment
		{
			get => alignment;
			set {
				if (alignment != value) {
					alignment = value;
					Owner?.InvalidateConstraintsAndArrangement();
				}
			}
		}

		private Alignment alignment;

		[TangerineIgnore]
		public HAlignment HorizontalAlignment
		{
			get => Alignment.X;
			set { Alignment = new Alignment { X = value, Y = Alignment.Y }; }
		}

		[TangerineIgnore]
		public VAlignment VerticalAlignment
		{
			get => Alignment.Y;
			set { Alignment = new Alignment { X = Alignment.X, Y = value }; }
		}


		[YuzuMember]
		public int ColumnSpan
		{
			get => columnSpan;
			set {
				if (columnSpan != value) {
					columnSpan = value;
					Owner?.InvalidateConstraintsAndArrangement();
				}
			}
		}

		private int columnSpan = 1;

		[YuzuMember]
		public int RowSpan
		{
			get => rowSpan;
			set {
				if (rowSpan != value) {
					rowSpan = value;
					Owner?.InvalidateConstraintsAndArrangement();
				}
			}
		}

		private int rowSpan = 1;

		[YuzuMember]
		public Vector2 Stretch
		{
			get => stretch;
			set {
				if (stretch != value) {
					stretch = value;
					Owner?.InvalidateConstraintsAndArrangement();
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
			set {
				if (ignore != value) {
					ignore = value;
					Owner?.InvalidateConstraintsAndArrangement();
				}
			}
		}

		private bool ignore;

		public static readonly DefaultLayoutCell Default = new DefaultLayoutCell();

		public DefaultLayoutCell() { }
		public DefaultLayoutCell(Alignment alignment) { Alignment = alignment; }
		public DefaultLayoutCell(Alignment alignment, float stretchX) : this(alignment, stretchX, 1) { }
		public DefaultLayoutCell(Alignment alignment, float stretchX, float stretchY)
		{
			Alignment = alignment;
			StretchX = stretchX;
			StretchY = stretchY;
		}

		public new ILayoutCell Clone()
		{
			var clone = (DefaultLayoutCell)base.Clone();
			return clone;
		}
	}

	[AllowedComponentOwnerTypes(typeof(Widget))]
	[TangerineRegisterComponent]
	public class LayoutCell : NodeComponent, ILayoutCell
	{
		public new Widget Owner { get => (Widget)base.Owner; set => base.Owner = value; }

		[TangerineInspect]
		[YuzuMember]
		public Alignment Alignment
		{
			get => alignment;
			set
			{
				if (alignment != value) {
					alignment = value;
					Owner?.InvalidateParentConstraintsAndArrangement();
				}
			}
		}

		private Alignment alignment;

		[TangerineIgnore]
		public HAlignment HorizontalAlignment
		{
			get => Alignment.X;
			set { Alignment = new Alignment { X = value, Y = Alignment.Y }; }
		}

		[TangerineIgnore]
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
					Owner?.InvalidateParentConstraintsAndArrangement();
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
					Owner?.InvalidateParentConstraintsAndArrangement();
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
					Owner?.InvalidateParentConstraintsAndArrangement();
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
					Owner?.InvalidateParentConstraintsAndArrangement();
				}
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
			((Widget)oldOwner)?.InvalidateParentConstraintsAndArrangement();
			Owner?.InvalidateParentConstraintsAndArrangement();
		}

		ILayoutCell ILayoutCell.Clone()
		{
			return (ILayoutCell)Clone();
		}
	}

#pragma warning disable CS0660, CS0661
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
#pragma warning restore CS0660, CS0661
}

