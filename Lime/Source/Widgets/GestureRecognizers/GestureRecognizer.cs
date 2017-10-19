using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public abstract class GestureRecognizer
	{
		public Node Owner { get; internal set; }
		protected Input Input => CommonWindow.Current.Input;

		internal protected abstract void Cancel();
		internal protected abstract void Update(IEnumerable<GestureRecognizer> recognizers);

		internal protected virtual bool ShouldDeferClicks(int buttonIndex) => false;

		protected struct PollableEvent
		{
			private int occurredOnIteration;

			public event Action Handler;

			public bool HasOccurred() => occurredOnIteration == WidgetContext.Current.GestureRecognizerManager.CurrentIteration;

			public void Raise()
			{
				CommonWindow.Current.Invalidate();
				occurredOnIteration = WidgetContext.Current.GestureRecognizerManager.CurrentIteration;
				Handler?.Invoke();
			}
		}
	}

	public class GestureRecognizerCollection : ICollection<GestureRecognizer>
	{
		private List<GestureRecognizer> gestures = new List<GestureRecognizer>();

		public int Count => gestures.Count;
		public bool IsReadOnly => false;
		private Node owner;

		public GestureRecognizerCollection(Node owner)
		{
			this.owner = owner;
		}

		public void Add(GestureRecognizer item)
		{
			if (item.Owner != null) {
				throw new InvalidOperationException();
			}
			item.Owner = owner;
			gestures.Add(item);
		}

		public void Clear() => gestures.Clear();
		public bool Contains (GestureRecognizer item) => gestures.Contains(item);
		public void CopyTo(GestureRecognizer [] array, int arrayIndex) => gestures.CopyTo(array, arrayIndex);
		public IEnumerator<GestureRecognizer> GetEnumerator() => gestures.GetEnumerator();

		public bool Remove(GestureRecognizer item)
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
