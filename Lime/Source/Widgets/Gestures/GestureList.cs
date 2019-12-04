using System;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public class GestureList : IList<Gesture>
	{
		private readonly List<Gesture> gestures;
		private readonly Node owner;

		public int Count => gestures.Count;
		public bool IsReadOnly => false;

		public GestureList(Node gesturesOwner)
		{
			gestures = new List<Gesture>();
			owner = gesturesOwner;
		}

		public void Add(Gesture item)
		{
			ResetOwner(item, owner);
			gestures.Add(item);
		}

		public bool Remove(Gesture item)
		{
			if (gestures.Remove(item)) {
				ResetOwner(item);
				return true;
			}
			return false;
		}

		public void Insert(int index, Gesture item)
		{
			ResetOwner(item, owner);
			gestures.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			ResetOwner(gestures[index]);
			gestures.RemoveAt(index);
		}

		public void Clear()
		{
			gestures.ForEach(ResetOwner);
			gestures.Clear();
		}

		public Gesture this[int index]
		{
			get => gestures[index];
			set
			{
				ResetOwner(value, owner);
				ResetOwner(gestures[index]);
				gestures[index] = value;
			}
		}

		public int IndexOf(Gesture item) => gestures.IndexOf(item);
		public bool Contains(Gesture item) => gestures.Contains(item);
		public void CopyTo(Gesture[] array, int arrayIndex) => gestures.CopyTo(array, arrayIndex);
		public List<Gesture>.Enumerator GetEnumerator() => gestures.GetEnumerator();
		IEnumerator<Gesture> IEnumerable<Gesture>.GetEnumerator() => gestures.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => gestures.GetEnumerator();

		private static void ResetOwner(Gesture gesture)
		{
			gesture.Owner = null;
		}

		private static void ResetOwner(Gesture gesture, Node newOwner)
		{
			if (newOwner != null && gesture.Owner != null) {
				throw new InvalidOperationException("Gesture already owned");
			}
			gesture.Owner = newOwner;
		}
	}
}
