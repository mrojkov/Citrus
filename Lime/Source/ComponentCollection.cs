using System;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public class ComponentCollection<Component> : IEnumerable<Component>
	{
		protected Bucket[] buckets = empty;

		private static Bucket[] empty = new Bucket[0];

		public bool Contains<T>() where T : Component
		{
			return Get<T>() != null;
		}

		public T Get<T>() where T : Component
		{
			var key = ComponentKeyResolver<T>.Key;
			for (var i = 0; i < buckets.Length; i++) {
				var b = buckets[(key + i) & (buckets.Length - 1)];
				if (b.Key == key) {
					return (T)b.Component;
				}
				if (b.Key == 0) {
					break;
				}
			}
			return default(T);
		}

		public T GetOrAdd<T>() where T : Component, new()
		{
			var c = Get<T>();
			if (c == null) {
				c = new T();
				Add(c);
			}
			return c;
		}

		public virtual void Add<T>(T component) where T : Component
		{
			if (Contains<T>()) {
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
						AddHelper(newBuckets, b.Key, b.Component);
					}
				}
				buckets = newBuckets;
			}
			AddHelper(buckets, ComponentKeyResolver<T>.Key, component);
		}

		private static void AddHelper<T>(Bucket[] buckets, int key, T component) where T: Component
		{
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

		private float CalcLoadFactor()
		{
			int c = 0;
			foreach (var b in buckets) {
				if (b.Key > 0) {
					c++;
				}
			}
			return ((float)c) / buckets.Length;
		}

		public virtual bool Remove<T>() where T : Component
		{
			var key = ComponentKeyResolver<T>.Key;
			for (var i = 0; i < buckets.Length; i++) {
				var j = (key + i) & (buckets.Length - 1);
				if (buckets[j].Key == key) {
					buckets[j].Key = -1;
					buckets[j].Component = default(Component);
					return true;
				}
			}
			return false;
		}

		IEnumerator<Component> IEnumerable<Component>.GetEnumerator()
		{
			foreach (var b in buckets) {
				if (b.Key > 0) {
					yield return b.Component;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (var b in buckets) {
				if (b.Key > 0) {
					yield return b.Component;
				}
			}
		}

		public void Clear()
		{
			for (int i = 0; i < buckets.Length; i++) {
				buckets[i].Key = 0;
				buckets[i].Component = default(Component);
			}
		}

		private static int componentKeyCounter = 1;

		private static class ComponentKeyResolver<T> where T : Component
		{
			public static readonly int Key = System.Threading.Interlocked.Increment(ref componentKeyCounter);
		}

		protected struct Bucket
		{
			// Key special values:
			//  0 - means an empty bucket.
			// -1 - a deleted component.
			public int Key;
			public Component Component;
		}
	}
}
