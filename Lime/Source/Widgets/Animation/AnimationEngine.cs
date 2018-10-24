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
		public static DefaultAnimationEngine Instance = new DefaultAnimationEngine();

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
			var previousTime = animation.TimeInternal;
			var currentTime = previousTime + delta;
			animation.TimeInternal = currentTime;
			if (!animation.NextMarkerOrTriggerTime.HasValue) {
				var frameIndex = AnimationUtils.SecondsToFrames(currentTime);
				var frameTime = AnimationUtils.SecondsPerFrame * frameIndex;
				var stepOverFrame = previousTime <= frameTime && frameTime <= currentTime;
				animation.NextMarkerOrTriggerTime = GetNextMarkerOrTriggerTime(animation, frameIndex + (stepOverFrame ? 0 : 1));
			}

			if (!animation.NextMarkerOrTriggerTime.HasValue || currentTime <= animation.NextMarkerOrTriggerTime.Value) {
				ApplyAnimators(animation, invokeTriggers: false);
			} else {
				var frameTime = animation.NextMarkerOrTriggerTime.Value;
				var frameIndex = AnimationUtils.SecondsToFrames(frameTime);
				animation.NextMarkerOrTriggerTime = null;
				var currentMarker = animation.Markers.GetByFrame(frameIndex);
				if (currentMarker != null) {
					ProcessMarker(animation, currentMarker);
				}
				ApplyAnimators(animation, invokeTriggers: true, animationTimeCorrection: previousTime - frameTime);
				if (!animation.IsRunning) {
					animation.RaiseStopped();
				}
			}
		}

		protected static double? GetNextMarkerOrTriggerTime(Animation animation, int nextFrame)
		{
			int? nextMarkerOrTriggerFrame = null;
			foreach (var marker in animation.Markers) {
				if (marker.Frame >= nextFrame) {
					nextMarkerOrTriggerFrame = marker.Frame;
					break;
				}
			}
			for (var child = animation.Owner.FirstChild; child != null; child = child.NextSibling) {
				foreach (var animator in child.Animators) {
					if (!animator.Enabled || !animator.IsTriggerable || animator.AnimationId != animation.Id) {
						continue;
					}

					if (animator.TryGetNextKeyFrame(nextFrame, out var keyFrame)) {
						if (!nextMarkerOrTriggerFrame.HasValue || keyFrame < nextMarkerOrTriggerFrame.Value) {
							nextMarkerOrTriggerFrame = keyFrame;
						}
					}
				}
			}
			return nextMarkerOrTriggerFrame * AnimationUtils.SecondsPerFrame;
		}

		protected virtual void ProcessMarker(Animation animation, Marker marker)
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
						AdvanceAnimation(animation, (float)delta);
					}
					break;
				case MarkerAction.Stop:
					animation.TimeInternal = AnimationUtils.FramesToSeconds(marker.Frame);
					animation.IsRunning = false;
					break;
			}
			marker.CustomAction?.Invoke();
		}

		public override void ApplyAnimators(Animation animation, bool invokeTriggers, double animationTimeCorrection = 0)
		{
			ApplyAnimators(animation.Owner, animation, invokeTriggers, animationTimeCorrection);
		}

		private static void ApplyAnimators(Node node, Animation animation, bool invokeTriggers, double animationTimeCorrection = 0)
		{
			for (var child = node.FirstChild; child != null; child = child.NextSibling) {
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

	public class FastForwardAnimationEngine : DefaultAnimationEngine
	{
		public override void AdvanceAnimation(Animation animation, float delta)
		{
			var previousTime = animation.TimeInternal;
			var currentTime = previousTime + delta;
			animation.TimeInternal = currentTime;
			if (!animation.NextMarkerOrTriggerTime.HasValue) {
				var frameIndex = AnimationUtils.SecondsToFrames(currentTime);
				var frameTime = AnimationUtils.SecondsPerFrame * frameIndex;
				var stepOverFrame = previousTime <= frameTime && frameTime <= currentTime;
				animation.NextMarkerOrTriggerTime = GetNextMarkerOrTriggerTime(animation, frameIndex + (stepOverFrame ? 0 : 1));
			}

			if (!animation.NextMarkerOrTriggerTime.HasValue || currentTime <= animation.NextMarkerOrTriggerTime.Value) {
				// do nothing
			} else {
				var frameTime = animation.NextMarkerOrTriggerTime.Value;
				var frameIndex = AnimationUtils.SecondsToFrames(frameTime);
				animation.NextMarkerOrTriggerTime = null;
				var currentMarker = animation.Markers.GetByFrame(frameIndex);
				if (currentMarker != null) {
					ProcessMarker(animation, currentMarker);
				}
				ApplyAnimators(animation, invokeTriggers: true, animationTimeCorrection: previousTime - frameTime);
				if (!animation.IsRunning) {
					animation.RaiseStopped();
				}
			}
		}
	}
}
