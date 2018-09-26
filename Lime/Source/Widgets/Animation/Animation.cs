using System;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	public class Animation : ICloneable
	{
		private bool isRunning;
		internal Animation Next;
		internal double TimeInternal;
		internal double? NextMarkerOrTriggerTime;
		public event Action Stopped;
		public AnimationEngine AnimationEngine = DefaultAnimationEngine.Instance;
		public string RunningMarkerId { get; set; }

		[YuzuMember]
		[TangerineIgnore]
		public MarkerList Markers { get; private set; }

		[YuzuMember]
		public string Id { get; set; }

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

		public Node Owner { get; internal set; }

		public bool IsRunning
		{
			get { return isRunning; }
			set
			{
				if (isRunning != value) {
					isRunning = value;
					Owner?.RefreshRunningAnimationCount();
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

		internal void RaiseStopped()
		{
			Stopped?.Invoke();
		}

		public Animation Clone()
		{
			var clone = (Animation)MemberwiseClone();
			clone.Owner = null;
			clone.Next = null;
			clone.Markers = MarkerList.DeepClone(Markers, clone);
			return clone;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}
