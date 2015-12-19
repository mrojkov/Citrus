using System;
using System.Collections.Generic;

namespace Lime
{
	class LinearAllocator
	{
		private bool roundSizes;

		public LinearAllocator(bool roundSizes)
		{
			this.roundSizes = roundSizes;
		}

		public void Allocate(float availableSize, List<Constraints> items)
		{
			var copy = new List<Constraints>(items.Count);
			copy.AddRange(items);
			AllocateHelper(availableSize, copy);
			if (roundSizes) {
				foreach (var i in items) {
					i.Size = i.Size.Round();
				}
			}
		}

		private void AllocateHelper(float availableSize, List<Constraints> items)
		{
			if (items.Count == 0) {
				return;
			}
			NormalizeStretchFactors(items);
			float allocatedSize = 0;
			foreach (var i in items) {
				i.Size = Mathf.Clamp(availableSize * i.Stretch, i.MinSize, i.MaxSize);
				allocatedSize += i.Size;
			}
			if (Math.Abs(allocatedSize - availableSize) < 0.01f) {
				// We perfectly fit the items into the designated space.
				return;
			}
			var containerOverfilled = allocatedSize > availableSize;
			for (int i = items.Count - 1; i >= 0; i--) {
				var t = items[i];
				if (t.Size == (containerOverfilled ? t.MinSize : t.MaxSize)) {
					items.RemoveAt(i);
					availableSize = Math.Max(0, availableSize - t.Size);
				}
			}
			AllocateHelper(availableSize, items);
		}

		private static void NormalizeStretchFactors(List<Constraints> items)
		{
			float totalStretch = 0;
			foreach (var i in items) {
				totalStretch += i.Stretch;
			}
			if (totalStretch > 0) {
				foreach (var i in items) {
					i.Stretch /= totalStretch;
				}
			}
		}

		public class Constraints
		{
			public float MinSize;
			public float MaxSize;
			public float Stretch;
			public float Size;
		}
	}
}

