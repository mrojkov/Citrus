using System;
using System.Collections;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	public class AnimationClip
	{
		[YuzuMember]
		public string AnimationId { get; set; }

		[YuzuMember]
		[TangerineReadOnly]
		public int Begin { get; set; }

		[YuzuMember]
		[TangerineReadOnly]
		public int End { get; set; }

		[YuzuMember]
		public int Offset { get; set; }

		[YuzuMember]
		public bool Reversed { get; set; }

		internal Animation Animation
		{
			get {
				var node = Owner.Owner.Owner;
				node.Animations.TryFind(AnimationId, out var animation);
				return animation;
			}
		}

		public int Length
		{
			get => End - Begin;
			set => End = Begin + value;
		}

		public AnimationTrack Owner { get; internal set; }

		public double RemapTime(double time)
		{
			if (!Reversed) {
				return time;
			}
			var relativeTime = time - AnimationUtils.FramesToSeconds(Begin - Offset);
			var t = 1 - (float)(relativeTime / AnimationUtils.FramesToSeconds(Length));
			return AnimationUtils.FramesToSeconds(Begin + Offset) + AnimationUtils.FramesToSeconds(Length) * t;
		}

		public AnimationClip Clone()
		{
			var clone = (AnimationClip)MemberwiseClone();
			clone.Owner = null;
			return clone;
		}
	}

	public class AnimationClipList : IList<AnimationClip>
	{
		private readonly List<AnimationClip> clips;
		private AnimationTrack owner;

		public int Count => clips.Count;
		public bool IsReadOnly => false;

		public AnimationClipList(AnimationTrack owner)
		{
			this.owner = owner;
			clips = new List<AnimationClip>();
		}

		public AnimationClipList(AnimationTrack owner, int capacity = 0)
		{
			this.owner = owner;
			clips = new List<AnimationClip>(capacity);
		}

		public void Clear()
		{
			for (int i = Count - 1; i >= 0; i--) {
				RemoveAt(i);
			}
		}

		public bool Contains(AnimationClip item) => clips.Contains(item);
		public void CopyTo(AnimationClip[] array, int arrayIndex) => clips.CopyTo(array, arrayIndex);
		public List<AnimationClip>.Enumerator GetEnumerator() => clips.GetEnumerator();

		IEnumerator<AnimationClip> IEnumerable<AnimationClip>.GetEnumerator() => clips.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => clips.GetEnumerator();

		public int IndexOf(AnimationClip item) => clips.IndexOf(item);

		public void Insert(int index, AnimationClip clip)
		{
			if (clip.Owner != null) {
				throw new InvalidOperationException();
			}
			clips.Insert(index, clip);
			clip.Owner = owner;
		}

		public bool Remove(AnimationClip clip)
		{
			int index = GetIndexByFrame(clip.Begin);
			if (index < 0) {
				return false;
			}
			if (clip.Owner != owner) {
				throw new InvalidOperationException();
			}
			clip.Owner = null;
			clips.RemoveAt(index);
			return true;
		}

		public void RemoveAt(int index)
		{
			clips[index].Owner = null;
			clips.RemoveAt(index);
		}

		public AnimationClip this[int index]
		{
			get { return clips[index]; }
			set { throw new NotSupportedException(); }
		}

		public AnimationClip GetByFrame(int frame)
		{
			int index = GetIndexByFrame(frame);
			return index >= 0 ? clips[index] : null;
		}

		private int GetIndexByFrame(int frame)
		{
			int l = 0;
			int r = clips.Count - 1;
			while (l <= r) {
				int m = (l + r) / 2;
				if (clips[m].Begin < frame) {
					l = m + 1;
				} else if (clips[m].Begin > frame) {
					r = m - 1;
				} else {
					return m;
				}
			}
			return -1;
		}

		public int FindIndex(Predicate<AnimationClip> match)
		{
			return clips.FindIndex(match);
		}

		public bool Exists(Predicate<AnimationClip> match)
		{
			return clips.Exists(match);
		}

		public void Add(AnimationClip clip)
		{
			if (clip.Owner != null) {
				throw new InvalidOperationException();
			}
			clip.Owner = owner;
			clips.Add(clip);
		}

		public void AddOrdered(AnimationClip clip)
		{
			if (clip.Owner != null) {
				throw new InvalidOperationException();
			}
			clip.Owner = owner;
			if (Count == 0 || clip.Begin > this[Count - 1].Begin) {
				clips.Add(clip);
			} else {
				int i = 0;
				while (clips[i].Begin < clip.Begin) {
					i++;
				}
				if (clips[i].Begin == clip.Begin) {
					clips[i] = clip;
				} else {
					clips.Insert(i, clip);
				}
			}
		}
	}

}
