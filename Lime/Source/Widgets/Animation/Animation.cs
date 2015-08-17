using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class Animation : ICloneable
	{
		[ProtoMember(1)]
		public MarkerCollection Markers { get; private set; }

		[ProtoMember(2)]
		public string Id;

		[ProtoMember(3)]
		public int Time;

		public int Frame
		{
			get { return AnimationUtils.MsecsToFrames(Time); }
			set { Time = AnimationUtils.FramesToMsecs(value); }
		}

		[ProtoMember(4)]
		public bool IsRunning;

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
			AnimationEngine.AdvanceAnimation(owner, this, delta);
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
			AnimationEngine.ApplyAnimators(owner, this, invokeTriggers);
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
			clone.Markers = MarkerCollection.DeepClone(Markers);
			return clone;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}
