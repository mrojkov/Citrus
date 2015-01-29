using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Lime.Widgets2
{
	[ProtoContract]
	public class ComponentList : IList<Component>
	{
		Node Owner;
		List<Component> items = new List<Component>();
		Behaviour[] behaviours;
		Behaviour[] lateBehaviours;

		public ComponentList(Node owner)
		{
			Owner = owner;
		}

		public Behaviour[] Behaviours
		{
			get
			{
				if (behaviours == null) {
					behaviours = GetBehaviourArray(lateBehaviours: false);
				}
				return behaviours;
			}
		}

		public Behaviour[] LateBehaviours
		{
			get
			{
				if (lateBehaviours == null) {
					lateBehaviours = GetBehaviourArray(lateBehaviours: true);
				}
				return lateBehaviours;
			}
		}

		private Behaviour[] GetBehaviourArray(bool lateBehaviours)
		{
			int c = 0;
			foreach (var i in items) {
				var b = i as Behaviour;
				if (b != null && b.IsLate == lateBehaviours) {
					c++;
				}
			}
			int j = 0;
			var behaviours = new Behaviour[c];
			foreach (var i in items) {
				var b = i as Behaviour;
				if (b != null && b.IsLate == lateBehaviours) {
					behaviours[j++] = b;
				}
			}
			return behaviours;
		}

		public int IndexOf(Component item)
		{
			return items.IndexOf(item);
		}

		public void Insert(int index, Component item)
		{
			behaviours = lateBehaviours = null;
			items.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			behaviours = lateBehaviours = null;
			items.RemoveAt(index);
		}

		public Component this[int index]
		{
			get
			{
				return items[index];
			}
			set
			{
				behaviours = lateBehaviours = null;
				items[index] = value;
			}
		}

		public void Add(Component item)
		{
			behaviours = lateBehaviours = null;
			items.Add(item);
		}

		public void Clear()
		{
			behaviours = lateBehaviours = null;
			items.Clear();
		}

		public bool Contains(Component item)
		{
			return items.Contains(item);
		}

		public void CopyTo(Component[] array, int arrayIndex)
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

		public bool Remove(Component item)
		{
			behaviours = lateBehaviours = null;
			return items.Remove(item);
		}

		public IEnumerator<Component> GetEnumerator()
		{
			return items.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return items.GetEnumerator();
		}

		public ComponentList Clone(Node owner)
		{
			var clone = new ComponentList(owner);
			foreach (var c in this) {
				clone.Add(c.Clone());
			}
			return clone;
		}
	}
}
