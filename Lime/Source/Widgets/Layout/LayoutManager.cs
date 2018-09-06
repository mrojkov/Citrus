using System;
using System.Collections.Generic;

namespace Lime
{
	public class LayoutManager
	{
		// ArrangeQueue has root-nodes first order to minimize ArrangeChildren calls.
		private readonly DepthOrderedQueue arrangeQueue = new DepthOrderedQueue(rootToLeavesOrder: true);
		// MeasureQueue has leaf-nodes first order, because widget size constraints depends only on the widget's children constraints.
		private readonly DepthOrderedQueue measureQueue = new DepthOrderedQueue(rootToLeavesOrder: false);

		public void AddToArrangeQueue(ILayout layout)
		{
			arrangeQueue.Enqueue(layout);
		}

		public void AddToMeasureQueue(ILayout layout)
		{
			measureQueue.Enqueue(layout);
		}

		public void Layout()
		{
			while (true) {
				var l = measureQueue.Dequeue();
				if (l == null) {
					break;
				}
				// Ignore this layout in case its orphaned now. This may happen when we somehow trigger Layout.InvalidateConstraintsAndArrangement
				// and replace given layout on the same frame.
				if (l.Owner == null) {
					continue;
				}
				// Keep in mind: MeasureConstraints could force a parent constraints
				// invalidation when child constraints has changed.
				// See MinSize/MaxSize setters.
				l.MeasureSizeConstraints();
			}
			while (true) {
				var l = arrangeQueue.Dequeue();
				if (l == null) {
					break;
				}
				if (l.Owner == null) {
					continue;
				}
				// Keep in mind: ArrangeChildren could force a child re-arrangement when changes a child size.
				// See ILayout.OnSizeChanged implementation.
				l.ArrangeChildren();
			}
		}

		class DepthOrderedQueue
		{
			const int MaxDepth = 20;
			private List<ILayout>[] buckets = new List<ILayout>[MaxDepth];
			private readonly bool rootToLeavesOrder;

			public DepthOrderedQueue(bool rootToLeavesOrder)
			{
				this.rootToLeavesOrder = rootToLeavesOrder;
			}

			public void Enqueue(ILayout layout)
			{
				int d = Math.Min(MaxDepth - 1, CalcNodeDepth(layout.Owner));
				if (!rootToLeavesOrder) {
					d = MaxDepth - 1 - d;
				}
				if (buckets[d] == null) {
					buckets[d] = new List<ILayout>();
				}
				buckets[d].Add(layout);
			}

			public ILayout Dequeue()
			{
				for (int i = 0; i < MaxDepth; i++) {
					var bucket = buckets[i];
					if (bucket == null)
						continue;
					int c = bucket.Count;
					if (c > 0) {
						var layout = bucket[c - 1];
						bucket.RemoveAt(c - 1);
						return layout;
					}
				}
				return null;
			}

			private static int CalcNodeDepth(Node item)
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
