using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public sealed class AnimationList : IList<Animation>
	{
		private Node owner;
		private List<Animation> list;

		public int Count
		{
			get { return list.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public Animation this[int index]
		{
			get { return list[index]; }
			set
			{
				var a = list[index];
				if (a != null) {
					a.Owner = null;
				}
				list[index] = value;
				value.Owner = owner;
			}
		}

		public Animation this[string id]
		{
			get { return Find(id); }
		}

		public AnimationList(Node owner)
		{
			this.owner = owner;
			list = new List<Animation>();
		}

		public AnimationList(Node owner, int count)
		{
			this.owner = owner;
			list = new List<Animation>(count);
		}
		
		public AnimationList Clone(Node owner)
		{
			var result = new AnimationList(owner, Count);
			foreach (var animation in this) {
				result.Add(animation.Clone());
			}
			return result;
		}

		public Animation Find(string id)
		{
			Animation animation;
			if (!TryFind(id, out animation)) {
				throw new Lime.Exception("Unknown animation '{0}'", id);
			}
			return animation;
		}

		public bool TryFind(string id, out Animation animation)
		{
			foreach (var a in this) {
				if (a.Id == id) {
					animation = a;
					return true;
				}
			}
			animation = null;
			return false;
		}

		public bool TryRun(string animationId, string markerId = null)
		{
			Animation animation;
			return TryFind(animationId, out animation) && animation.TryRun(markerId);
		}

		public void Run(string animationId, string markerId = null)
		{
			if (!TryRun(animationId, markerId)) {
				throw new Lime.Exception("Unknown animation or marker");
			}
		}

		public int IndexOf(Animation item)
		{
			return list.IndexOf(item);
		}

		public void Insert(int index, Animation item)
		{
			item.Owner = owner;
			list.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			list[index].Owner = null;
			list.RemoveAt(index);
		}

		public void Add(Animation item)
		{
			list.Add(item);
			item.Owner = owner;
		}

		public void Clear()
		{
			foreach (var animation in list) {
				animation.Owner = null;
			}
			list.Clear();
		}

		public bool Contains(Animation item)
		{
			return list.Contains(item);
		}

		public void CopyTo(Animation[] array, int arrayIndex)
		{
			list.CopyTo(array, arrayIndex);
		}

		public bool Remove(Animation item)
		{
			if (list.Remove(item)) {
				item.Owner = null;
				return true;
			}
			return false;
		}

		IEnumerator<Animation> IEnumerable<Animation>.GetEnumerator()
		{
			return list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator();
		}

		public List<Animation>.Enumerator GetEnumerator()
		{
			return list.GetEnumerator();
		}
	}
}
