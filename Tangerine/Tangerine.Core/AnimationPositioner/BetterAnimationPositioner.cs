using System;
using System.Collections.Generic;
using System.Linq;
using Lime;

namespace Tangerine.Core
{
	class BetterAnimationPositioner : IAnimationPositioner
	{
		public bool CacheAnimationsStates { get; set; }

		public void SetAnimationFrame(Animation animation, int frame, bool animationMode, bool stopAnimations)
		{
			Audio.GloballyEnable = false;
			ResetAnimations(animation.OwnerNode);
			animation.IsRunning = true;
			animation.Time = 0;
			animation.OwnerNode.SetTangerineFlag(TangerineFlags.IgnoreMarkers, true);
			// Advance animation on Threshold more than needed to ensure the last trigger will be processed.
			AdvanceAnimation(animation.OwnerNode, AnimationUtils.FramesToSeconds(frame) + AnimationUtils.Threshold);
			// Set animation exactly on the given frame.
			animation.Frame = frame;
			animation.OwnerNode.SetTangerineFlag(TangerineFlags.IgnoreMarkers, false);
			if (stopAnimations) {
				StopAnimations(animation.OwnerNode);
			}
			Audio.GloballyEnable = true;
		}

		private void AdvanceAnimation(Node node, double delta)
		{
			var animations = node.Components.Get<AnimationComponent>()?.Animations;
			while (delta > 0) {
				var clampedDelta = delta;
				if (animations != null) {
					// Clamp delta to make sure we aren't going to skip any marker or trigger.
					foreach (var animation in animations) {
						if (animation.IsRunning) {
							if (FindClosestFrameWithMarkerOrTrigger(animation, out var frame)) {
								clampedDelta = Math.Min(clampedDelta, CalcDelta(animation.Time, AnimationUtils.FramesToSeconds(frame)));
							}
						}
					}
					foreach (var animation in animations) {
						if (animation.IsRunning) {
							animation.AnimationEngine.AdvanceAnimation(animation, clampedDelta);
						}
					}
				}
				foreach (var child in node.Nodes) {
					AdvanceAnimation(child, clampedDelta);
				}
				delta -= clampedDelta;
			}
		}

		private static double CalcDelta(double currentTime, double triggerTime)
		{
			if (triggerTime - currentTime > AnimationUtils.SecondsPerFrame - AnimationUtils.Threshold) {
				return triggerTime - currentTime - AnimationUtils.Threshold;
			} else {
				return triggerTime - currentTime + AnimationUtils.Threshold;
			}
		}

		private bool FindClosestFrameWithMarkerOrTrigger(Animation animation, out int frame)
		{
			var animationFrame = AnimationUtils.SecondsToFramesCeiling(animation.Time);
			frame = int.MaxValue;
			if (animation.Markers.Count > 0) {
				foreach (var marker in animation.Markers) {
					if (marker.Frame >= animationFrame) {
						frame = marker.Frame;
						break;
					}
				}
			}
			foreach (var abstractAnimator in animation.EffectiveTriggerableAnimators) {
				if (abstractAnimator is IAnimator animator) {
					foreach (var k in animator.ReadonlyKeys) {
						if (k.Frame >= animationFrame) {
							if (k.Frame < frame) {
								frame = k.Frame;
							}
							break;
						}
					}
				}
			}
			return frame != int.MaxValue;
		}

		private void ResetAnimations(Node node)
		{
			foreach (var n in node.Nodes) {
				ResetAnimations(n);
			}
		}

		private void StopAnimations(Node node)
		{
			foreach (var animation in node.Animations) {
				animation.IsRunning = false;
			}
			foreach (var n in node.Nodes) {
				StopAnimations(n);
			}
		}
	}
}
