using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Lime
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class MutuallyExclusiveDerivedComponentsAttribute : Attribute
	{ }

	public class Component
	{
		private static Dictionary<Type, int> keyMap = new Dictionary<Type, int>();
		private static int keyCounter;

		internal static int GetKeyForType(Type type)
		{
			lock (keyMap) {
				if (keyMap.TryGetValue(type, out int key)) {
					return key;
				}
				Type t = type;
				while (t != null) {
					if (t.GetCustomAttribute<MutuallyExclusiveDerivedComponentsAttribute>(false) != null) {
						break;
					}
					t = t.BaseType;
				}
				t = t ?? type;
				if (!keyMap.TryGetValue(t, out key)) {
					key = ++keyCounter;
					keyMap.Add(t, key);
				}
				return key;
			}
		}

		internal int GetKey() => GetKeyForType(GetType());
	}

	public class ComponentCollection<TComponent> : ICollection<TComponent> where TComponent : Component
	{
		private static Bucket[] emptyBuckets = new Bucket[0];

		protected Bucket[] buckets = emptyBuckets;

		public int Count { get; private set; }

		public bool IsReadOnly => false;

		public virtual bool Contains(TComponent component) => ContainsKey(component.GetKey());
		public bool Contains<T>() where T : TComponent => ContainsKey(ComponentKeyResolver<T>.Key);
		public bool Contains(Type type) => ContainsKey(Component.GetKeyForType(type));

		private bool ContainsKey(int key)
		{
			for (var i = 0; i < buckets.Length; i++) {
				var b = buckets[(key + i) & (buckets.Length - 1)];
				if (b.Key == key) {
					return true;
				}
				if (b.Key == 0) {
					break;
				}
			}
			return false;
		}

		private TComponent Get(int key)
		{
			for (var i = 0; i < buckets.Length; i++) {
				var b = buckets[(key + i) & (buckets.Length - 1)];
				if (b.Key == key) {
					return b.Component;
				}
				if (b.Key == 0) {
					break;
				}
			}
			return default(TComponent);
		}

		public TComponent Get(Type type) => Get(Component.GetKeyForType(type));

		public T Get<T>() where T : TComponent => Get(ComponentKeyResolver<T>.Key) as T;

		public T GetOrAdd<T>() where T : TComponent, new()
		{
			var c = Get<T>();
			if (c == null) {
				c = new T();
				Add(c);
			}
			return c;
		}

		public virtual void Add(TComponent component)
		{
			if (Contains(component)) {
				throw new InvalidOperationException("Attempt to add a component twice.");
			}
			if (buckets.Length == 0) {
				buckets = new Bucket[1];
			}
			var loadFactor = CalcLoadFactor();
			if (loadFactor >= 0.7f) {
				var newBuckets = new Bucket[buckets.Length * 2];
				foreach (var b in buckets) {
					if (b.Key > 0) {
						AddHelper(newBuckets, b.Component);
					}
				}
				buckets = newBuckets;
			}
			AddHelper(buckets, component);
			Count++;
		}

		private float CalcLoadFactor() => (float)Count / buckets.Length;

		private static void AddHelper(Bucket[] buckets, TComponent component)
		{
			int key = component.GetKey();
			for (var i = 0; i < buckets.Length; i++) {
				var j = (key + i) & (buckets.Length - 1);
				if (buckets[j].Key <= 0) {
					buckets[j].Key = key;
					buckets[j].Component = component;
					return;
				}
			}
			throw new InvalidOperationException();
		}

		public bool Remove<T>() where T : TComponent
		{
			var c = Get<T>();
			return c != null && Remove(c);
		}

		public bool Remove(Type type)
		{
			var c = Get(type);
			return c != null && Remove(c);
		}

		public virtual bool Remove(TComponent component)
		{
			var key = component.GetKey();
			for (var i = 0; i < buckets.Length; i++) {
				var j = (key + i) & (buckets.Length - 1);
				if (buckets[j].Key == key) {
					buckets[j].Key = -1;
					buckets[j].Component = default(TComponent);
					Count--;
					return true;
				}
			}
			return false;
		}

		public bool Replace<T>(T component) where T : TComponent
		{
			var r = Remove<T>();
			Add(component);
			return r;
		}

		IEnumerator<TComponent> IEnumerable<TComponent>.GetEnumerator() => new Enumerator(buckets);

		IEnumerator IEnumerable.GetEnumerator() => new Enumerator(buckets);

		public Enumerator GetEnumerator() => new Enumerator(buckets);

		public struct Enumerator : IEnumerator<TComponent>
		{
			private int index;
			private Bucket[] buckets;

			public Enumerator(Bucket[] buckets)
			{
				index = -1;
				this.buckets = buckets;
			}

			public TComponent Current => buckets[index].Component;
			object IEnumerator.Current => buckets[index].Component;

			public bool MoveNext()
			{
				for (index++; index < buckets.Length; index++) {
					if (buckets[index].Key > 0) {
						return true;
					}
				}
				return false;
			}

			public void Reset()
			{
				index = -1;
			}

			public void Dispose() { }
		}

		public virtual void Clear()
		{
			for (int i = 0; i < buckets.Length; i++) {
				buckets[i].Key = 0;
				buckets[i].Component = default(TComponent);
			}
			Count = 0;
		}

		public void CopyTo(TComponent[] array, int arrayIndex)
		{
			for (var i = 0; i < buckets.Length; i++) {
				if (buckets[i].Key > 0) {
					array[arrayIndex++] = buckets[i].Component;
				}
			}
		}

		private static class ComponentKeyResolver<T> where T : TComponent
		{
			public static readonly int Key = Component.GetKeyForType(typeof(T));
		}

		public struct Bucket
		{
			// Key special values:
			//  0 - means an empty bucket.
			// -1 - a deleted component.
			public int Key;
			public TComponent Component;
		}
	}
}
