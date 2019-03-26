using System;

namespace Lime
{
	internal class AnimationCollisionMap
	{
		private int count;
		public IAnimator[] Animators { get; private set; } = new IAnimator[16];

		/// <summary>
		/// Gets an animator with the same key.Animable and key.TargetPropertyPath.
		/// </summary>
		public bool TryGetAnimator(IAnimator key, out IAnimator animator)
		{
			var hashCode = key.Animable.GetHashCode() ^ key.TargetPropertyPathUID;
			for (var i = 0; i < Animators.Length; i++) {
				var a = Animators[(hashCode + i) & (Animators.Length - 1)];
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

		public void AddAnimator(IAnimator animator, bool replace)
		{
			EnsureEnoughRoom();
			var hashCode = animator.Animable.GetHashCode() ^ animator.TargetPropertyPathUID;
			for (var i = 0; i < Animators.Length; i++) {
				var j = (hashCode + i) & (Animators.Length - 1);
				var a = Animators[j];
				if (a == null) {
					Animators[j] = animator;
					count++;
					break;
				} else if (a.Animable == animator.Animable && a.TargetPropertyPath == animator.TargetPropertyPath) {
					if (replace) {
						Animators[j] = animator;
					}
					break;
				}
			}
		}

		private void EnsureEnoughRoom()
		{
			if (count > Animators.Length / 2) {
				var newAnimators = new IAnimator[Animators.Length * 2];
				foreach (var a in Animators) {
					if (a == null) {
						continue;
					}
					var hashCode = a.Animable.GetHashCode() ^ a.TargetPropertyPathUID;
					for (var i = 0; i < newAnimators.Length; i++) {
						var j = (hashCode + i) & (newAnimators.Length - 1);
						if (newAnimators[j] == null) {
							newAnimators[j] = a;
							break;
						}
					}
				}
				Animators = newAnimators;
			}
		}

		public void Clear()
		{
			for (int i = 0; i < Animators.Length; i++) {
				Animators[i] = null;
			}
			count = 0;
		}
	}
}
