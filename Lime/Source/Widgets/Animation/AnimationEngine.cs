using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class AnimationEngine
	{
		public virtual bool TryRunAnimation(Animation animation, string markerId) { return false; }
		public virtual void AdvanceAnimation(Animation animation, float delta) { }
		public virtual void ApplyAnimators(Animation animation, bool invokeTriggers) { }
	}

	public class AnimationEngineDelegate : AnimationEngine
	{
		public Func<Animation, string, bool> OnRunAnimation;
		public Action<Animation, float> OnAdvanceAnimation;
		public Action<Animation, bool> OnApplyAnimators;

		public override bool TryRunAnimation(Animation animation, string markerId)
		{
			return (OnRunAnimation != null) && OnRunAnimation(animation, markerId);
		}

		public override void AdvanceAnimation(Animation animation, float delta)
		{
			if (OnAdvanceAnimation != null) {
				OnAdvanceAnimation(animation, delta);
			}
		}

		public override void ApplyAnimators(Animation animation, bool invokeTriggers)
		{
			if (OnApplyAnimators != null) {
				OnApplyAnimators(animation, invokeTriggers);
			}
		}
	}

	public class DefaultAnimationEngine : AnimationEngine
	{
		public static readonly DefaultAnimationEngine Instance = new DefaultAnimationEngine();

		public override bool TryRunAnimation(Animation animation, string markerId)
		{
			var frame = 0;
			if (markerId != null) {
				var marker = animation.Markers.TryFind(markerId);
				if (marker == null) {
					return false;
				}
				frame = marker.Frame;
			}
			animation.Frame = frame;
			animation.RunningMarkerId = markerId;
			animation.IsRunning = true;
			return true;
		}

		public override void AdvanceAnimation(Animation animation, float delta)
		{
			if (!animation.IsRunning) {
				return;
			}
			double remainedDelta = delta;
			// Set the delta limit to ensure we process no more than one frame at a time.
			const double deltaLimit = AnimationUtils.SecondsPerFrame * 0.99;
			while (remainedDelta > deltaLimit) {
				AdvanceAnimationHelper(animation, deltaLimit, applyAnimators: false);
				remainedDelta -= deltaLimit;
			}
			AdvanceAnimationHelper(animation, remainedDelta, applyAnimators: true);
		}

		private void AdvanceAnimationHelper(Animation animation, double delta, bool applyAnimators)
		{
			var previousTime = animation.TimeInternal;
			var currentTime = previousTime + delta;
			animation.TimeInternal = currentTime;
			var frameIndex = AnimationUtils.SecondsToFrames(currentTime);
			var frameTime = AnimationUtils.SecondsPerFrame * frameIndex;
			var stepOverFrame = previousTime <= frameTime && frameTime < currentTime;
			if (stepOverFrame && animation.Markers.Count > 0) {
				var marker = animation.Markers.GetByFrame(frameIndex);
				if (marker != null) {
					ProcessMarker(animation, marker);
				}
			}
			if (applyAnimators || stepOverFrame) {
				ApplyAnimators(animation, invokeTriggers: stepOverFrame);
				if (!animation.IsRunning) {
					animation.OnStopped();
				}
			}
		}

		private void ProcessMarker(Animation animation, Marker marker)
		{
			if ((animation.Owner.TangerineFlags & TangerineFlags.IgnoreMarkers) != 0) {
				return;
			}
			switch (marker.Action) {
				case MarkerAction.Jump:
					var gotoMarker = animation.Markers.TryFind(marker.JumpTo);
					if (gotoMarker != null && gotoMarker != marker) {
						var delta = animation.Time - AnimationUtils.FramesToSeconds(animation.Frame);
						animation.TimeInternal = gotoMarker.Time;
						AdvanceAnimationHelper(animation, delta, applyAnimators: true); 
					}
					break;
				case MarkerAction.Stop:
					animation.TimeInternal = AnimationUtils.FramesToSeconds(marker.Frame);
					animation.IsRunning = false;
					break;
				case MarkerAction.Destroy:
					animation.TimeInternal = AnimationUtils.FramesToSeconds(marker.Frame);
					animation.IsRunning = false;
					animation.Owner.Unlink();
					break;
			}
			if (marker.CustomAction != null) {
				marker.CustomAction();
			}
		}

		public override void ApplyAnimators(Animation animation, bool invokeTriggers)
		{
			ApplyAnimators(animation.Owner, animation, invokeTriggers);
		}

		private static void ApplyAnimators(Node node, Animation animation, bool invokeTriggers)
		{
			for (var child = node.Nodes.FirstOrNull(); child != null; child = child.NextSibling) {
				var animators = child.Animators;
				animators.Apply(animation.Time, animation.Id);
				if (invokeTriggers) {
					animators.InvokeTriggers(animation.Frame);
				}
				if (animation.Id != null) {
					ApplyAnimators(child, animation, invokeTriggers);
				}
			}
		}
	}
}
