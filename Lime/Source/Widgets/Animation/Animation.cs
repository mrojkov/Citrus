using System;
using Yuzu;

namespace Lime
{
	public class Animation : ICloneable
	{
		private Node owner;
		private bool isRunning;
		internal Animation Next;
		internal double TimeInternal;
		internal double? NextMarkerOrTriggerTime;
		public event Action Stopped;
		public AnimationEngine AnimationEngine = DefaultAnimationEngine.Instance;
		public string RunningMarkerId { get; set; }

		[YuzuMember]
		public MarkerList Markers { get; private set; }

		[YuzuMember]
		public string Id;

		public double Time
		{
			get { return TimeInternal; }
			set
			{
				TimeInternal = value;
				NextMarkerOrTriggerTime = null;
				AnimationEngine.ApplyAnimators(this, false);
			}
		}

		public int Frame
		{
			get { return AnimationUtils.SecondsToFrames(Time); }
			set { Time = AnimationUtils.FramesToSeconds(value); }
		}

		public Node Owner
		{
			get { return owner; }
			internal set
			{
				if (value == null) {
					IsRunning = false;
				}
				owner = value;
			}
		}

		public bool IsRunning
		{
			get { return isRunning; }
			set
			{
				if (isRunning != value) {
					isRunning = value;
					if (value) {
						Owner.RunningAnimationsCount++;
					} else {
						Owner.RunningAnimationsCount--;
					}
				}
			}
		}

		public Animation()
		{
			Markers = new MarkerList(this);
		}

		public void Advance(float delta)
		{
			if (IsRunning) {
				AnimationEngine.AdvanceAnimation(this, delta);
			}
		}

		public void Run(string markerId = null)
		{
			if (!TryRun(markerId)) {
				throw new Lime.Exception("Unknown marker '{0}'", markerId);
			}
		}

		public bool TryRun(string markerId = null, double animationTimeCorrection = 0)
		{
			if (AnimationEngine.TryRunAnimation(this, markerId, animationTimeCorrection)) {
				Stopped = null;
				return true;
			}
			return false;
		}

		public void ApplyAnimators(bool invokeTriggers)
		{
			AnimationEngine.ApplyAnimators(this, invokeTriggers);
		}

		internal void OnStopped()
		{
			if (Stopped != null) {
				Stopped();
			}
		}

		public Animation Clone()
		{
			var clone = (Animation)MemberwiseClone();
			clone.Owner = null;
			clone.Next = null;
			clone.Markers = MarkerCollection.DeepClone(Markers, clone);
			return clone;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}
