using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	public class BoxedKeyframeList<T> : IKeyframeList
	{
		private TypedKeyframeList<T> source;

		public int Version => source.Version;

		public int Count => source.Count;

		public bool IsReadOnly => false;

		public BoxedKeyframeList(TypedKeyframeList<T> source)
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

		public IKeyframe GetByFrame(int frame)
		{
			int index = GetIndexByFrame(frame);
			return index >= 0 ? source[GetIndexByFrame(frame)] : null;
		}

		private int GetIndexByFrame(int frame)
		{
			int l = 0;
			int r = source.Count - 1;
			while (l <= r) {
				int m = (l + r) / 2;
				if (source[m].Frame < frame) {
					l = m + 1;
				} else if (source[m].Frame > frame) {
					r = m - 1;
				} else {
					return m;
				}
			}
			return -1;
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
			int index = GetIndexByFrame(item.Frame);
			if (index < 0) {
				return false;
			}
			source.RemoveAt(index);
			return true;
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
}
