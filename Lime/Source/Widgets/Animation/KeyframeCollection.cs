using System.Collections.Generic;
using System;

namespace Lime
{
	public interface IKeyframeCollection : IList<IKeyframe>
	{
		IKeyframe CreateKeyframe();

		void Add(int frame, object value, KeyFunction function = KeyFunction.Linear);
		void AddOrdered(int frame, object value, KeyFunction function = KeyFunction.Linear);
		void AddOrdered(IKeyframe keyframe);

		int Version { get; }
	}

	public class KeyframeCollectionProxy<T> : IKeyframeCollection
	{
		KeyframeCollection<T> source;

		public int Version { get; private set; }

		public KeyframeCollectionProxy(KeyframeCollection<T> source)
		{
			this.source = source;
		}

		public void Add(IKeyframe item)
		{
			Version++;
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
			Version++;
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

		public IKeyframe CreateKeyframe()
		{
			return new Keyframe<T>();
		}

		public IKeyframe this[int index]
		{
			get { return source[index]; }
			set { source[index] = (Keyframe<T>)value; Version++; }
		}

		public void Clear()
		{
			source.Clear();
			Version++;
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

		public int Count
		{
			get { return source.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(IKeyframe item)
		{
			Version++;
			return source.Remove((Keyframe<T>)item);
		}

		public IEnumerator<IKeyframe> GetEnumerator()
		{
			return source.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return source.GetEnumerator();
		}

		public int IndexOf(IKeyframe item)
		{
			return source.IndexOf((Keyframe<T>)item);
		}

		public void Insert(int index, IKeyframe item)
		{
			Version++;
			source.Insert(index, (Keyframe<T>)item);
		}

		public void RemoveAt(int index)
		{
			Version++;
			source.RemoveAt(index);
		}
	}

	public class KeyframeCollection<T> : List<Keyframe<T>>
	{
		internal int RefCount { get; private set; }

		internal void AddRef() => RefCount++;
		internal void Release() => RefCount--;

		public KeyframeCollection<T> Clone()
		{
			var clone = new KeyframeCollection<T>();
			foreach (var i in this) {
				clone.Add(i.Clone());
			}
			return clone;
		}

		public void Add(int frame, T value, KeyFunction function = KeyFunction.Linear)
		{
			Add(new Keyframe<T>(frame, value, function));
		}

		public new void Add(Keyframe<T> keyframe)
		{
			if (Count == 0 || keyframe.Frame > this[Count - 1].Frame) {
				base.Add(keyframe);
			} else {
				throw new InvalidOperationException();
			}
		}

		public void AddOrdered(Keyframe<T> keyframe)
		{
			if (Count == 0 || keyframe.Frame > this[Count - 1].Frame) {
				base.Add(keyframe);
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
	}
}
