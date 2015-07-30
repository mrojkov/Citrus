using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class AnimationEngine
	{
		public virtual bool TryRunAnimation(Node node, string markerId) { return false; }
		public virtual void AdvanceAnimation(Node node, float delta) { }
		public virtual void ApplyAnimators(Node node, bool invokeTriggers) { }
	}

	public class DefaultAnimationEngine : AnimationEngine
	{
		public static readonly DefaultAnimationEngine Instance = new DefaultAnimationEngine();

		public override bool TryRunAnimation(Node node, string markerId)
		{
			Marker marker = node.Markers.TryFind(markerId);
			if (marker == null) {
				return false;
			}
			node.AnimationFrame = marker.Frame;
			node.CurrentAnimation = markerId;
			node.IsRunning = true;
			return true;
		}

		public override void AdvanceAnimation(Node node, float delta)
		{
			int deltaMs = (int)(delta * 1000 + 0.5f);
			while (deltaMs > AnimationUtils.MsecsPerFrame) {
				AdvanceAnimationShort(node, AnimationUtils.MsecsPerFrame);
				deltaMs -= AnimationUtils.MsecsPerFrame;
			}
			AdvanceAnimationShort(node, deltaMs);
		}

		private void AdvanceAnimationShort(Node node, int delta)
		{
			if (node.IsRunning) {
				int prevFrame = AnimationUtils.MsecsToFrames(node.AnimationTime - 1);
				int currFrame = AnimationUtils.MsecsToFrames(node.AnimationTime + delta - 1);
				node.animationTime += delta;
				if (prevFrame != currFrame && node.Markers.Count > 0) {
					Marker marker = node.Markers.GetByFrame(currFrame);
					if (marker != null) {
						ProcessMarker(node, marker, ref prevFrame, ref currFrame);
					}
				}
				bool invokeTriggers = prevFrame != currFrame;
				ApplyAnimators(node, invokeTriggers);
				if (!node.IsRunning) {
					node.OnAnimationStopped();
				}
			}
		}

		private void ProcessMarker(Node node, Marker marker, ref int prevFrame, ref int currFrame)
		{
			switch (marker.Action) {
				case MarkerAction.Jump:
					var gotoMarker = node.Markers.TryFind(marker.JumpTo);
					if (gotoMarker != null) {
						int hopFrames = gotoMarker.Frame - node.AnimationFrame;
						node.animationTime += AnimationUtils.FramesToMsecs(hopFrames);
						prevFrame += hopFrames;
						currFrame += hopFrames;
						ProcessMarker(node, gotoMarker, ref prevFrame, ref currFrame);
					}
					break;
				case MarkerAction.Stop:
					node.animationTime = AnimationUtils.FramesToMsecs(marker.Frame);
					prevFrame = currFrame - 1;
					node.IsRunning = false;
					break;
				case MarkerAction.Destroy:
					node.animationTime = AnimationUtils.FramesToMsecs(marker.Frame);
					prevFrame = currFrame - 1;
					node.IsRunning = false;
					node.Unlink();
					break;
			}
			if (marker.CustomAction != null) {
				marker.CustomAction();
			}
		}

		public override void ApplyAnimators(Node node, bool invokeTriggers)
		{
			for (var child = node.Nodes.FirstOrNull(); child != null; child = child.NextSibling) {
				var animators = child.Animators;
				animators.Apply(node.animationTime);
				if (invokeTriggers) {
					animators.InvokeTriggers(node.AnimationFrame);
				}
				if (node.PropagateAnimation) {
					child.animationTime = node.animationTime;
					child.ApplyAnimators(invokeTriggers);
				}
			}
		}
	}
}
