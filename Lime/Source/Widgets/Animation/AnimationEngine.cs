using System;

namespace Lime
{
	public class AnimationEngine
	{
		public virtual bool TryRunAnimation(Animation animation, string markerId, double animationTimeCorrection = 0) { return false; }
		public virtual void AdvanceAnimation(Animation animation, float delta) { }
		public virtual void ApplyAnimators(Animation animation, bool invokeTriggers, double animationTimeCorrection = 0) { }
	}

	public class AnimationEngineDelegate : AnimationEngine
	{
		public Func<Animation, string, double, bool> OnRunAnimation;
		public Action<Animation, float> OnAdvanceAnimation;
		public Action<Animation, bool, double> OnApplyAnimators;

		public override bool TryRunAnimation(Animation animation, string markerId, double animationTimeCorrection = 0)
		{
			return (OnRunAnimation != null) && OnRunAnimation(animation, markerId, animationTimeCorrection);
		}

		public override void AdvanceAnimation(Animation animation, float delta)
		{
			if (OnAdvanceAnimation != null) {
				OnAdvanceAnimation(animation, delta);
			}
		}

		public override void ApplyAnimators(Animation animation, bool invokeTriggers, double animationTimeCorrection = 0)
		{
			if (OnApplyAnimators != null) {
				OnApplyAnimators(animation, invokeTriggers, animationTimeCorrection);
			}
		}
	}

	public class DefaultAnimationEngine : AnimationEngine
	{
		public static readonly DefaultAnimationEngine Instance = new DefaultAnimationEngine();

		public override bool TryRunAnimation(Animation animation, string markerId, double animationTimeCorrection = 0)
		{
			var frame = 0;
			if (markerId != null) {
				var marker = animation.Markers.TryFind(markerId);
				if (marker == null) {
					return false;
				}
				frame = marker.Frame;
			}
			animation.Time = AnimationUtils.FramesToSeconds(frame) + animationTimeCorrection;
			animation.RunningMarkerId = markerId;
			animation.IsRunning = true;
			return true;
		}

		public override void AdvanceAnimation(Animation animation, float delta)
		{
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
			if (!animation.IsRunning) {
				return;
			}
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
				ApplyAnimators(animation, stepOverFrame, previousTime - frameTime);
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

		public override void ApplyAnimators(Animation animation, bool invokeTriggers, double animationTimeCorrection = 0)
		{
			ApplyAnimators(animation.Owner, animation, invokeTriggers, animationTimeCorrection);
		}

		private static void ApplyAnimators(Node node, Animation animation, bool invokeTriggers, double animationTimeCorrection = 0)
		{
			for (var child = node.Nodes.FirstOrNull(); child != null; child = child.NextSibling) {
				var animators = child.Animators;
				animators.Apply(animation.Time, animation.Id);
				if (invokeTriggers) {
					animators.InvokeTriggers(animation.Frame, animationTimeCorrection);
				}
				if (animation.Id != null) {
					ApplyAnimators(child, animation, invokeTriggers);
				}
			}
		}
	}
}
