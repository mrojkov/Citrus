using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public abstract class Gesture
	{
		public Node Owner { get; internal set; }
		protected WindowInput Input => CommonWindow.Current.Input;

		internal protected abstract void Cancel();
		internal protected abstract void Update(IEnumerable<Gesture> gestures);

		internal protected virtual bool ShouldDeferClicks(int buttonIndex) => false;

		protected struct PollableEvent
		{
			private int occurredOnIteration;

			public event Action Handler;

			public bool HasOccurred() => occurredOnIteration == WidgetContext.Current.GestureManager.CurrentIteration;

			public void Raise()
			{
				CommonWindow.Current.Invalidate();
				occurredOnIteration = WidgetContext.Current.GestureManager.CurrentIteration;
				Handler?.Invoke();
			}
		}
	}

	public class GestureList : IList<Gesture>
	{
		private readonly List<Gesture> gestures = new List<Gesture>();

		public int Count => gestures.Count;
		public bool IsReadOnly => false;
		private Node owner;

		public GestureList(Node owner)
		{
			this.owner = owner;
		}

		public void Add(Gesture item)
		{
			if (item.Owner != null) {
				throw new InvalidOperationException();
			}
			item.Owner = owner;
			gestures.Add(item);
		}

		public void Clear() => gestures.Clear();
		public bool Contains (Gesture item) => gestures.Contains(item);
		public void CopyTo(Gesture [] array, int arrayIndex) => gestures.CopyTo(array, arrayIndex);
		public List<Gesture>.Enumerator GetEnumerator() => gestures.GetEnumerator();

		IEnumerator<Gesture> IEnumerable<Gesture>.GetEnumerator() => gestures.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => gestures.GetEnumerator();

		public bool Remove(Gesture item)
		{
			if (gestures.Remove(item)) {
				item.Owner = null;
				return true;
			}
			return false;
		}

		public int IndexOf(Gesture item) { return gestures.IndexOf(item); }

		public void Insert(int index, Gesture item)
		{
			if (item.Owner != null) {
				throw new InvalidOperationException();
			}
			item.Owner = owner;
			gestures.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			gestures[index].Owner = null;
			gestures.RemoveAt(index);
		}

		public Gesture this[int index]
		{
			get { return gestures[index]; }
			set
			{
				gestures[index].Owner = null;
				gestures[index] = value;
				value.Owner = owner;
			}
		}
	}

}
