using System;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public class TypedKeyframeList<T> : IList<Keyframe<T>>
	{
		private List<Keyframe<T>> items = new List<Keyframe<T>>();

		public int Version { get; private set; }
		internal int RefCount { get; private set; }
		public int Count => items.Count;
		public bool IsReadOnly => false;

		internal void AddRef() => RefCount++;
		internal void Release() => RefCount--;

		public TypedKeyframeList<T> Clone()
		{
			var clone = new TypedKeyframeList<T>();
			foreach (var i in this) {
				clone.Add(i.Clone());
			}
			return clone;
		}

		internal void IncreaseVersion() => Version++;

		public void Add(int frame, T value, KeyFunction function = KeyFunction.Linear)
		{
			Add(new Keyframe<T> {
				Frame = frame,
				Value = value,
				Function = function
			});
		}

		public void Add(int frame, object value, KeyFunction function = KeyFunction.Linear)
		{
			Add(new Keyframe<T> {
				Frame = frame,
				Value = (T)value,
				Function = function
			});
		}

		public void AddOrdered(Keyframe<T> keyframe)
		{
			Version++;
			if (Count == 0 || keyframe.Frame > items[Count - 1].Frame) {
				items.Add(keyframe);
			} else {
				int i = 0;
				while (items[i].Frame < keyframe.Frame) {
					i++;
				}
				if (items[i].Frame == keyframe.Frame) {
					items[i] = keyframe;
				} else {
					Insert(i, keyframe);
				}
			}
		}

		public void AddOrdered(int frame, T value, KeyFunction function = KeyFunction.Linear)
		{
			AddOrdered(new Keyframe<T> {
				Frame = frame,
				Value = value,
				Function = function
			});
		}

		public Keyframe<T> CreateKeyframe() => new Keyframe<T>();

		public int FindIndex(Predicate<Keyframe<T>> match)
		{
			return items.FindIndex(match);
		}

		public int FindLastIndex(Predicate<Keyframe<T>> match)
		{
			return items.FindLastIndex(match);
		}

		public void Add(Keyframe<T> item)
		{
			IncreaseVersion();
			items.Add(item);
		}

		public void AddRange(IEnumerable<Keyframe<T>> collection)
		{
			foreach (var i in collection) {
				Add(i);
			}
		}

		public Keyframe<T> this[int index]
		{
			get { return items[index]; }
			set { items[index] = value; }
		}

		public void Clear()
		{
			items.Clear();
			IncreaseVersion();
		}

		public bool Contains(Keyframe<T> item)
		{
			return items.Contains(item);
		}

		public void CopyTo(Keyframe<T>[] array, int arrayIndex)
		{
			foreach (var i in items) {
				array[arrayIndex++] = i;
			}
		}

		public bool Remove(Keyframe<T> item)
		{
			IncreaseVersion();
			if (items.Remove(item)) {
				return true;
			}
			return false;
		}

		public int IndexOf(Keyframe<T> item)
		{
			return items.IndexOf(item);
		}

		public void Insert(int index, Keyframe<T> item)
		{
			IncreaseVersion();
			items.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			IncreaseVersion();
			items.RemoveAt(index);
		}

		public List<Keyframe<T>>.Enumerator GetEnumerator()
		{
			return items.GetEnumerator();
		}

		IEnumerator<Keyframe<T>> IEnumerable<Keyframe<T>>.GetEnumerator()
		{
			return items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return items.GetEnumerator();
		}
	}
}
