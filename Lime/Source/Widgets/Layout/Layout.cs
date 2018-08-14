using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class LayoutComponent : NodeComponent
	{
		private ILayout layout;

		[YuzuMember]
		public ILayout Layout
		{
			get => layout;
			set
			{
				layout = value;
				if (Owner != null) {
					layout?.InvalidateConstraintsAndArrangement(Owner as Widget);
				}
			}
		}

		protected override void OnOwnerChanged(Node oldOwner)
		{
			base.OnOwnerChanged(oldOwner);
			if (Owner != null) {
				layout?.InvalidateConstraintsAndArrangement(Owner as Widget);
			} else if (oldOwner != null) {
				(oldOwner as Widget).Layout.InvalidateConstraintsAndArrangement(oldOwner as Widget);
			}
		}
	}

	public interface ILayout
	{
		List<Rectangle> DebugRectangles { get; }

		bool ConstraintsValid { get; }
		bool ArrangementValid { get; }

		void OnSizeChanged(Widget widget, Vector2 sizeDelta);
		void InvalidateArrangement(Widget widget);
		void InvalidateConstraintsAndArrangement(Widget widget);
		void MeasureSizeConstraints(Widget widget);
		void ArrangeChildren(Widget widget);
	}

	[YuzuDontGenerateDeserializer]
	[TangerineIgnore]
	public class CommonLayout
	{
		public List<Rectangle> DebugRectangles { get; protected set; }

		public bool ConstraintsValid { get; protected set; }
		public bool ArrangementValid { get; protected set; }

		[YuzuMember]
		public bool IgnoreHidden { get; set; }

		public CommonLayout()
		{
			IgnoreHidden = true;
			// Make it true by default, because we want the first Invalidate() to add it to the layout queue.
			ConstraintsValid = ArrangementValid = true;
		}

		public void InvalidateConstraintsAndArrangement(Widget widget)
		{
			if (widget.LayoutManager != null && ConstraintsValid) {
				ConstraintsValid = false;
				widget.LayoutManager.AddToMeasureQueue(widget);
			}
			InvalidateArrangement(widget);
		}

		public void InvalidateArrangement(Widget widget)
		{
			if (widget.LayoutManager != null && ArrangementValid) {
				ArrangementValid = false;
				widget.LayoutManager.AddToArrangeQueue(widget);
			}
		}

		public virtual void OnSizeChanged(Widget widget, Vector2 sizeDelta)
		{
			// Size changing could only affect children arrangement, not the widget's size constraints.
			InvalidateArrangement(widget);
		}

		public virtual void MeasureSizeConstraints(Widget widget) { }

		public virtual void ArrangeChildren(Widget widget) { }

#region protected methods
		protected List<Widget> GetChildren(Widget widget)
		{
			return widget.Nodes.OfType<Widget>().Where(
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
#endregion
	}
}
