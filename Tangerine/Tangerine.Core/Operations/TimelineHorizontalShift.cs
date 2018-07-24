using System;
using System.Linq;
using Lime;

namespace Tangerine.Core.Operations
{
	public static class TimelineHorizontalShift
	{
		public static void Perform(int column, int direction)
		{
			var container = Document.Current.Container;
			foreach (var node in container.Nodes) {
				foreach (var animator in node.Animators.Where(i => i.AnimationId == Document.Current.AnimationId).ToList()) {
					var keys = direction > 0 ? animator.ReadonlyKeys.Reverse() : animator.ReadonlyKeys;
					foreach (var k in keys.ToList()) {
						if (k.Frame >= column) {
							var k1 = k.Clone();
							k1.Frame += direction.Sign();
							if (direction > 0 || k.Frame > column) {
								SetKeyframe.Perform(animator, k1);
							}
							// Order is importent. RemoveKeyframe must be after SetKeyframe,
							// to prevent animator clean up if all keys were removed.
							RemoveKeyframe.Perform(animator, k.Frame);
						}
					}
				}
			}
			var markers = container.Markers.ToList();
			if (direction > 0) {
				markers.Reverse();
			}
			foreach (var m in markers) {
				if (m.Frame >= column) {
					var m1 = m.Clone();
					m1.Frame += direction.Sign();
					bool isReSet = direction > 0 || m.Frame > column;

					DeleteMarker.Perform(container, m, !isReSet);
					if (isReSet) {
						SetMarker.Perform(container, m1, false);
					}
				}
			}
		}
	}
}
