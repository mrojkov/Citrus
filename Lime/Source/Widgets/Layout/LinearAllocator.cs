using System;
using System.Collections.Generic;

namespace Lime
{
	static class LinearAllocator
	{
		public static float[] Allocate(float availableSize, Constraints[] constraints, bool roundSizes)
		{
			var indices = new List<int>(constraints.Length);
			for (int i = 0; i < constraints.Length; i++) {
				indices.Add(i);
			}
			var sizes = new float[constraints.Length];
			while (indices.Count != 0) {
				var totalStretch = CalcTotalStretch(constraints, indices);
				if (totalStretch < 1e-5) {
					foreach (var i in indices) {
						sizes[i] = constraints[i].MinSize;
					}
					break;
				}
				float allocatedSize = 0;
				foreach (var i in indices) {
					var c = constraints[i];
					var s = availableSize * c.Stretch / totalStretch;
					s = Mathf.Clamp(s, c.MinSize, c.MaxSize);
					allocatedSize += (sizes[i] = s);
				}
				if (Math.Abs(allocatedSize - availableSize) < 0.1f) {
					// We perfectly fit the items into the designated space.
					break;
				}
				var containerOverfilled = allocatedSize > availableSize;
				for (int i = indices.Count - 1; i >= 0; i--) {
					var index = indices[i];
					var t = constraints[index];
					if (sizes[index] == (containerOverfilled ? t.MinSize : t.MaxSize)) {
						availableSize = Math.Max(0, availableSize - sizes[index]);
						indices.RemoveAt(i);
					}
				}
			}
			if (roundSizes) {
				float roundingError = 0;
				for (int i = 0; i < sizes.Length; i++) {
					var rs = sizes[i].Round();
					roundingError += rs - sizes[i];
					while (roundingError >= 1.0f && rs >= constraints[i].MinSize + 1) {
						roundingError -= 1.0f;
						rs -= 1;
					}
					while (roundingError <= -1.0f && rs <= constraints[i].MaxSize - 1) {
						roundingError += 1.0f;
						rs += 1;
					}
					sizes[i] = rs;
				}
			}
			return sizes;
		}

		private static float CalcTotalStretch(Constraints[] constraints, List<int> indices)
		{
			float ts = 0;
			foreach (var i in indices) {
				ts += constraints[i].Stretch;
			}
			return ts;
		}

		internal struct Constraints
		{
			public float MinSize;
			public float MaxSize;
			public float Stretch;
		}
	}
}

