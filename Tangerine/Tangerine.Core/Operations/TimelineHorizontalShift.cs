using System.Linq;
using Lime;

namespace Tangerine.Core.Operations
{
	public class TimelineHorizontalShift : Operation
	{
		public override bool IsChangingDocument => true;

		private int column;
		private int delta;

		private TimelineHorizontalShift(int column, int delta)
		{
			this.column = column;
			this.delta = delta;
		}

		public static void Perform(int column, int delta)
		{
			Document.Current.History.Perform(new TimelineHorizontalShift(column, delta));
		}

		public class Processor : OperationProcessor<TimelineHorizontalShift>
		{
			protected override void InternalRedo(TimelineHorizontalShift op)
			{
				if (Document.Current.Animation.IsLegacy) {
					foreach (var node in Document.Current.Container.Nodes) {
						Shift(node);
					}
				} else {
					Node namesakeAnimationOwner = null;
					foreach (var descendant in Document.Current.Animation.OwnerNode.Descendants) {
						if (descendant.Animations.TryFind(Document.Current.AnimationId, out _)) {
							namesakeAnimationOwner = descendant;
							Shift(descendant);
						}
						if (namesakeAnimationOwner != null && descendant != namesakeAnimationOwner.NextSibling) {
							continue;
						}
						namesakeAnimationOwner = null;
						Shift(descendant);
					}
				}
				void Shift(Node node)
				{
					foreach (var animator in node.Animators.Where(i => i.AnimationId == Document.Current.AnimationId)) {
						var keys = op.delta > 0 ? animator.ReadonlyKeys.Reverse() : animator.ReadonlyKeys;
						foreach (var k in keys) {
							if (k.Frame >= op.column) {
								k.Frame += op.delta;
							}
						}
						animator.ResetCache();
					}
					node.Animators.Invalidate();
				}
				var markers = op.delta > 0 ? Document.Current.Animation.Markers.Reverse() : Document.Current.Animation.Markers;
				foreach (var m in markers) {
					if (m.Frame >= op.column) {
						m.Frame += op.delta;
					}
				}
			}
			protected override void InternalUndo(TimelineHorizontalShift op)
			{
				op.delta = -op.delta;
				InternalRedo(op);
				op.delta = -op.delta;
			}
		}
	}
}
