using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public sealed class AnimationList : List<Animation>
	{
		public Animation this[string id]
		{
			get { return Find(id); }
		}

		public AnimationList() { }
		public AnimationList(int count) : base(count) { }
		
		public AnimationList Clone()
		{
			var result = new AnimationList(Count);
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
	}
}
