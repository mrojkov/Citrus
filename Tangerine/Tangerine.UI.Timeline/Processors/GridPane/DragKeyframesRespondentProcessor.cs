using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline
{
	public class DragKeyframesRespondentProcessor : ITaskProvider
	{
		public IEnumerator<object> Task()
		{
			var g = Timeline.Instance.Globals;
			while (true) {
				var r = g.Get<DragKeyframesRequest>();
				if (r != null) {
					DragKeys(r.Offset, r.RemoveOriginals);
					ShiftSelection(r.Offset);
					g.Remove<DragKeyframesRequest>();
				}
				yield return null;
			}
		}

		static void ShiftSelection(IntVector2 offset)
		{
			Operations.ShiftGridSelection.Perform(offset);
		}

		static void DragKeys(IntVector2 offset, bool removeOriginals)
		{
			var processedKeys = new HashSet<IKeyframe>();
			var operations = new List<Action>();
			foreach (var row in Document.Current.Rows) {
				var spans = row.Components.GetOrAdd<GridSpanListComponent>().Spans;
				foreach (var span in spans.GetNonOverlappedSpans()) {
					var node = row.Components.Get<NodeRow>()?.Node ?? row.Components.Get<PropertyRow>()?.Node;
					if (node == null) {
						continue;
					}
					var property = row.Components.Get<PropertyRow>()?.Animator.TargetProperty;
					foreach (var a in node.Animators) {
						if (property != null && a.TargetProperty != property) {
							continue;
						}
						foreach (var k in a.Keys.Where(k => k.Frame >= span.A && k.Frame < span.B)) {
							if (processedKeys.Contains(k)) {
								continue;
							}
							processedKeys.Add(k);
							if (removeOriginals) {
								operations.Insert(0, () => Core.Operations.RemoveKeyframe.Perform(a, k.Frame));
							}
							var destRow = row.Index + offset.Y;
							if (!CheckRowRange(destRow)) {
								continue;
							}
							var destRowComponents = Document.Current.Rows[destRow].Components;
							var destNode = destRowComponents.Get<NodeRow>()?.Node ?? destRowComponents.Get<PropertyRow>()?.Node;
							if (destNode == null || !ArePropertiesCompatible(node, destNode, a.TargetProperty)) {
								continue;
							}
							if (k.Frame + offset.X >= 0) {
								var k1 = k.Clone();
								k1.Frame += offset.X;
								operations.Add(() => Core.Operations.SetKeyframe.Perform(destNode, a.TargetProperty, Document.Current.AnimationId, k1));
							}
						}
					}
				}
			}
			Document.Current.History.BeginTransaction();
			foreach (var o in operations) {
				o();
			}
			Document.Current.History.EndTransaction();
		}

		static bool CheckRowRange(int row)
		{
			return row >= 0 && row < Document.Current.Rows.Count;
		}

		static bool ArePropertiesCompatible(object object1, object object2, string property)
		{
			var t1 = object1.GetType().GetProperty(property)?.PropertyType;
			var t2 = object2.GetType().GetProperty(property)?.PropertyType;
			return t1 != null && t1 == t2;
		}
	}
}