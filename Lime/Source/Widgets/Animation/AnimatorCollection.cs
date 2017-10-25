using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Lime
{
	public sealed class TriggerAttribute : Attribute {}

	public sealed class AnimatorCollection : ICollection<IAnimator>, IDisposable
	{
		static List<IAnimator> emptyList = new List<IAnimator>();
		static IAnimator[] emptyArray = new IAnimator[0];
		List<IAnimator> animatorList = emptyList;
		IAnimator[] animatorArray;
		Node owner;

		public int Version { get; private set; }

		public AnimatorCollection(Node owner)
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

		public AnimatorCollection(Node owner, int capacity)
		{
			this.owner = owner;
			if (capacity > 0) {
				animatorList = new List<IAnimator>(capacity);
			}
		}

		public IAnimator[] AsArray
		{
			get {
				if (animatorArray == null) {
					if (animatorList.Count > 0) {
						animatorArray = animatorList.ToArray();
					} else {
						animatorArray = emptyArray;
					}
				}
				return animatorArray;
			}
		}

		internal static AnimatorCollection SharedClone(Node owner, AnimatorCollection source)
		{
			var result = new AnimatorCollection(owner, source.Count);
			foreach (var animator in source.animatorList) {
				result.Add(animator.Clone());
			}
			return result;
		}

		public bool TryFind(string propertyName, out IAnimator animator, string animationId = null)
		{
			foreach (IAnimator a in AsArray) {
				if (a.TargetProperty == propertyName && a.AnimationId == animationId) {
					animator = a;
					return true;
				}
			}
			animator = null;
			return false;
		}

		public bool TryFind<T>(string propertyName, out Animator<T> animator, string animationId = null)
		{
			IAnimator a;
			TryFind(propertyName, out a, animationId);
			animator = a as Animator<T>;
			return animator != null;
		}

		public IAnimator this[string propertyName, string animationId = null]
		{
			get {
				IAnimator animator;
				if (TryFind(propertyName, out animator, animationId)) {
					return animator;
				}
				PropertyInfo pi = owner.GetType().GetProperty(propertyName);
				if (pi == null) {
					throw new Lime.Exception("Unknown property {0} in {1}", propertyName, owner.GetType().Name);
				}
				animator = AnimatorRegistry.Instance.CreateAnimator(pi.PropertyType);
				animator.TargetProperty = propertyName;
				animator.AnimationId = animationId;
				Add(animator);
				Version++;
				return animator;
			}
		}

		public bool Contains(IAnimator item)
		{
			return animatorList.Contains(item);
		}

		void ICollection<IAnimator>.CopyTo(IAnimator[] a, int index)
		{
			animatorList.CopyTo(a, index);
		}

		bool ICollection<IAnimator>.IsReadOnly {
			get { return false; }
		}

		public IAnimator this[int index] {
			get { return animatorList[index]; }
		}

		public bool Remove(IAnimator item)
		{
			bool result = animatorList.Remove(item);
			if (animatorList.Count == 0) {
				animatorList = emptyList;
			}
			animatorArray = null;
			Version++;
			return result;
		}

		public void Clear() { animatorList = emptyList; animatorArray = null; Version++; }

		public int Count { get { return AsArray.Length; } }

		IEnumerator<IAnimator> IEnumerable<IAnimator>.GetEnumerator()
		{
			return animatorList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return animatorList.GetEnumerator();
		}

		public void Add(IAnimator animator)
		{
			if (animatorList == emptyList) {
				animatorList = new List<IAnimator>();
			}
			animatorArray = null;
			animator.Bind(owner);
			animatorList.Add(animator);
			Version++;
		}

		public int GetOverallDuration()
		{
			int val = 0;
			foreach (var animator in animatorList) {
				val = Math.Max(val, animator.Duration);
			}
			return val;
		}

		public void Apply(double time, string animationId = null)
		{
			foreach (var animator in AsArray) {
				if (animator.AnimationId == animationId) {
					animator.Apply(time);
				}
			}
		}

		public void InvokeTriggers(int frame, double animationTimeCorrection = 0)
		{
			foreach (var animator in AsArray) {
				if (animator.IsTriggerable) {
					animator.InvokeTrigger(frame, animationTimeCorrection);
				}
			}
		}
	}
}
