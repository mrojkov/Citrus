using ProtoBuf;
using System.Collections.Generic;

namespace Lime
{
	public interface IKeyframeCollection : ICollection<IKeyframe>
	{
		IKeyframe this[int index] { get; set; }

		IKeyframe CreateKeyframe();

		void Add(int frame, object value, KeyFunction function = KeyFunction.Linear);
	}

	public class KeyframeCollectionProxy<T> : IKeyframeCollection
	{
		KeyframeCollection<T> source;

		public KeyframeCollectionProxy(KeyframeCollection<T> source)
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
			foreach (var i in source) {
				yield return (IKeyframe)i;
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return source.GetEnumerator();
		}
	}

	[ProtoContract]
	public class KeyframeCollection<T> : List<Keyframe<T>>
	{
		internal bool Shared;

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
	}
}
