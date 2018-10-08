using System.Collections.Generic;
using Lime;

namespace Tangerine.Core.Operations
{
	public class RemoveAnimationProcessor : OperationProcessor<RemoveFromList>
	{
		protected override void InternalDo(RemoveFromList op)
		{
			if (op.List is Node.TangerineAnimationCollection animations) {
				var animators = new List<IAnimator>();
				var animation = animations[op.Index];
				animation.FindAnimators(animators);
				foreach (var a in animators) {
					RemoveFromCollection<AnimatorCollection, IAnimator>.Perform(a.Owner.Animators, a);
				}
				if (Document.Current.SelectedAnimation == animation) {
					SetProperty.Perform(Document.Current, nameof(Document.SelectedAnimation), null);
				}
			}
		}

		protected override void InternalRedo(RemoveFromList op) { }
		protected override void InternalUndo(RemoveFromList op) { }
	}
}
