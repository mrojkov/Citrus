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

		public virtual void MeasureSizeConstraints(Widget widget) { }

		public virtual void ArrangeChildren(Widget widget) { }
	}
}
