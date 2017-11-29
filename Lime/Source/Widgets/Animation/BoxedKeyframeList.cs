using System;
using System.Collections;
using System.Collections.Generic;

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
}
