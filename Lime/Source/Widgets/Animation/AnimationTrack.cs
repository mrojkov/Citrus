using System;
using System.Collections;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	public class AnimationTrack : IAnimationHost, IAnimable
	{
		public const float MaxWeight = 100;

		public Animation Owner { get; internal set; }

		public object UserData { get; set; }

		[YuzuMember]
		[TangerineGroup("Track")]
		public string Id { get; set; }

		[YuzuMember]
		[TangerineGroup("Track")]
		[TangerineValidRange(0.0f, MaxWeight)]
		[TangerineKeyframeColor(1)]
		public float Weight { get; set; } = MaxWeight;

		internal void InvalidateCache()
		{
			Owner?.InvalidateCache();
		}

		[YuzuMember]
		[TangerineIgnore]
		public AnimationClipList Clips { get; private set; }

		[YuzuMember]
		public AnimatorCollection Animators { get; private set; }

		IAnimable IAnimable.Owner { get => null; set => throw new NotSupportedException(); }
		void IAnimationHost.OnAnimatorCollectionChanged() { }
		Component IAnimationHost.GetComponent(Type type) => throw new NotSupportedException();

		[TangerineIgnore]
		[YuzuMember]
		public TangerineFlags TangerineFlags { get; set; }

		public bool GetTangerineFlag(TangerineFlags flag) => (TangerineFlags & flag) != 0;

		public void SetTangerineFlag(TangerineFlags flag, bool value)
		{
			if (value) {
				TangerineFlags |= flag;
			} else {
				TangerineFlags &= ~flag;
			}
		}

		private double cachedTime = double.NaN;
		private float cachedWeight;

		public float CalcWeight(double time)
		{
			if (time == cachedTime) {
				return cachedWeight;
			}
			if (Animators.Count > 0) {
				foreach (var a in Animators) {
					a.Apply(time);
				}
			}
			cachedTime = time;
			cachedWeight = Weight.Clamp(0, MaxWeight);
			return cachedWeight;
		}

		public AnimationTrack()
		{
			Clips = new AnimationClipList(this);
			Animators = new AnimatorCollection(this);
		}

		void IAnimationHost.OnTrigger(string property, object value, double animationTimeCorrection) { }
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
			owner?.InvalidateCache();
		}

		public bool Remove(AnimationTrack track)
		{
			if (tracks.Remove(track)) {
				track.Owner = null;
				owner?.InvalidateCache();
				return true;
			}
			return false;
		}

		public void RemoveAt(int index)
		{
			tracks[index].Owner = null;
			tracks.RemoveAt(index);
			owner?.InvalidateCache();
		}

		public AnimationTrack this[int index]
		{
			get { return tracks[index]; }
			set { throw new NotSupportedException(); }
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
			owner?.InvalidateCache();
		}
	}
}
