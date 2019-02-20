using System;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public class AnimationTrackList : IList<AnimationTrack>
	{
		private readonly List<AnimationTrack> tracks;
		public Animation Owner { get; internal set; }

		public int Count => tracks.Count;
		public bool IsReadOnly => false;

		public AnimationTrackList(int capacity = 0)
		{
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
			track.Owner = this;
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

		public AnimationTrackList Clone()
		{
			var result = new AnimationTrackList(Count);
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
			track.Owner = this;
		}
	}
}
