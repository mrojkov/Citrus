using System;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public class AnimationTrack : IList<AnimationClip>
	{
		private readonly List<AnimationClip> clips;
		public AnimationTrackList Owner { get; internal set; }

		public int Count => clips.Count;
		public bool IsReadOnly => false;

		public AnimationTrack(int capacity = 0)
		{
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
			clip.Owner = this;
		}

		public bool Remove(AnimationClip clip)
		{
			int index = GetIndexByFrame(clip.Frame);
			if (index < 0) {
				return false;
			}
			if (clip.Owner != this) {
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

		public AnimationTrack Clone()
		{
			var result = new AnimationTrack(Count);
			foreach (var clip in this) {
				result.Add(clip.Clone());
			}
			return result;
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
				if (clips[m].Frame < frame) {
					l = m + 1;
				} else if (clips[m].Frame > frame) {
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
			clip.Owner = this;
			clips.Add(clip);
		}

		public void AddOrdered(AnimationClip clip)
		{
			if (clip.Owner != null) {
				throw new InvalidOperationException();
			}
			clip.Owner = this;
			if (Count == 0 || clip.Frame > this[Count - 1].Frame) {
				clips.Add(clip);
			} else {
				int i = 0;
				while (clips[i].Frame < clip.Frame) {
					i++;
				}
				if (clips[i].Frame == clip.Frame) {
					clips[i] = clip;
				} else {
					clips.Insert(i, clip);
				}
			}
		}
	}
}
