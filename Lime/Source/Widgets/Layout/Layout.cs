using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yuzu;

namespace Lime
{
	public interface ILayout
	{
		List<Rectangle> DebugRectangles { get; }

		Widget Owner { get; set; }

		bool ConstraintsValid { get; }
		bool ArrangementValid { get; }

		void OnSizeChanged(Widget widget, Vector2 sizeDelta);
		void InvalidateArrangement();
		void InvalidateConstraintsAndArrangement();
		void MeasureSizeConstraints();
		void ArrangeChildren();
	}

	[MutuallyExclusiveDerivedComponents]
	[AllowedComponentOwnerTypes(typeof(Widget))]
	[YuzuDontGenerateDeserializer]
	public class Layout : NodeComponent, ILayout
	{
		public new Widget Owner
		{
			get => (Widget)base.Owner;
			set => base.Owner = value;
		}

		public List<Rectangle> DebugRectangles { get; protected set; }

		public bool ConstraintsValid { get; protected set; }
		public bool ArrangementValid { get; protected set; }

		[YuzuMember]
		public LayoutCell DefaultCell
		{
			get => defaultCell;
			set
			{
				if (defaultCell != null) {
					defaultCell.Owner = null;
				}
				defaultCell = value;
				if (defaultCell != null) {
					defaultCell.Owner = Owner;
				}
			}
		}

		private LayoutCell defaultCell;

		[YuzuMember]
		public bool IgnoreHidden
		{
			get => ignoreHidden;
			set {
				if (ignoreHidden != value) {
					ignoreHidden = value;
					InvalidateConstraintsAndArrangement();
				}
			}
		}

		private bool ignoreHidden;

		public Layout()
		{
			ignoreHidden = true;
			// Make it true by default, because we want the first Invalidate() to add it to the layout queue.
			ConstraintsValid = ArrangementValid = true;
		}

		public void InvalidateConstraintsAndArrangement()
		{
			if (Owner == null) {
				return;
			}
			if (Owner.LayoutManager != null && ConstraintsValid) {
				ConstraintsValid = false;
				Owner.LayoutManager.AddToMeasureQueue(this);
			}
			InvalidateArrangement();
		}

		public void InvalidateArrangement()
		{
			if (Owner == null) {
				return;
			}
			if (Owner.LayoutManager != null && ArrangementValid) {
				ArrangementValid = false;
				Owner.LayoutManager.AddToArrangeQueue(this);
			}
		}

		public virtual void OnSizeChanged(Widget widget, Vector2 sizeDelta)
		{
			// Size changing could only affect children arrangement, not the widget's size constraints.
			InvalidateArrangement();
		}

		public virtual void MeasureSizeConstraints() { }

		public virtual void ArrangeChildren() { }

		public override NodeComponent Clone()
		{
			Layout clone = (Layout)base.Clone();
			clone.DebugRectangles = null;
			clone.defaultCell = null;
			clone.DefaultCell = (LayoutCell)DefaultCell?.Clone();
			clone.ConstraintsValid = false;
			clone.ArrangementValid = false;
			return clone;
		}

		protected List<Widget> GetChildren()
		{
			return Owner.Nodes.OfType<Widget>().Where(
				i => (!IgnoreHidden || i.Visible) &&
				!(i.LayoutCell ?? LayoutCell.Default).Ignore
			).ToList();
		}

		protected static void LayoutWidgetWithinCell(Widget widget, Vector2 position, Vector2 size, Alignment alignment, List<Rectangle> debugRectangles = null)
		{
			if (debugRectangles != null) {
				debugRectangles.Add(new Rectangle { A = position, B = position + size });
			}
			var innerSize = Vector2.Clamp(size, widget.EffectiveMinSize, widget.EffectiveMaxSize);
			if (alignment.X == HAlignment.Right) {
				position.X += size.X - innerSize.X;
			} else if (alignment.X == HAlignment.Center) {
				position.X += ((size.X - innerSize.X) / 2).Round();
			}
			if (alignment.Y == VAlignment.Bottom) {
				position.Y += size.Y - innerSize.Y;
			} else if (alignment.Y == VAlignment.Center) {
				position.Y += ((size.Y - innerSize.Y) / 2).Round();
			}
			widget.Position = position;
			widget.Size = innerSize;
			widget.Pivot = Vector2.Zero;
		}

		protected LayoutCell EffectiveLayoutCell(Widget widget)
		{
			return widget.LayoutCell ?? DefaultCell ?? LayoutCell.Default;
		}

		protected override void OnOwnerChanged(Node oldOwner)
		{
			base.OnOwnerChanged(oldOwner);
			if (Owner != null) {
				InvalidateConstraintsAndArrangement();
			}
			if (oldOwner != null) {
				var w = (Widget)oldOwner;
				(w).Layout.InvalidateConstraintsAndArrangement();
			}
			if (defaultCell != null) {
				defaultCell.Owner = Owner;
			}
		}
	}
}
