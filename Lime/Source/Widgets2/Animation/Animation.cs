using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Lime.Widgets2
{
	[ProtoContract]
	public class Animation : Behaviour
	{
		public event Action Stopped;

		[ProtoMember(1)]
		public bool Propagate { get; set; }

		public string Current { get; set; }

		[ProtoMember(2)]
		public float Speed { get; set; }

		[ProtoMember(4)]
		public MarkerCollection Markers;

		[ProtoMember(5)]
		public bool IsRunning { get; set; }
		public bool IsStopped
		{
			get { return !IsRunning; }
			set { IsRunning = !value; }
		}

		[Trigger]
		[TangerineProperty(6)]
		public string Trigger { get; set; }

		private int time;
		[ProtoMember(7)]
		public int Time
		{
			get { return time; }
			set
			{
				time = value;
				ApplyAnimators(invokeTriggers: false);
			}
		}

		public Animation()
		{
			Speed = 1;
			Animators = new AnimatorCollection(this);
			Markers = new MarkerCollection();
		}

		public override void Update(float delta)
		{
			if (IsRunning) {
				Advance(delta * Speed);
			}
		}

		public override Component Clone()
		{
			var clone = (Animation)base.Clone();
			clone.Markers = MarkerCollection.DeepClone(Markers);
			return clone;
		}

		public bool TryRun(string markerId)
		{
			Marker marker = Markers.TryFind(markerId);
			if (marker == null) {
				return false;
			}
			Stopped = null;
			Frame = marker.Frame;
			Current = markerId;
			IsRunning = true;
			return true;
		}

		internal protected override void OnTrigger(string property)
		{
			if (property != "Trigger") {
				return;
			}
			if (String.IsNullOrEmpty(Trigger)) {
				Time = 0;
				IsRunning = true;
			} else {
				TryRun(Trigger);
			}
		}

		public void Run(string markerId)
		{
			if (!TryRun(markerId)) {
				throw new Lime.Exception("Unknown animation '{0}' in node '{1}'", markerId, this.ToString());
			}
		}

		public int Frame
		{
			get { return AnimationUtils.MsecsToFrames(Time); }
			set { Time = AnimationUtils.FramesToMsecs(value); }
		}

		public void Advance(float delta)
		{
			int deltaMs = (int)(delta * 1000 + 0.5f);
			while (deltaMs > AnimationUtils.MsecsPerFrame) {
				AdvanceShort(AnimationUtils.MsecsPerFrame);
				deltaMs -= AnimationUtils.MsecsPerFrame;
			}
			AdvanceShort(deltaMs);
		}

		private void AdvanceShort(int delta)
		{
			if (IsRunning) {
				int prevFrame = AnimationUtils.MsecsToFrames(time - 1);
				int currFrame = AnimationUtils.MsecsToFrames(time + delta - 1);
				time += delta;
				if (prevFrame != currFrame && Markers.Count > 0) {
					Marker marker = Markers.GetByFrame(currFrame);
					if (marker != null) {
						ProcessMarker(marker, ref prevFrame, ref currFrame);
					}
				}
				bool invokeTriggers = prevFrame != currFrame;
				ApplyAnimators(invokeTriggers);
				if (!IsRunning) {
					OnStopped();
				}
			}
		}

		protected virtual void OnStopped()
		{
			if (Stopped != null) {
				Stopped();
			}
		}

		private void ProcessMarker(Marker marker, ref int prevFrame, ref int currFrame)
		{
			switch (marker.Action) {
				case MarkerAction.Jump:
					var gotoMarker = Markers.TryFind(marker.JumpTo);
					if (gotoMarker != null) {
						int hopFrames = gotoMarker.Frame - Frame;
						time += AnimationUtils.FramesToMsecs(hopFrames);
						prevFrame += hopFrames;
						currFrame += hopFrames;
						ProcessMarker(gotoMarker, ref prevFrame, ref currFrame);
					}
					break;
				case MarkerAction.Stop:
					time = AnimationUtils.FramesToMsecs(marker.Frame);
					prevFrame = currFrame - 1;
					IsRunning = false;
					break;
				case MarkerAction.Destroy:
					time = AnimationUtils.FramesToMsecs(marker.Frame);
					prevFrame = currFrame - 1;
					IsRunning = false;
					Owner.Unlink();
					break;
			}
			if (marker.CustomAction != null) {
				marker.CustomAction();
			}
		}

		private void ApplyAnimators(bool invokeTriggers)
		{
			for (var node = Owner.Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
				foreach (var component in Owner.Components) {
					var animators = component.Animators;
					animators.Apply(time);
					if (invokeTriggers) {
						animators.InvokeTriggers(Frame);
					}
				}
				if (Propagate) {
					node.Animation.time = time;
					node.Animation.ApplyAnimators(invokeTriggers);
				}
			}
		}
	}
}
