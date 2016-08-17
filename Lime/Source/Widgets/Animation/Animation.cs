using System;
using Yuzu;

namespace Lime
{
	public class Animation : ICloneable
	{
		internal int TimeInternal;

		[YuzuMember]
		public MarkerCollection Markers { get; private set; }

		[YuzuMember]
		public string Id;

		public int Time
		{
			get { return TimeInternal; }
			set
			{
				TimeInternal = value;
				AnimationEngine.ApplyAnimators(this, false);
			}
		}

		public int Frame
		{
			get { return AnimationUtils.MsecsToFrames(Time); }
			set { Time = AnimationUtils.FramesToMsecs(value); }
		}

		public bool IsRunning;

		public Node Owner;

		public event Action Stopped;

		public AnimationEngine AnimationEngine = DefaultAnimationEngine.Instance;

		public string RunningMarkerId { get; set; }

		public Animation()
		{
			Markers = new MarkerCollection();
		}

		public void Advance(Node owner, float delta)
		{
			if (!IsRunning) {
				return;
			}
			AnimationEngine.AdvanceAnimation(this, delta);
		}

		public void Run(string markerId = null)
		{
			if (!TryRun(markerId)) {
				throw new Lime.Exception("Unknown marker '{0}'", markerId);
			}
		}

		public bool TryRun(string markerId = null)
		{
			if (AnimationEngine.TryRunAnimation(this, markerId)) {
				Stopped = null;
				return true;
			}
			return false;
		}

		public void ApplyAnimators(Node owner, bool invokeTriggers)
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
			clone.Markers = MarkerCollection.DeepClone(Markers);
			return clone;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}
