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
		List<Animator> animators = emptyList;
		
		Node owner;

		public AnimatorCollection(Node owner)
		{
			this.owner = owner;
		}

		internal static AnimatorCollection SharedClone(Node owner, AnimatorCollection source)
		{
			var result = new AnimatorCollection(owner);
			foreach (var animator in source.animators) {
				result.Add(animator.SharedClone());
			}
			return result;
		}

		public Animator this[string propertyName]
		{
			get {
				foreach (Animator a in animators)
					if (a.TargetProperty == propertyName)
						return a;
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
			return animators.Contains(item);
		}
		
		void ICollection<Animator>.CopyTo(Animator[] a, int index)
		{
			animators.CopyTo(a, index);
		}
		
		bool ICollection<Animator>.IsReadOnly { 
			get { return false; }
		}
		
		public Animator this[int index] { 
			get { return animators[index]; }
		}
		
		public bool Remove(Animator item)
		{
			bool result = animators.Remove(item);
			if (animators.Count == 0) {
				animators = emptyList;
			}
			return result;
		}
		
		public void Clear() { animators = emptyList; }
		
		public int Count { get { return animators.Count; } }
	
		IEnumerator<Animator> IEnumerable<Animator>.GetEnumerator()
		{
			return animators.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return animators.GetEnumerator();
		}
		 
		public void Add(Animator animator)
		{
			if (animators == emptyList) {
				animators = new List<Animator>();
			}
			animator.Bind(owner);
			animators.Add(animator);
		}
		
		public int GetOverallDuration()
		{
			int val = 0;
			int count = animators.Count;
			for (int i = 0; i < count; i++) {
				val = Math.Max(val, animators[i].Duration);
			}
			return val;
		}

		public void Apply(int time)
		{
			int count = animators.Count;
			for (int i = 0; i < count; i++) {
				animators[i].Apply(time);
			}
		}

		public void InvokeTriggers(int frame)
		{
			int count = animators.Count;
			for (int i = 0; i < count; i++) {
				var a = animators[i];
				if (a.IsTriggerable) {
					a.InvokeTrigger(frame);
				}
			}
		}
	}
}
