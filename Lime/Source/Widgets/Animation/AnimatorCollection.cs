using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Lime
{
	public sealed class TriggerAttribute : Attribute {}

	public sealed class AnimatorCollection : ICollection<IAnimator>, IDisposable
	{
		private IAnimationHost owner;
		private List<IAnimator> list;
		public int Count => list?.Count ?? 0;
#if TANGERINE
		public int Version { get; private set; }
#endif // TANGERINE

		public AnimatorCollection(IAnimationHost owner)
		{
			this.owner = owner;
		}

		public void Dispose()
		{
			foreach (var a in this) {
				a.Dispose();
			}
			Clear();
		}

		public void Add(IAnimator item)
		{
			if (item.Owner != null) {
				throw new InvalidOperationException();
			}
			item.Owner = owner;
			CreateListIfNeeded();
			list.Add(item);
			owner.OnAnimatorsChanged();
#if TANGERINE
			Version++;
#endif // TANGERINE
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CreateListIfNeeded()
		{
			if (list == null) {
				list = new List<IAnimator>();
			}
		}

		public void AddRange(IEnumerable<IAnimator> collection)
		{
			foreach (var a in collection) {
				Add(a);
			}
		}

		public void Clear()
		{
			if (list != null) {
				foreach (var a in list) {
					a.Owner = null;
				}
				list = null;
			}
#if TANGERINE
			Version++;
#endif // TANGERINE
			owner.OnAnimatorsChanged();
		}

		internal static AnimatorCollection SharedClone(IAnimationHost owner, AnimatorCollection source)
		{
			var result = new AnimatorCollection(owner);
			foreach (var animator in source) {
				result.Add(animator.Clone());
			}
			return result;
		}

		public bool TryFind(string propertyPath, out IAnimator animator, string animationId = null)
		{
			animator = null;
			foreach (var a in this) {
				if (a.TargetPropertyPath == propertyPath && a.AnimationId == animationId) {
					animator = a;
					return true;
				}
			}
			return false;
		}

		public bool TryFind<T>(string propertyPath, out Animator<T> animator, string animationId = null)
		{
			TryFind(propertyPath, out var a, animationId);
			animator = a as Animator<T>;
			return animator != null;
		}

		public IAnimator this[string propertyPath, string animationId = null]
		{
			get
			{
				IAnimator animator;
				if (TryFind(propertyPath, out animator, animationId)) {
					return animator;
				}
				var (p, _, _) = AnimationUtils.GetPropertyByPath(owner, propertyPath);
				var pi = p.Info;
				if (pi == null) {
					throw new Lime.Exception("Unknown property {0} in {1}", propertyPath, owner.GetType().Name);
				}
				animator = AnimatorRegistry.Instance.CreateAnimator(pi.PropertyType);
				animator.TargetPropertyPath = propertyPath;
				animator.AnimationId = animationId;
				Add(animator);
#if TANGERINE
				Version++;
#endif // TANGERINE
				owner.OnAnimatorsChanged();
				return animator;
			}
		}

		public bool Contains(IAnimator item)
		{
			foreach (var a in this) {
				if (a == item) {
					return true;
				}
			}
			return false;
		}

		void ICollection<IAnimator>.CopyTo(IAnimator[] array, int index) => list.CopyTo(array, index);

		bool ICollection<IAnimator>.IsReadOnly => false;

		public bool Remove(IAnimator item)
		{
			if (item.Owner != owner) {
				return false;
			}
			if (list?.Remove(item) == true) {
				item.Owner = null;
#if TANGERINE
				Version++;
#endif // TANGERINE
				owner.OnAnimatorsChanged();
				return true;
			}
			return false;
		}

		public bool Remove(string propertyName, string animationId = null)
		{
			return TryFind(propertyName, out var animator, animationId) ? Remove(animator) : false;
		}

		public int GetOverallDuration(string animationId = null)
		{
			int val = 0;
			foreach (var a in this) {
				if (a.AnimationId == animationId) {
					val = Math.Max(val, a.Duration);
				}
			}
			return val;
		}

		public void CalcAndApply(double time, string animationId = null)
		{
			foreach (var a in this) {
				if (a.AnimationId == animationId) {
					a.CalcAndApply(time);
				}
			}
		}

		public void InvokeTriggers(int frame, string animationId = null, double animationTimeCorrection = 0)
		{
			foreach (var a in this) {
				if (a.IsTriggerable && a.AnimationId == animationId) {
					a.InvokeTrigger(frame, animationTimeCorrection);
				}
			}
		}

		public void Invalidate()
		{
#if TANGERINE
			Version++;
#endif // TANGERINE
		}

		IEnumerator<IAnimator> IEnumerable<IAnimator>.GetEnumerator()
		{
			CreateListIfNeeded();
			return list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			CreateListIfNeeded();
			return list.GetEnumerator();
		}

		public List<IAnimator>.Enumerator GetEnumerator()
		{
			CreateListIfNeeded();
			return list.GetEnumerator();
		}
	}
}
