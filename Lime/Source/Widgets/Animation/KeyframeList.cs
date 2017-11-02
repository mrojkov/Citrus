using System;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public interface IKeyframeList : IList<IKeyframe>
	{
		IKeyframe CreateKeyframe();

		void Add(int frame, object value, KeyFunction function = KeyFunction.Linear);
		void AddOrdered(int frame, object value, KeyFunction function = KeyFunction.Linear);
		void AddOrdered(IKeyframe keyframe);

		int Version { get; }
	}

	public class KeyframeListProxy<T> : IKeyframeList
	{
		private KeyframeList<T> source;

		public int Version => source.Version;

		public int Count => source.Count;

		public bool IsReadOnly => false;

		public KeyframeListProxy(KeyframeList<T> source)
		{
			this.source = source;
		}

		public void Add(IKeyframe item)
		{
			source.Add((Keyframe<T>)item);
		}

		public void Add(int frame, object value, KeyFunction function = KeyFunction.Linear)
		{
			Add(new Keyframe<T> {
				Frame = frame,
				Value = (T)value,
				Function = function
			});
		}

		public void AddOrdered(IKeyframe item)
		{
			source.AddOrdered((Keyframe<T>)item);
		}

		public void AddOrdered(int frame, object value, KeyFunction function = KeyFunction.Linear)
		{
			AddOrdered(new Keyframe<T> {
				Frame = frame,
				Value = (T)value,
				Function = function
			});
		}

		public IKeyframe CreateKeyframe() => new Keyframe<T>();

		public IKeyframe this[int index]
		{
			get { return source[index]; }
			set { source[index] = (Keyframe<T>)value; }
		}

		public void Clear()
		{
			source.Clear();
		}

		public bool Contains(IKeyframe item)
		{
			return source.Contains((Keyframe<T>)item);
		}

		public void CopyTo(IKeyframe[] array, int arrayIndex)
		{
			foreach (var item in source) {
				array[arrayIndex++] = item;
			}
		}

		public bool Remove(IKeyframe item)
		{
			return source.Remove((Keyframe<T>)item);
		}

		public IEnumerator<IKeyframe> GetEnumerator()
		{
			return source.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return source.GetEnumerator();
		}

		public int IndexOf(IKeyframe item)
		{
			return source.IndexOf((Keyframe<T>)item);
		}

		public void Insert(int index, IKeyframe item)
		{
			source.Insert(index, (Keyframe<T>)item);
		}

		public void RemoveAt(int index)
		{
			source.RemoveAt(index);
		}
	}

	public class KeyframeList<T> : IList<Keyframe<T>>
	{
		private List<Keyframe<T>> items = new List<Keyframe<T>>();

		public int Version { get; private set; }
		internal int RefCount { get; private set; }
		public int Count => items.Count;
		public bool IsReadOnly => false;

		internal void AddRef() => RefCount++;
		internal void Release() => RefCount--;

		public KeyframeList<T> Clone()
		{
			var clone = new KeyframeList<T>();
			foreach (var i in this) {
				clone.Add(i.Clone());
			}
			return clone;
		}

		public void Add(Keyframe<T> item)
		{
			Version++;
			if (Count == 0 || item.Frame > this[Count - 1].Frame) {
				items.Add(item);
			} else {
				throw new InvalidOperationException();
			}
		}

		public void Add(int frame, T value, KeyFunction function = KeyFunction.Linear)
		{
			Add(new Keyframe<T> {
				Frame = frame,
				Value = value,
				Function = function
			});
		}

		public void AddOrdered(Keyframe<T> keyframe)
		{
			Version++;
			if (Count == 0 || keyframe.Frame > this[Count - 1].Frame) {
				items.Add(keyframe);
			} else {
				int i = 0;
				while (this[i].Frame < keyframe.Frame) {
					i++;
				}
				if (this[i].Frame == keyframe.Frame) {
					this[i] = keyframe;
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

		public Keyframe<T> CreateKeyframe()
		{
			return new Keyframe<T>();
		}

		public Keyframe<T> this[int index]
		{
			get { return items[index]; }
			set { items[index] = value; Version++; }
		}

		public void Clear()
		{
			items.Clear();
			Version++;
		}

		public bool Contains(Keyframe<T> item)
		{
			return items.Contains(item);
		}

		public void CopyTo(Keyframe<T>[] array, int arrayIndex)
		{
			foreach (var item in items) {
				array[arrayIndex++] = item;
			}
		}

		public bool Remove(Keyframe<T> item)
		{
			Version++;
			return items.Remove(item);
		}

		public int IndexOf(Keyframe<T> item)
		{
			return items.IndexOf(item);
		}

		public void Insert(int index, Keyframe<T> item)
		{
			Version++;
			items.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			Version++;
			items.RemoveAt(index);
		}

		public IEnumerator<Keyframe<T>> GetEnumerator()
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

		public int FindIndex(Predicate<Keyframe<T>> match)
		{
			return items.FindIndex(match);
		}

		public int FindLastIndex(Predicate<Keyframe<T>> match)
		{
			return items.FindLastIndex(match);
		}
	}
}
