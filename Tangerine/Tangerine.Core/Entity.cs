using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lime;

namespace Tangerine.Core
{
	public interface IComponent { }

	public class ComponentCollection : IEnumerable<IComponent>
	{
		private BitSet32 componentSet = new BitSet32();

		public Entity Owner { get; private set; }

		public ComponentCollection(Entity owner)
		{
			Owner = owner;
		}

		public void Clear()
		{
			for (int i = 0; i < componentSet.Count; i++) {
				if (componentSet[i]) {
					componentSet[i] = false;
					ComponentMapRegistry.Instance.Maps[i].Remove(Owner);
				}
			}
		}

		public bool Has<T>() where T : IComponent
		{
			return ComponentMap<T>.Instance.Get(Owner) != null;
		}

		public T Get<T>() where T : IComponent
		{
			return ComponentMap<T>.Instance.Get(Owner);
		}

		public void Add<T>(T component) where T : IComponent
		{
			var map = ComponentMap<T>.Instance;
			int index = ComponentMapRegistry.Instance.TypeIndices[typeof(T)];
			if (componentSet[index]) {
				throw new ArgumentException("Attempt to add a component twice");
			}
			map.Add(Owner, component);
			componentSet[index] = true;
		}

		public void Remove<T>() where T : IComponent
		{
			var map = ComponentMap<T>.Instance;
			int index = ComponentMapRegistry.Instance.TypeIndices[typeof(T)];
			if (!componentSet[index]) {
				throw new ArgumentException("Attempt to remove a missing component");
			}
			map.Remove(Owner);
			componentSet[index] = false;
		}

		IEnumerator<IComponent> IEnumerable<IComponent>.GetEnumerator()
		{
			for (int i = 0; i < componentSet.Count; i++) {
				if (componentSet[i]) {
					yield return ComponentMapRegistry.Instance.Maps[i].Get(Owner);
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			for (int i = 0; i < componentSet.Count; i++) {
				if (componentSet[i]) {
					yield return ComponentMapRegistry.Instance.Maps[i].Get(Owner);
				}
			}
		}
	}

	internal class ComponentMapRegistry
	{
		public static readonly ComponentMapRegistry Instance = new ComponentMapRegistry();

		public readonly List<IComponentMap> Maps = new List<IComponentMap>();
		public readonly Dictionary<Type, int> TypeIndices = new Dictionary<Type, int>();
	}

	internal interface IComponentMap
	{
		IComponent Get(Entity entity);
		void Add(Entity entity, IComponent component);
		void Remove(Entity entity);
	}

	internal class ComponentMap<T> : IComponentMap where T : IComponent
	{
		public static readonly ComponentMap<T> Instance = new ComponentMap<T>();

		private readonly Dictionary<int, T> components = new Dictionary<int, T>();

		ComponentMap()
		{
			var r = ComponentMapRegistry.Instance;
			r.TypeIndices.Add(typeof(T), r.Maps.Count);
			r.Maps.Add(this);
		}

		public T Get(Entity entity)
		{
			T value;
			return components.TryGetValue(entity.uid, out value) ? value : default(T);
		}

		public void Add(Entity entity, T component)
		{
			components.Add(entity.uid, component);
		}

		public void Remove(Entity entity)
		{
			components.Remove(entity.uid);
		}

		IComponent IComponentMap.Get(Entity entity)
		{
			return Get(entity);
		}

		void IComponentMap.Add(Entity entity, IComponent component)
		{
			Add(entity, (T)component);
		}
	}

	public class Entity : IDisposable
	{
		internal int uid;	
		private static readonly UidManager uidManager = new UidManager();
		public ComponentCollection Components { get; private set; }

		public Entity()
		{
			uid = uidManager.Acquire();
			Components = new ComponentCollection(this);
		}

		~Entity()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool dispose)
		{
			Components.Clear();
			uidManager.Release(uid);
		}
	}

	internal class UidManager
	{
		private int uids;
		private Stack<int> freeUids = new Stack<int>();

		public int Acquire()
		{
			return freeUids.Count > 0 ? freeUids.Pop() : uids++;
		}

		public void Release(int uid)
		{
			freeUids.Push(uid);
		}
	}
}