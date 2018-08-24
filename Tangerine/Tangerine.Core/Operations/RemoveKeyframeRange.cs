using Lime;
using System.Collections.Generic;

namespace Tangerine.Core.Operations
{
	public class RemoveKeyframeRange : Operation
	{
		public override bool IsChangingDocument => true;

		private IAnimator animator;
		private int start;
		private int end;
		private IAnimationHost owner;

		private RemoveKeyframeRange(IAnimator animator, int start, int end)
		{
			this.animator = animator;
			this.start = start;
			this.end = end;
			this.owner = animator.Owner;
		}

		public static void Perform(IAnimator animator, int start, int end)
		{
			Document.Current.History.Perform(new RemoveKeyframeRange(animator, start, end));
		}

		public class Processor : OperationProcessor<RemoveKeyframeRange>
		{
			private class Backup
			{
				public List<IKeyframe> Keyframes;
			}

			protected override void InternalRedo(RemoveKeyframeRange op)
			{
				var keys = op.animator.Keys;
				int count = 0;
				var backup = new Backup { Keyframes = new List<IKeyframe>() };
				for (int i = 0; i < keys.Count; ++i) {
					if (keys[i].Frame >= op.start && keys[i].Frame <= op.end) {
						backup.Keyframes.Add(keys[i]);
						count++;
					} else if (keys[i].Frame > op.end) {
						if (count == 0) {
							op.Save(backup);
							return;
						}
						keys[i - count] = keys[i];
					}
				}
				op.Save(backup);
				for (var i = 0; i < count; ++i) {
					keys.RemoveAt(keys.Count - 1);
				}
				if (op.animator.Keys.Count == 0) {
					op.owner.Animators.Remove(op.animator);
				} else {
					op.animator.ResetCache();
				}
			}
			protected override void InternalUndo(RemoveKeyframeRange op)
			{
				if (op.animator.Owner == null) {
					op.owner.Animators.Add(op.animator);
				}
				var backup = op.Restore<Backup>();
				foreach (var key in backup.Keyframes) {
					op.animator.Keys.AddOrdered(key);
				}
				op.animator.ResetCache();
			}
		}
	}
}
