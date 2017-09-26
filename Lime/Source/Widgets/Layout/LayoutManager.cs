using System;
using System.Collections.Generic;

namespace Lime
{
	public class LayoutManager
	{
		// ArrangeQueue has root-nodes first order to minimize ArrangeChildren calls.
		private DepthOrderedQueue arrangeQueue = new DepthOrderedQueue(rootToLeavesOrder: true);
		// MeasureQueue has leaf-nodes first order, because widget size constraints depends only on the widget's children constraints.
		private DepthOrderedQueue measureQueue = new DepthOrderedQueue(rootToLeavesOrder: false);

		public void AddToArrangeQueue(Widget widget)
		{
			arrangeQueue.Enqueue(widget);
		}

		public void AddToMeasureQueue(Widget widget)
		{
			measureQueue.Enqueue(widget);
		}

		public void Layout()
		{
			while (true) {
				var w = measureQueue.Dequeue();
				if (w == null) {
					break;
				}
				// Keep in mind: MeasureConstraints could force a parent constraints
				// invalidation when child constraints has changed.
				// See MinSize/MaxSize setters.
				w.Layout.MeasureSizeConstraints(w);
			}
			while (true) {
				var w = arrangeQueue.Dequeue();
				if (w == null) {
					break;
				}
				// Keep in mind: ArrangeChildren could force a child re-arrangement when changes a child size.
				// See ILayout.OnSizeChanged implementation.
				w.Layout.ArrangeChildren(w);
			}
		}

		class DepthOrderedQueue
		{
			const int MaxDepth = 20;
			private List<Widget>[] buckets = new List<Widget>[MaxDepth];
			private bool rootToLeavesOrder;

			public DepthOrderedQueue(bool rootToLeavesOrder)
			{
				this.rootToLeavesOrder = rootToLeavesOrder;
			}

			public void Enqueue(Widget widget)
			{
				int d = Math.Min(MaxDepth - 1, CalcNodeDepth(widget));
				if (!rootToLeavesOrder) {
					d = MaxDepth - 1 - d;
				}
				if (buckets[d] == null) {
					buckets[d] = new List<Widget>();
				}
				buckets[d].Add(widget);
			}

			public Widget Dequeue()
			{
				for (int i = 0; i < MaxDepth; i++) {
					var bucket = buckets[i];
					if (bucket == null)
						continue;
					int c = bucket.Count;
					if (c > 0) {
						var widget = bucket[c - 1];
						bucket.RemoveAt(c - 1);
						return widget;
					}
				}
				return null;
			}

			private int CalcNodeDepth(Node item)
			{
				int d = 0;
				while (item != null) {
					d++;
					item = item.Parent;
				}
				return d - 1;
			}
		}
	}
}