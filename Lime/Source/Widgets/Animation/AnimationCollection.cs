using System;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public sealed class AnimationCollection : IList<Animation>, IList
	{
		private Node owner;

		public int Count { get; private set; }

		bool ICollection<Animation>.IsReadOnly => false;

		bool IList.IsReadOnly => false;

		bool IList.IsFixedSize => false;

		object ICollection.SyncRoot => this;

		bool ICollection.IsSynchronized => false;

		object IList.this[int index]
		{
			get => this[index];
			set => this[index] = (Animation)value;
		}

		public Animation this[int index]
		{
			get
			{
				if (unchecked((uint)index) >= Count) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				var animation = owner.FirstAnimation;
				while (index > 0) {
					animation = animation.Next;
					index--;
				}
				return animation;
			}
			set
			{
				if (unchecked((uint)index) > Count) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				if (value == null) {
					throw new ArgumentNullException(nameof(value));
				}
				if (value.Owner != null) {
					throw new InvalidOperationException();
				}
				RemoveAt(index);
				Insert(index, value);
			}
		}

		public AnimationCollection(Node owner)
		{
			this.owner = owner;
		}

		public AnimationCollection Clone(Node owner)
		{
			var result = new AnimationCollection(owner);
			for (var a = this.owner.FirstAnimation; a != null; a = a.Next) {
				result.Add(a.Clone());
			}
			return result;
		}

		public Animation Find(string id)
		{
			Animation animation;
			if (!TryFind(id, out animation)) {
				throw new Lime.Exception("Unknown animation '{0}'", id);
			}
			return animation;
		}

		public bool TryFind(string id, out Animation animation)
		{
			for (var a = owner.FirstAnimation; a != null; a = a.Next) {
				if (a.Id == id) {
					animation = a;
					return true;
				}
			}
			animation = null;
			return false;
		}

		public bool TryFind(int idComparisonCode, out Animation animation)
		{
			for (var a = owner.FirstAnimation; a != null; a = a.Next) {
				if (a.IdComparisonCode == idComparisonCode) {
					animation = a;
					return true;
				}
			}
			animation = null;
			return false;
		}

		public bool TryRun(string animationId, string markerId = null, double animationTimeCorrection = 0)
		{
			Animation animation;
			return TryFind(animationId, out animation) && animation.TryRun(markerId, animationTimeCorrection);
		}

		public void Run(string animationId, string markerId = null)
		{
			if (!TryRun(animationId, markerId)) {
				if (animationId != null && markerId != null) {
					throw new Lime.Exception(string.Format("Unknown animation ({0}) or marker ({1})", animationId, markerId));
				} else if (animationId != null && markerId == null) {
					throw new Lime.Exception(string.Format("Unknown animation ({0})", animationId));
				} else if (animationId == null && markerId != null) {
					throw new Lime.Exception(string.Format("Unknown marker ({0})", markerId));
				} else {
					throw new Lime.Exception("Unknown animation or marker");
				}
			}
		}

		public void Add(Animation item) => Insert(Count, item);

		public void AddRange(IEnumerable<Animation> collection)
		{
			foreach (var animation in collection) {
				Add(animation);
			}
		}

		public void Clear()
		{
			for (var a = owner.FirstAnimation; a != null; ) {
				var t = a.Next;
				a.Owner = null;
				a.Next = null;
				a = t;
			}
			owner.FirstAnimation = null;
			Count = 0;
			owner.RefreshRunningAnimationCount();
		}

		public bool Contains(Animation item) => item != null && item.Owner == owner;

		public void CopyTo(Animation[] array, int arrayIndex)
		{
			int i = arrayIndex;
			for (var a = owner.FirstAnimation; a != null; a = a.Next) {
				array[i++] = a;
			}
		}

		public bool Remove(Animation item)
		{
			var index = IndexOf(item);
			if (index >= 0) {
				RemoveAt(index);
				return true;
			}
			return false;
		}

		IEnumerator<Animation> IEnumerable<Animation>.GetEnumerator() => new Enumerator(owner.FirstAnimation);
		IEnumerator IEnumerable.GetEnumerator() => new Enumerator(owner.FirstAnimation);

		public Enumerator GetEnumerator() => new Enumerator(owner.FirstAnimation);

		public int IndexOf(Animation item)
		{
			if (item != null && item.Owner == owner) {
				var index = 0;
				var animation = owner.FirstAnimation;
				while (animation != item) {
					animation = animation.Next;
					index++;
				}
				return index;
			}
			return -1;
		}

		public void Insert(int index, Animation item)
		{
			if (unchecked((uint)index) > Count) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			if (item == null) {
				throw new ArgumentNullException(nameof(item));
			}
			if (item.Owner != null) {
				throw new InvalidOperationException();
			}
			item.Owner = owner;
			if (index == 0) {
				item.Next = owner.FirstAnimation;
				owner.FirstAnimation = item;
			} else {
				var a = owner.FirstAnimation;
				while (index > 1) {
					a = a.Next;
					index--;
				}
				item.Next = a.Next;
				a.Next = item;
			}
			Count++;
			owner.RefreshRunningAnimationCount();
		}

		public void RemoveAt(int index)
		{
			if (unchecked((uint)index) >= Count) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			Animation prev = null;
			Animation curr = null;
			var oldIdx = index;
			for (curr = owner.FirstAnimation; index > 0; curr = curr.Next, index--) {
				prev = curr;
			}
			if (prev == null) {
				owner.FirstAnimation = curr.Next;
			} else {
				prev.Next = curr.Next;
			}
			curr.Owner = null;
			curr.Next = null;
			Count--;
			owner.RefreshRunningAnimationCount();
		}

		int IList.Add(object value)
		{
			Add((Animation)value);
			return Count - 1;
		}

		bool IList.Contains(object value) => value is Animation animation && Contains(animation);

		int IList.IndexOf(object value) => value is Animation animation ? IndexOf(animation) : -1;

		void IList.Insert(int index, object value) => Insert(index, (Animation)value);

		void IList.Remove(object value)
		{
			if (value is Animation animation) {
				Remove(animation);
			}
		}

		void ICollection.CopyTo(Array array, int index) => CopyTo((Animation[])array, index);

		public struct Enumerator : IEnumerator<Animation>
		{
			private Animation first;
			private Animation current;

			public Enumerator(Animation first)
			{
				this.first = first;
				current = null;
			}

			object IEnumerator.Current => current;

			public bool MoveNext()
			{
				if (current == null) {
					current = first;
				} else {
					current = current.Next;
				}
				return current != null;
			}

			public void Reset() => current = null;

			public Animation Current => current;

			public void Dispose() { }
		}
	}
}
