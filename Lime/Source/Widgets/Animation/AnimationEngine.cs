using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class AnimationEngine
	{
		public virtual bool TryRunAnimation(Animation animation, string markerId) { return false; }
		public virtual void AdvanceAnimation(Node node, Animation animation, float delta) { }
		public virtual void ApplyAnimators(Node node, Animation animation, bool invokeTriggers) { }
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

		public override void AdvanceAnimation(Node node, Animation animation, float delta)
		{
			var deltaMs = (int)(delta * 1000 + 0.5f);
			while (deltaMs > AnimationUtils.MsecsPerFrame) {
				AdvanceAnimationShort(node, animation, AnimationUtils.MsecsPerFrame);
				deltaMs -= AnimationUtils.MsecsPerFrame;
			}
			AdvanceAnimationShort(node, animation, deltaMs);
		}

		private void AdvanceAnimationShort(Node node, Animation animation, int delta)
		{
			if (animation.IsRunning) {
				var prevFrame = AnimationUtils.MsecsToFrames(animation.Time - 1);
				var currFrame = AnimationUtils.MsecsToFrames(animation.Time + delta - 1);
				animation.Time += delta;
				if (prevFrame != currFrame && animation.Markers.Count > 0) {
					var marker = animation.Markers.GetByFrame(currFrame);
					if (marker != null) {
						ProcessMarker(node, animation, marker, ref prevFrame, ref currFrame);
					}
				}
				var invokeTriggers = prevFrame != currFrame;
				ApplyAnimators(node, animation, invokeTriggers);
				if (!animation.IsRunning) {
					animation.OnStopped();
				}
			}
		}

		private void ProcessMarker(Node node, Animation animation, Marker marker, ref int prevFrame, ref int currFrame)
		{
			switch (marker.Action) {
				case MarkerAction.Jump:
					var gotoMarker = animation.Markers.TryFind(marker.JumpTo);
					if (gotoMarker != null) {
						var hopFrames = gotoMarker.Frame - animation.Frame;
						animation.Time += AnimationUtils.FramesToMsecs(hopFrames);
						prevFrame += hopFrames;
						currFrame += hopFrames;
						ProcessMarker(node, animation, gotoMarker, ref prevFrame, ref currFrame);
					}
					break;
				case MarkerAction.Stop:
					animation.Time = AnimationUtils.FramesToMsecs(marker.Frame);
					prevFrame = currFrame - 1;
					animation.IsRunning = false;
					break;
				case MarkerAction.Destroy:
					animation.Time = AnimationUtils.FramesToMsecs(marker.Frame);
					prevFrame = currFrame - 1;
					animation.IsRunning = false;
					node.Unlink();
					break;
			}
			if (marker.CustomAction != null) {
				marker.CustomAction();
			}
		}

		public override void ApplyAnimators(Node node, Animation animation, bool invokeTriggers)
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
