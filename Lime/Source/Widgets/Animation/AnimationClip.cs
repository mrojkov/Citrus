using System;
using System.Collections;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	public enum AnimationClipExtrapolation
	{
		[TangerineIgnore]
		None,
		Hold,
		Repeat,
		PingPong
	}

	public class AnimationClip
	{
		private double cachedTime = double.NaN;
		private double cachedLocalTime;
		private string animationId;
		private int beginFrame;
		private int endFrame;
		private int inFrame;
		private bool reversed;
		private AnimationClipExtrapolation postExtrapolation = AnimationClipExtrapolation.Hold;

		public double BeginTime { get; private set; }
		public double EndTime { get; private set; }
		public double DurationInSeconds { get; private set; }
		public double InTime { get; private set; }

		[YuzuMember]
		[TangerineGroup("Clip")]
		public string AnimationId
		{
			get => animationId;
			set {
				if (animationId != value) {
					animationId = value;
					AnimationIdComparisonCode = Toolbox.StringUniqueCodeGenerator.Generate(value);
					Owner?.InvalidateCache();
				}
			}
		}

		public int AnimationIdComparisonCode { get; private set; }

		[YuzuMember]
		[TangerineReadOnly]
		[TangerineGroup("Clip")]
		public int BeginFrame
		{
			get => beginFrame;
			set {
				if (beginFrame != value) {
					beginFrame = value;
					BeginTime = value * AnimationUtils.SecondsPerFrame;
					DurationInSeconds = EndTime - BeginTime;
					Owner?.InvalidateCache();
				}
			}
		}

		[YuzuMember]
		[TangerineReadOnly]
		[TangerineGroup("Clip")]
		public int EndFrame
		{
			get => endFrame;
			set {
				if (endFrame != value) {
					endFrame = value;
					EndTime = value * AnimationUtils.SecondsPerFrame;
					DurationInSeconds = EndTime - BeginTime;
					Owner?.InvalidateCache();
				}
			}
		}

		[YuzuMember]
		[TangerineGroup("Clip")]
		public int InFrame
		{
			get => inFrame;
			set {
				if (inFrame != value) {
					inFrame = value;
					InTime = value * AnimationUtils.SecondsPerFrame;
					Owner?.InvalidateCache();
				}
			}
		}

		[YuzuMember]
		[TangerineGroup("Clip")]
		public bool Reversed
		{
			get => reversed;
			set {
				if (reversed != value) {
					reversed = value;
					Owner?.InvalidateCache();
				}
			}
		}

		[YuzuMember]
		[TangerineGroup("Clip")]
		public AnimationClipExtrapolation PostExtrapolation
		{
			get => postExtrapolation;
			set {
				if (postExtrapolation != value) {
					postExtrapolation = value;
					Owner?.InvalidateCache();
				}
			}
		}

#if TANGERINE
		public bool IsSelected { get; set; }
#endif

		internal Animation CachedAnimation { get; set; }

		internal Animation Animation
		{
			get {
				var node = Owner.Owner.Owner;
				node.Animations.TryFind(AnimationIdComparisonCode, out var animation);
				return animation;
			}
		}

		public int DurationInFrames
		{
			get => EndFrame - BeginFrame;
			set => EndFrame = BeginFrame + value;
		}

		public AnimationTrack Owner { get; internal set; }

		public double ToLocalTime(double time)
		{
			if (time == cachedTime) {
				return cachedLocalTime;
			}
			cachedTime = time;
			var localTime = time - BeginTime - InTime;
			switch (PostExtrapolation) {
				case AnimationClipExtrapolation.Hold:
				case AnimationClipExtrapolation.None:
					localTime = Mathf.Clamp(localTime, 0, DurationInSeconds);
					break;
				case AnimationClipExtrapolation.Repeat:
					localTime %= CachedAnimation.DurationInSeconds;
					break;
				case AnimationClipExtrapolation.PingPong:
					var period = CachedAnimation.DurationInSeconds * 2;
					localTime %= period;
					if (localTime > period * 0.5) {
						localTime = period - localTime;
					}
					break;
			}
			if (!Reversed) {
				cachedLocalTime = localTime;
			} else {
				cachedLocalTime = DurationInSeconds - AnimationUtils.SecondsPerFrame - localTime;
			}
			return cachedLocalTime;
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

		public void Insert(int index, AnimationClip item)
		{
			if (item.Owner != null) {
				throw new InvalidOperationException();
			}
			clips.Insert(index, item);
			item.Owner = owner;
			owner?.InvalidateCache();
		}

		public bool Remove(AnimationClip clip)
		{
			int index = GetIndexByFrame(clip.BeginFrame);
			if (index < 0) {
				return false;
			}
			if (clip.Owner != owner) {
				throw new InvalidOperationException();
			}
			clip.Owner = null;
			clips.RemoveAt(index);
			owner?.InvalidateCache();
			return true;
		}

		public void RemoveAt(int index)
		{
			clips[index].Owner = null;
			clips.RemoveAt(index);
			owner?.InvalidateCache();
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
				if (clips[m].BeginFrame < frame) {
					l = m + 1;
				} else if (clips[m].BeginFrame > frame) {
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
			owner?.InvalidateCache();
		}

		public void AddOrdered(AnimationClip clip)
		{
			if (clip.Owner != null) {
				throw new InvalidOperationException();
			}
			clip.Owner = owner;
			if (Count == 0 || clip.BeginFrame > this[Count - 1].BeginFrame) {
				clips.Add(clip);
			} else {
				int i = 0;
				while (clips[i].BeginFrame < clip.BeginFrame) {
					i++;
				}
				if (clips[i].BeginFrame == clip.BeginFrame) {
					clips[i] = clip;
				} else {
					clips.Insert(i, clip);
				}
			}
			owner?.InvalidateCache();
		}
	}

}
