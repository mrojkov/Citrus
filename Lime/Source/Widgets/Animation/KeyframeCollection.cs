using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public interface IKeyframeCollection : ICollection<IKeyframe>
	{
		IKeyframe this[int index] { get; set; }

		IKeyframe CreateKeyframe();

		void Add(int frame, object value, KeyFunction function = KeyFunction.Linear);
	}

	public class BoxedKeyframeCollection<T> : IKeyframeCollection
	{
		KeyframeCollection<T> source;

		public BoxedKeyframeCollection(KeyframeCollection<T> source)
		{
			this.source = source;
		}

		public void Add(int frame, object value, KeyFunction function = KeyFunction.Linear)
		{
			Add(new Keyframe<T> {
				Frame = frame,
				Value = (T)value,
				Function = function
			});
		}

		public void Add(IKeyframe item)
		{
			source.Add((Keyframe<T>)item);
		}

		public IKeyframe CreateKeyframe()
		{
			return new Keyframe<T>();
		}

		public IKeyframe this[int index]
		{
			get { return (IKeyframe)source[index]; }
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
				array[arrayIndex++] = (IKeyframe)item;
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
	}

	[ProtoContract]
	public class KeyframeCollection<T> : ICollection<Keyframe<T>>
	{
		internal bool Shared;

		List<Keyframe<T>> items = new List<Keyframe<T>>();

		public Keyframe<T> this[int index] {
			get { return items[index]; }
			set { items[index] = value; }
		}

		public KeyframeCollection<T> Clone()
		{
			var clone = new KeyframeCollection<T>();
			foreach (var i in this.items) {
				clone.Add(i.Clone());
			}
			return clone;
		}

		public void Add(Keyframe<T> item)
		{
			items.Add(item);
		}

		public void Add(int frame, T value, KeyFunction function = KeyFunction.Linear)
		{
			items.Add(new Keyframe<T>(frame, value, function));
		}

		public void Clear()
		{
			items.Clear();
		}

		public bool Contains(Keyframe<T> item)
		{
			return items.Contains(item);
		}

		public void CopyTo(Keyframe<T>[] array, int arrayIndex)
		{
			items.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return items.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(Keyframe<T> item)
		{
			return items.Remove(item);
		}

		public IEnumerator<Keyframe<T>> GetEnumerator()
		{
			return items.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return items.GetEnumerator();
		}
	}
}
