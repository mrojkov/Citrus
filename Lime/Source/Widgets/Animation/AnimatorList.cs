using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Lime
{
	public sealed class TriggerAttribute : Attribute {}

	public sealed class AnimatorList : IList<IAnimator>, IDisposable
	{
		private static readonly List<IAnimator> emptyList = new List<IAnimator>();
		private static readonly IAnimator[] emptyArray = new IAnimator[0];
		private List<IAnimator> animatorList = emptyList;
		private IAnimator[] animatorArray;
		private Node owner;

		public int Version { get; private set; }
		public int Count => AsArray.Length;
		bool ICollection<IAnimator>.IsReadOnly => false;

		public AnimatorList(Node owner)
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

		public AnimatorList(Node owner, int capacity)
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

		internal static AnimatorList SharedClone(Node owner, AnimatorList source)
		{
			var result = new AnimatorList(owner, source.Count);
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

		public bool Contains(IAnimator item) => animatorList.Contains(item);
		void ICollection<IAnimator>.CopyTo(IAnimator[] a, int index) => animatorList.CopyTo(a, index);
		public int IndexOf(IAnimator item) => animatorList.IndexOf(item);

		public void Insert(int index, IAnimator item)
		{
			throw new NotSupportedException();
		}

		public IAnimator this[int index] {
			get { return animatorList[index]; }
			set { throw new NotSupportedException(); }
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

		public void RemoveAt(int index)
		{
			animatorList.RemoveAt(index);
			if (animatorList.Count == 0) {
				animatorList = emptyList;
			}
			animatorArray = null;
			Version++;
		}

		public void Clear()
		{
			animatorList = emptyList;
			animatorArray = null;
			Version++;
		}

		IEnumerator<IAnimator> IEnumerable<IAnimator>.GetEnumerator() => animatorList.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => animatorList.GetEnumerator();

		public void Add(IAnimator animator)
		{
			if (animatorList == emptyList) {
				animatorList = new List<IAnimator>();
			}
			animatorArray = null;
			animator.Bind(owner);
			animatorList.Add(animator);
			Animation animation;
			if (owner.Animations.TryFind(animator.AnimationId, out animation)) {
				animation.NextMarkerOrTriggerTime = null;
			}
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
