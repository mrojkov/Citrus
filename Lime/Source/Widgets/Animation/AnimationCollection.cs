using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Lime
{
	public sealed class AnimationCollection : ICollection<Animation>
	{
		private Node owner;

		internal Animation First { get; private set; }
		public int Count { get; private set; }

		public bool IsReadOnly => false;

		public Animation this[string id]
		{
			get { return Find(id); }
		}

		public AnimationCollection(Node owner)
		{
			this.owner = owner;
		}

		public AnimationCollection Clone(Node owner)
		{
			var result = new AnimationCollection(owner);
			for (var a = First; a != null; a = a.Next) {
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
			for (var a = First; a != null; a = a.Next) {
				if (a.Id == id) {
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

		public void Add(Animation item)
		{
			if (item.Owner != null) {
				throw new InvalidOperationException();
			}
			item.Owner = owner;
			if (First == null) {
				First = item;
			} else {
				Animation a = First;
				while (a.Next != null) {
					a = a.Next;
				}
				a.Next = item;
			}
			Count++;
		}

		public void AddRange(IEnumerable<Animation> collection)
		{
			foreach (var animation in collection) {
				Add(animation);
			}
		}

		public void Clear()
		{
			for (var a = First; a != null; ) {
				var t = a.Next;
				a.Owner = null;
				a.Next = null;
				a = t;
			}
			First = null;
			Count = 0;
		}

		public bool Contains(Animation item)
		{
			for (var a = First; a != null; a = a.Next) {
				if (a == item) {
					return true;
				}
			}
			return false;
		}

		public void CopyTo(Animation[] array, int arrayIndex)
		{
			int i = arrayIndex;
			for (var a = First; a != null; a = a.Next) {
				array[i++] = a;
			}
		}

		public bool Remove(Animation item)
		{
			if (item.Owner != owner) {
				throw new InvalidOperationException();
			}
			Animation prev = null;
			for (var a = First; a != null; a = a.Next) {
				if (a == item) {
					if (prev == null) {
						First = a.Next;
					} else {
						prev.Next = a.Next;
					}
					a.Owner = null;
					a.Next = null;
					Count--;
					return true;
				}
				prev = a;
			}
			return false;
		}

		IEnumerator<Animation> IEnumerable<Animation>.GetEnumerator() => new Enumerator(First);
		IEnumerator IEnumerable.GetEnumerator() => new Enumerator(First);

		public Enumerator GetEnumerator() => new Enumerator(First);

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
