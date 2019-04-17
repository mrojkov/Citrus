using System;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public interface IAnimable
	{
		IAnimable Owner { get; set; }
	}

	public class Animable : IAnimable
	{
		private IAnimable owner;
		public IAnimable Owner
		{
			get => owner;
			set
			{
				var previousOwner = owner;
				owner?.UnbindAnimators();
				owner = value;
				owner?.UnbindAnimators();
				OnOwnerChanged(previousOwner);
			}
		}

		protected virtual void OnOwnerChanged(IAnimable previousOwner) { }

		public Animable Clone()
		{
			var clone = (Animable)MemberwiseClone();
			clone.owner = null;
			return clone;
		}
	}

	public interface IAnimationHost
	{
		AnimatorCollection Animators { get; }
		void OnAnimatorCollectionChanged();
		void OnTrigger(string property, object value, double animationTimeCorrection = 0);
		Component GetComponent(Type type);
	}

	public static class IAnimableExtensions
	{
		public static IAnimationHost GetAnimationHost(this IAnimable animable)
		{
			while (animable != null) {
				if (animable is IAnimationHost host) {
					return host;
				}
				animable = animable.Owner;
			}
			return null;
		}

		public static void UnbindAnimators(this IAnimable animable)
		{
			var host = animable.GetAnimationHost();
			if (host == null) {
				return;
			}
			foreach (var a in host.Animators) {
				// Optimization: absence of `.` in path means its a node property being animated, so we never need to unbind it
				if (a.TargetPropertyPath.IndexOf('.') != -1) {
					a.Unbind();
				}
			}
		}
	}

	public class AnimableList<T> : Animable, IList<T>, IList
	{
		private List<T> list = new List<T>();
		public T this[int index]
		{
			get => list[index];
			set
			{
				if (list[index] is IAnimable a0) {
					a0.Owner = null;
				}
				list[index] = value;
				if (list[index] is IAnimable a1) {
					a1.Owner = this;
				}
			}
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		object ICollection.SyncRoot => ((IList)list).SyncRoot;
		bool ICollection.IsSynchronized => ((IList)list).IsSynchronized;
		void ICollection.CopyTo(Array array, int index) => CopyTo((T[])array, index);

		object IList.this[int index] { get => this[index]; set => this[index] = (T)value; }
		bool IList.IsFixedSize => false;
		int IList.Add(object value)
		{
			Add((T)value);
			return Count - 1;
		}
		bool IList.Contains(object value) => Contains((T)value);
		int IList.IndexOf(object value) => IndexOf((T)value);
		void IList.Insert(int index, object value) => Insert(index, (T)value);
		void IList.Remove(object value) => Remove((T)value);

		public int Count => list.Count;
		public bool IsReadOnly => false;
		public void Add(T item) => Insert(Count, item);
		public void Clear()
		{
			foreach (var item in this) {
				InvalidateAnimableOrUnbindAnimators(item);
			}
			list.Clear();
		}
		public bool Contains(T item) => list.Contains(item);
		public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);
		public int IndexOf(T item) => list.IndexOf(item);
		public void Insert(int index, T item)
		{
			InvalidateAnimableOrUnbindAnimators(item, this);
			list.Insert(index, item);
		}
		public bool Remove(T item) => list.Remove(item);
		public void RemoveAt(int index)
		{
			InvalidateAnimableOrUnbindAnimators(list[index], null);
			list.RemoveAt(index);
		}

		private void InvalidateAnimableOrUnbindAnimators(T item, IAnimable newOwner = null)
		{
			if (item is IAnimable animable) {
				animable.Owner = newOwner;
			} else {
				Owner.UnbindAnimators();
			}
		}

		public List<T>.Enumerator GetEnumerator() => list.GetEnumerator();

		public AnimableList()
		{
			var t = typeof(T);
			if (!t.IsValueType && t != typeof(string) && !typeof(IAnimable).IsAssignableFrom(t)) {
				throw new InvalidOperationException("T must be either value type, string or class implementing IAnimable");
			}
		}
	}
}
