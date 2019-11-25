using System;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public sealed class AnimationCollection : IList<Animation>, IList
	{
		private AnimationComponent owner;
		private List<Animation> items = new List<Animation>();

		public int Count => items.Count;

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
			get => items[index];
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

		public AnimationCollection(AnimationComponent owner)
		{
			this.owner = owner;
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
			foreach (var a in items) {
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
			foreach (var a in items) {
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
			while (Count > 0) {
				RemoveAt(Count - 1);
			}
		}

		public bool Contains(Animation item) => item != null && item.Owner == owner;

		public void CopyTo(Animation[] array, int arrayIndex)
		{
			items.CopyTo(array, arrayIndex);
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

		IEnumerator<Animation> IEnumerable<Animation>.GetEnumerator() => GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public List<Animation>.Enumerator GetEnumerator() => items.GetEnumerator();

		public int IndexOf(Animation item)
		{
			if (item != null && item.Owner == owner) {
				return items.IndexOf(item);
			}
			return -1;
		}

		public void Insert(int index, Animation item)
		{
			if (item == null) {
				throw new ArgumentNullException(nameof(item));
			}
			if (item.Owner != null) {
				throw new ArgumentException(nameof(item));
			}
			items.Insert(index, item);
			item.Owner = owner;
			owner.OnAnimationAdded(item);
		}

		public void RemoveAt(int index)
		{
			var item = items[index];
			items.RemoveAt(index);
			item.Owner = null;
			owner.OnAnimationRemoved(item);
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
	}
}
