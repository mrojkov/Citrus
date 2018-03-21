using System;
using System.Linq;
using Lime;

namespace Tangerine.Core.Operations
{
	public class TimelineHorizontalShift : Operation
	{
		public readonly int Column;
		public readonly int Direction;
		public override bool IsChangingDocument => true;

		public static void Perform(int column, int direction)
		{
			Document.Current.History.Perform(new TimelineHorizontalShift(column, direction));
		}

		private TimelineHorizontalShift(int column, int direction)
		{
			Column = column;
			Direction = direction;
		}

		public class Processor : OperationProcessor<TimelineHorizontalShift>
		{
			protected override void InternalDo(TimelineHorizontalShift op)
			{
				var container = Document.Current.Container;
				foreach (var node in container.Nodes) {
					foreach (var animator in node.Animators.Where(i => i.AnimationId == Document.Current.AnimationId)) {
						var keys = op.Direction > 0 ? animator.ReadonlyKeys.Reverse() : animator.ReadonlyKeys;
						foreach (var k in keys.ToList()) {
							if (k.Frame >= op.Column) {
								var k1 = k.Clone();
								k1.Frame += op.Direction.Sign();
								if (op.Direction > 0 || k.Frame > op.Column) {
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
				if (op.Direction > 0) {
					markers.Reverse();
				}
				foreach (var m in markers) {
					if (m.Frame >= op.Column) {
						DeleteMarker.Perform(container.Markers, m);
						var m1 = m.Clone();
						m1.Frame += op.Direction.Sign();
						if (op.Direction > 0 || m.Frame > op.Column) {
							SetMarker.Perform(container.Markers, m1);
						}
					}
				}
			}

			protected override void InternalRedo(TimelineHorizontalShift op) { }
			protected override void InternalUndo(TimelineHorizontalShift op) { }
		}
	}
}
