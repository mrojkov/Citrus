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
			var deltaMs = (int)(delta * 1000 + 0.5f);
			while (deltaMs > AnimationUtils.MsecsPerFrame) {
				AdvanceAnimationShort(animation, AnimationUtils.MsecsPerFrame);
				deltaMs -= AnimationUtils.MsecsPerFrame;
			}
			AdvanceAnimationShort(animation, deltaMs);
		}

		private void AdvanceAnimationShort(Animation animation, int delta)
		{
			if (animation.IsRunning) {
				var prevFrame = AnimationUtils.MsecsToFrames(animation.Time - 1);
				var currFrame = AnimationUtils.MsecsToFrames(animation.Time + delta - 1);
				animation.TimeInternal += delta;
				if (prevFrame != currFrame && animation.Markers.Count > 0) {
					var marker = animation.Markers.GetByFrame(currFrame);
					if (marker != null) {
						ProcessMarker(animation, marker, ref prevFrame, ref currFrame);
					}
				}
				var invokeTriggers = prevFrame != currFrame;
				ApplyAnimators(animation, invokeTriggers);
				if (!animation.IsRunning) {
					animation.OnStopped();
				}
			}
		}

		private void ProcessMarker(Animation animation, Marker marker, ref int prevFrame, ref int currFrame)
		{
			switch (marker.Action) {
				case MarkerAction.Jump:
					var gotoMarker = animation.Markers.TryFind(marker.JumpTo);
					if (gotoMarker != null) {
						var hopFrames = gotoMarker.Frame - animation.Frame;
						animation.TimeInternal += AnimationUtils.FramesToMsecs(hopFrames);
						prevFrame += hopFrames;
						currFrame += hopFrames;
						ProcessMarker(animation, gotoMarker, ref prevFrame, ref currFrame);
					}
					break;
				case MarkerAction.Stop:
					animation.TimeInternal = AnimationUtils.FramesToMsecs(marker.Frame);
					prevFrame = currFrame - 1;
					animation.IsRunning = false;
					break;
				case MarkerAction.Destroy:
					animation.TimeInternal = AnimationUtils.FramesToMsecs(marker.Frame);
					prevFrame = currFrame - 1;
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
