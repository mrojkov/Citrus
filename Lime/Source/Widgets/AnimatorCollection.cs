using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;
using Lime;
using ProtoBuf;

namespace Lime
{
	public sealed class TriggerAttribute : Attribute {}

	[ProtoContract]
	public sealed class AnimatorCollection : ICollection<Animator>
	{
		static List<Animator> emptyList = new List<Animator>();
		static Animator[] emptyArray = new Animator[0];
		List<Animator> animatorList = emptyList;
		Animator[] animatorArray;
		Node owner;

		public AnimatorCollection() { /* ctor for ProtoBuf only */ }

		public AnimatorCollection(Node owner)
		{
			this.owner = owner;
		}

		public Animator[] AsArray
		{
			get
			{
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
			var result = new AnimatorCollection(owner);
			foreach (var animator in source.animatorList) {
				result.Add(animator.SharedClone());
			}
			return result;
		}

		public Animator this[string propertyName]
		{
			get {
				foreach (Animator a in animatorList) {
					if (a.TargetProperty == propertyName) {
						return a;
					}
				}
				PropertyInfo pi = owner.GetType().GetProperty(propertyName);
				if (pi == null) {
					throw new Lime.Exception("Unknown property {0} in {1}", propertyName, owner.GetType().Name);
				}
				var animator = AnimatorRegistry.Instance.CreateAnimator(pi.PropertyType);
				animator.TargetProperty = propertyName;
				Add(animator);
				return animator;
			}
		}
		
		public bool Contains(Animator item)
		{
			return animatorList.Contains(item);
		}
		
		void ICollection<Animator>.CopyTo(Animator[] a, int index)
		{
			animatorList.CopyTo(a, index);
		}
		
		bool ICollection<Animator>.IsReadOnly { 
			get { return false; }
		}
		
		public Animator this[int index] { 
			get { return animatorList[index]; }
		}
		
		public bool Remove(Animator item)
		{
			bool result = animatorList.Remove(item);
			if (animatorList.Count == 0) {
				animatorList = emptyList;
			}
			animatorArray = null;
			return result;
		}
		
		public void Clear() { animatorList = emptyList; }
		
		public int Count { get { return animatorList.Count; } }
	
		IEnumerator<Animator> IEnumerable<Animator>.GetEnumerator()
		{
			return animatorList.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return animatorList.GetEnumerator();
		}
		 
		public void Add(Animator animator)
		{
			if (animatorList == emptyList) {
				animatorList = new List<Animator>();
			}
			animatorArray = null;
			animator.Bind(owner);
			animatorList.Add(animator);
		}
		
		public int GetOverallDuration()
		{
			int val = 0;
			foreach (var animator in animatorList) {
				val = Math.Max(val, animator.Duration);
			}
			return val;
		}

		public void Apply(int time)
		{
			foreach (var animator in AsArray) {
				animator.Apply(time);
			}
		}

		public void InvokeTriggers(int frame)
		{
			foreach (var animator in AsArray) {
				if (animator.IsTriggerable) {
					animator.InvokeTrigger(frame);
				}
			}
		}
	}
}
