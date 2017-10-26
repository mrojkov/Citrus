using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public abstract class Gesture
	{
		public Node Owner { get; internal set; }
		protected Input Input => CommonWindow.Current.Input;

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

	public class GestureCollection : ICollection<Gesture>
	{
		private List<Gesture> gestures = new List<Gesture>();

		public int Count => gestures.Count;
		public bool IsReadOnly => false;
		private Node owner;

		public GestureCollection(Node owner)
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
		public IEnumerator<Gesture> GetEnumerator() => gestures.GetEnumerator();

		public bool Remove(Gesture item)
		{
			if (gestures.Remove(item)) {
				item.Owner = null;
				return true;
			}
			return false;
		}

		IEnumerator IEnumerable.GetEnumerator() => gestures.GetEnumerator();
	}

}
