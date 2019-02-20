using System;
using System.Collections;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	public class AnimationTrack
	{
		public Animation Owner { get; internal set; }

		[YuzuMember]
		public string Id { get; set; }

		[YuzuMember]
		public AnimationClipList Clips { get; private set; }

		public AnimationTrack()
		{
			Clips = new AnimationClipList(this);
		}

		public AnimationTrack(int capacity = 0)
		{
			Clips = new AnimationClipList(this, capacity);
		}

		public AnimationTrack Clone()
		{
			var result = (AnimationTrack)MemberwiseClone();
			result.Owner = null;
			result.Clips = new AnimationClipList(result, Clips.Count);
			foreach (var clip in Clips) {
				result.Clips.Add(clip.Clone());
			}
			return result;
		}
	}

	public class AnimationTrackList : IList<AnimationTrack>
	{
		private readonly List<AnimationTrack> tracks;
		private readonly Animation owner;

		public int Count => tracks.Count;
		public bool IsReadOnly => false;

		public AnimationTrackList(Animation owner, int capacity = 0)
		{
			this.owner = owner;
			tracks = new List<AnimationTrack>(capacity);
		}

		public void Clear()
		{
			for (int i = Count - 1; i >= 0; i--) {
				RemoveAt(i);
			}
		}

		public bool Contains(AnimationTrack item) => tracks.Contains(item);
		public void CopyTo(AnimationTrack[] array, int arrayIndex) => tracks.CopyTo(array, arrayIndex);
		public List<AnimationTrack>.Enumerator GetEnumerator() => tracks.GetEnumerator();

		IEnumerator<AnimationTrack> IEnumerable<AnimationTrack>.GetEnumerator() => tracks.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => tracks.GetEnumerator();

		public int IndexOf(AnimationTrack item) => tracks.IndexOf(item);

		public void Insert(int index, AnimationTrack track)
		{
			if (track.Owner != null) {
				throw new InvalidOperationException();
			}
			tracks.Insert(index, track);
			track.Owner = owner;
		}

		public bool Remove(AnimationTrack track)
		{
			if (tracks.Remove(track)) {
				track.Owner = null;
				return true;
			}
			return false;
		}

		public void RemoveAt(int index)
		{
			tracks[index].Owner = null;
			tracks.RemoveAt(index);
		}

		public AnimationTrack this[int index]
		{
			get { return tracks[index]; }
			set { throw new NotSupportedException(); }
		}

		public AnimationTrackList Clone(Animation newOwner)
		{
			var result = new AnimationTrackList(newOwner, Count);
			foreach (var track in this) {
				result.Add(track.Clone());
			}
			return result;
		}

		public int FindIndex(Predicate<AnimationTrack> match)
		{
			return tracks.FindIndex(match);
		}

		public bool Exists(Predicate<AnimationTrack> match)
		{
			return tracks.Exists(match);
		}

		public void Add(AnimationTrack track)
		{
			if (track.Owner != null) {
				throw new InvalidOperationException();
			}
			tracks.Add(track);
			track.Owner = owner;
		}
	}
}
