using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Lime
{
	public sealed class AnimationList : IList<Animation>
	{
		private Node owner;
		private Animation defaultAnimation;
		private List<Animation> list;

		public int Count
		{
			get { return list.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public Animation DefaultAnimation
		{
			get
			{
				if (defaultAnimation == null) {
					if (Count == 0) {
						Add(new Animation());
					}
					defaultAnimation = list[0];
				}
				return defaultAnimation;
			}
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
				defaultAnimation = null;
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

		public bool TryRun(string animationId, string markerId = null, double animationTimeCorrection = 0)
		{
			Animation animation;
			return TryFind(animationId, out animation) && animation.TryRun(markerId, animationTimeCorrection);
		}

		public void Run(string animationId, string markerId = null)
		{
			if (!TryRun(animationId, markerId)) {
				if (animationId != null && markerId != null) {
					throw new Lime.Exception(string.Format("Unknown animation ({0}) or marker ({1})", animationId, markerId));
				} else if (animationId != null && markerId == null) {
					throw new Lime.Exception(string.Format("Unknown animation ({0})", animationId));
				} else if (animationId == null && markerId != null) {
					throw new Lime.Exception(string.Format("Unknown marker ({0})", markerId));
				} else {
					throw new Lime.Exception("Unknown animation or marker");
				}
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
			defaultAnimation = null;
		}

		public void RemoveAt(int index)
		{
			list[index].Owner = null;
			list.RemoveAt(index);
			defaultAnimation = null;
		}

		public void Add(Animation item)
		{
			list.Add(item);
			item.Owner = owner;
			defaultAnimation = null;
		}

		public void AddRange(IEnumerable<Animation> collection)
		{
			foreach (var animation in collection) {
				Add(animation);
			}
		}

		public void Clear()
		{
			foreach (var animation in list) {
				animation.Owner = null;
			}
			list.Clear();
			defaultAnimation = null;
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
				defaultAnimation = null;
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
