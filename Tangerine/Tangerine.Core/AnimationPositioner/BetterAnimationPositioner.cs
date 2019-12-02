using System;
using System.Collections.Generic;
using System.Linq;
using Lime;

namespace Tangerine.Core
{
	class BetterAnimationPositioner : IAnimationPositioner
	{
		public bool CacheAnimationsStates { get; set; }

		public void SetAnimationFrame(Animation animation, int frame, bool animationMode)
		{
			Audio.GloballyEnable = false;
			ResetAnimations(animation.OwnerNode);
			animation.IsRunning = true;
			animation.OwnerNode.SetTangerineFlag(TangerineFlags.IgnoreMarkers, true);
			AdvanceAnimation(animation.OwnerNode, AnimationUtils.FramesToSeconds(frame));
			animation.OwnerNode.SetTangerineFlag(TangerineFlags.IgnoreMarkers, false);
			StopAnimations(animation.OwnerNode);
			Audio.GloballyEnable = true;
		}

		private void AdvanceAnimation(Node node, double delta)
		{
			var animations = node.Components.AnimationComponent?.Animations;
			while (delta > 0) {
				var clampedDelta = delta;
				if (animations != null) {
					// Clamp delta to make sure we aren't going to skip any marker or trigger.
					foreach (var animation in animations) {
						if (animation.IsRunning) {
							var markerAhead = FindMarkerAhead(animation, animation.Time);
							if (markerAhead != null) {
								clampedDelta = Math.Min(markerAhead.Time - animation.Time + AnimationUtils.Threshold, clampedDelta);
							}
							var triggerAhead = FindTriggerAhead(animation, animation.Time);
							if (triggerAhead != null) {
								clampedDelta = Math.Min(
									AnimationUtils.FramesToSeconds(triggerAhead.Frame) - animation.Time + AnimationUtils.Threshold, clampedDelta);
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

		private static Marker FindMarkerAhead(Animation animation, double time)
		{
			if (animation.Markers.Count > 0) {
				var frame = AnimationUtils.SecondsToFramesCeiling(time);
				foreach (var marker in animation.Markers) {
					if (marker.Frame >= frame) {
						return marker;
					}
				}
			}
			return null;
		}

		private IKeyframe FindTriggerAhead(Animation animation, double time)
		{
			foreach (var abstractAnimator in animation.EffectiveTriggerableAnimators) {
				if (abstractAnimator is IAnimator animator) {
					var frame = AnimationUtils.SecondsToFramesCeiling(time);
					foreach (var k in animator.ReadonlyKeys) {
						if (k.Frame >= frame) {
							return k;
						}
					}
				}
			}
			return null;
		}


		private void ResetAnimations(Node node)
		{
			foreach (var animation in node.Animations) {
				animation.Time = 0;
			}
			foreach (var n in node.Nodes) {
				ResetAnimations(n);
			}
		}

		private void StopAnimations(Node node)
		{
			foreach (var animation in node.Animations) {
				animation.IsRunning = false;
				animation.ApplyAnimators();
			}
			foreach (var n in node.Nodes) {
				StopAnimations(n);
			}
		}
	}
}
