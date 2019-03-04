using System;

namespace Lime
{
	internal class AnimationCollisionMap
	{
		private int count;
		private IAnimator[] animators = new IAnimator[16];

		/// <summary>
		/// Gets an animator with the same key.Animable and key.TargetPropertyPath.
		/// </summary>
		public bool TryGetAnimator(IAnimator key, out IAnimator animator)
		{
			var hashCode = key.TargetPropertyHashCode;
			for (var i = 0; i < animators.Length; i++) {
				var a = animators[(hashCode + i) & (animators.Length - 1)];
				if (a == null) {
					break;
				}
				if (a.Animable == key.Animable && a.TargetPropertyPath == key.TargetPropertyPath) {
					animator = a;
					return true;
				}
			}
			animator = null;
			return false;
		}

		public void AddAnimator(IAnimator animator)
		{
			if (count > animators.Length / 2) {
				var newAnimators = new IAnimator[animators.Length * 2];
				foreach (var a in animators) {
					if (a != null) {
						AddAnimatorHelper(newAnimators, a);
					}
				}
				animators = newAnimators;
			}
			AddAnimatorHelper(animators, animator);
			count++;
		}

		private static void AddAnimatorHelper(IAnimator[] animators, IAnimator animator)
		{
			var hashCode = animator.TargetPropertyHashCode;
			for (var i = 0; i < animators.Length; i++) {
				var j = (hashCode + i) & (animators.Length - 1);
				if (animators[j] == null) {
					animators[j] = animator;
					return;
				}
			}
			throw new InvalidOperationException();
		}

		public void Clear()
		{
			for (int i = 0; i < animators.Length; i++) {
				animators[i] = null;
			}
			count = 0;
		}
	}
}
