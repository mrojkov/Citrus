using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
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

	public class CommonLayout
	{
		public List<Rectangle> DebugRectangles { get; protected set; }

		public bool ConstraintsValid { get; protected set; }
		public bool ArrangementValid { get; protected set; }

		public CommonLayout()
		{
			// Make it true by default, because we want the first Invalidate() to add it to the layout queue.
			ConstraintsValid = ArrangementValid = true;
		}

		public void InvalidateConstraintsAndArrangement(Widget widget)
		{
			if (ConstraintsValid) {
				ConstraintsValid = false;
				LayoutManager.Instance.AddToMeasureQueue(widget);
			}
			InvalidateArrangement(widget);
		}

		public void InvalidateArrangement(Widget widget)
		{
			if (ArrangementValid) {
				ArrangementValid = false;
				LayoutManager.Instance.AddToArrangeQueue(widget);
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
		protected static List<Widget> GetChildren(Widget widget)
		{
			return widget.Nodes.OfType<Widget>().Where(i => i.Visible).ToList();
		}

		protected static void LayoutWidgetWithinCell(Widget widget, Vector2 position, Vector2 size, List<Rectangle> debugRectangles)
		{
			debugRectangles.Add(new Rectangle { A = position, B = position + size });
			var halign = GetCellData(widget).Alignment.X;
			var valign = GetCellData(widget).Alignment.Y;
			var innerSize = Vector2.Clamp(size, widget.MinSize, widget.MaxSize);
			if (halign == HAlignment.Right) {
				position.X += size.X - innerSize.X;
			} else if (halign == HAlignment.Center) {
				position.X += ((size.X - innerSize.X) / 2).Round();
			}
			if (valign == VAlignment.Bottom) {
				position.Y += size.Y - innerSize.Y;
			} else if (valign == VAlignment.Center) {
				position.Y += ((size.Y - innerSize.Y) / 2).Round();
			}
			widget.Position = position;
			widget.Size = innerSize;
			widget.Pivot = Vector2.Zero;
		}

		protected static LayoutCell GetCellData(Widget cell)
		{
			return cell.LayoutCell ?? Lime.LayoutCell.Default;
		}
#endregion
	}
}
