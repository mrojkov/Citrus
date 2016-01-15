using System;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public interface INotifyListChanged
	{
		event Action Changed;
	}

	public class ObservableList<T> : IList<T>, INotifyListChanged
	{
		private List<T> items;
		private Action changed;

		event Action INotifyListChanged.Changed
		{
			add { changed += value; }
			remove { changed -= value; }
		}

		protected virtual void OnChanged()
		{
			if (changed != null) {
				changed();	
			}
		}

		public ObservableList()
		{
			items = new List<T>();
		}

		public void Add(T item)
		{
			items.Add(item);
			OnChanged();
		}

		public void Clear()
		{
			items.Clear();
			OnChanged();
		}

		public bool Contains(T item)
		{
			return items.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			items.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			if (items.Remove(item)) {
				OnChanged();
				return true;
			}
			return false;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return items.GetEnumerator();
		}

		public int Count { get { return items.Count; } }

		public bool IsReadOnly { get { return false; } }

		public T this[int index]
		{
			get { return items[index]; }
			set
			{
				items[index] = value;
				OnChanged();
			}
		}

		public int IndexOf(T item)
		{
			return items.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			items.Insert(index, item);
			OnChanged();
		}

		public void RemoveAt(int index)
		{
			items.RemoveAt(index);
			OnChanged();
		}
	}
}

