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

		public float[] Allocate(float availableSize, Constraints[] constraints)
		{
			var indices = new List<int>(constraints.Length);
			for (int i = 0; i < constraints.Length; i++) {
				indices.Add(i);
			}
			var sizes = new float[constraints.Length];
			AllocateHelper(availableSize, constraints, sizes, indices);
			return sizes;
		}

		private void AllocateHelper(float availableSize, Constraints[] constraints, float[] sizes, List<int> indices)
		{
			if (indices.Count == 0) {
				return;
			}
			var totalStretch = CalcTotalStretch(constraints, indices);
			float allocatedSize = 0;
			foreach (var i in indices) {
				var c = constraints[i];
				var s = availableSize * c.Stretch / totalStretch;
				if (roundSizes) {
					s = s.Round();
				}
				s = Mathf.Clamp(s, c.MinSize, c.MaxSize);
				allocatedSize += (sizes[i] = s);
			}
			if (Math.Abs(allocatedSize - availableSize) < 0.01f) {
				// We perfectly fit the items into the designated space.
				return;
			}
			var containerOverfilled = allocatedSize > availableSize;
			for (int i = indices.Count - 1; i >= 0; i--) {
				var t = constraints[i];
				if (sizes[i] == (containerOverfilled ? t.MinSize : t.MaxSize)) {
					availableSize = Math.Max(0, availableSize - sizes[i]);
					indices.RemoveAt(i);
				}
			}
			AllocateHelper(availableSize, constraints, sizes, indices);
		}

		private static float CalcTotalStretch(Constraints[] constraints, List<int> indices)
		{
			float ts = 0;
			foreach (var i in indices) {
				ts += constraints[i].Stretch;
			}
			if (ts == 0) {
				ts = 1;
			}
			return ts;
		}

		public struct Constraints
		{
			public float MinSize;
			public float MaxSize;
			public float Stretch;
		}
	}
}

