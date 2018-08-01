using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.Core.Operations;
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
					Document.Current.History.DoTransaction(() => {
						DragKeys(r.Offset, r.RemoveOriginals);
						ShiftSelection(r.Offset);
						g.Remove<DragKeyframesRequest>();
					});
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
					foreach (var a in node.Animators.ToList()) {
						if (property != null && a.TargetProperty != property) {
							continue;
						}
						IEnumerable<IKeyframe> keysEnumerable = a.Keys.Where(k => k.Frame >= span.A && k.Frame < span.B);
						if (offset.X > 0) {
							keysEnumerable = keysEnumerable.Reverse();
						}
						foreach (var k in keysEnumerable) {
							if (processedKeys.Contains(k)) {
								continue;
							}
							processedKeys.Add(k);
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
								// The same logic is used to create keyframes as everywhere, but extended by setting
								// all parameters from a particular keyframe. Yes, this creates some overhead.
								operations.Add(() => SetAnimableProperty.Perform(destNode, a.TargetProperty, k1.Value, true, false, k1.Frame));
								operations.Add(() => Core.Operations.SetKeyframe.Perform(destNode, a.TargetProperty, Document.Current.AnimationId, k1));
							}
							// Order is importent. RemoveKeyframe must be after SetKeyframe,
							// to prevent animator clean up if all keys were removed.
							if (removeOriginals) {
								operations.Add(() => Core.Operations.RemoveKeyframe.Perform(a, k.Frame));
							}
						}
					}
				}
			}
			foreach (var o in operations) {
				o();
			}
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
