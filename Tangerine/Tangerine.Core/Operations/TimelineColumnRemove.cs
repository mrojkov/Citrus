using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.Core.Operations
{
	public static class TimelineColumnRemove
	{
		public static void Perform(int column)
		{
			var container = Document.Current.Container;
			foreach (var node in container.Nodes) {
				var occupied = new HashSet<int>();
				if (node.Animators.Count == 0) {
					continue;
				}
				foreach (var animator in node.Animators.Where(i => i.AnimationId == Document.Current.AnimationId)) {
					foreach (var k in animator.ReadonlyKeys) {
						occupied.Add(k.Frame);
					}
				}
				int removeAt = occupied.Max() + 1;
				for (int i = column - 1; ; ++i) {
					if (!occupied.Contains(i)) {
						removeAt = i;
						break;
					}
				}
				foreach (var animator in node.Animators.Where(i => i.AnimationId == Document.Current.AnimationId).ToList()) {
					var keys = animator.ReadonlyKeys.ToList();
					foreach (var k in keys) {
						if (k.Frame >= removeAt) {
							var k1 = k.Clone();
							k1.Frame -= 1;
							SetKeyframe.Perform(animator, k1);
							RemoveKeyframe.Perform(animator, k.Frame);
						}
					}
				}
			}
			var markers = container.Markers.ToList();
			if (markers.Count == 0) {
				return;
			}
			var markersOccupied = new HashSet<int>();
			foreach (var m in markers) {
				markersOccupied.Add(m.Frame);
			}
			int markersRemoveAt = markersOccupied.Max() + 1;
			for (int i = column - 1; ; ++i) {
				if (!markersOccupied.Contains(i)) {
					markersRemoveAt = i;
					break;
				}
			}
			foreach (var m in markers) {
				if (m.Frame >= markersRemoveAt) {
					var m1 = m.Clone();
					m1.Frame -= 1;
					DeleteMarker.Perform(container, m, false);
					SetMarker.Perform(container, m1, false);
				}
			}
		}
	}
}
