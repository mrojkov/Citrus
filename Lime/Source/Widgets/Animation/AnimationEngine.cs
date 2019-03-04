using System;
using System.Collections.Generic;

namespace Lime
{
	public class AnimationEngine
	{
		public virtual bool TryRunAnimation(Animation animation, string markerId, double animationTimeCorrection = 0) { return false; }
		public virtual void AdvanceAnimation(Animation animation, float delta) { }
		public virtual void CalcEffectiveAnimators(Animation animation) { }
		public virtual void ApplyEffectiveAnimators(Animation animation, bool invokeTriggers, double animationTimeCorrection = 0) { }
	}

	public class AnimationEngineDelegate : AnimationEngine
	{
		public Func<Animation, string, double, bool> OnRunAnimation;
		public Action<Animation, float> OnAdvanceAnimation;
		public Action<Animation> OnCalcEffectiveAnimators;
		public Action<Animation, bool, double> OnApplyEffectiveAnimators;

		public override bool TryRunAnimation(Animation animation, string markerId, double animationTimeCorrection = 0)
		{
			return (OnRunAnimation != null) && OnRunAnimation(animation, markerId, animationTimeCorrection);
		}

		public override void AdvanceAnimation(Animation animation, float delta)
		{
			OnAdvanceAnimation?.Invoke(animation, delta);
		}

		public override void CalcEffectiveAnimators(Animation animation)
		{
			OnCalcEffectiveAnimators?.Invoke(animation);
		}

		public override void ApplyEffectiveAnimators(Animation animation, bool invokeTriggers, double animationTimeCorrection = 0)
		{
			OnApplyEffectiveAnimators?.Invoke(animation, invokeTriggers, animationTimeCorrection);
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
				CalcEffectiveAnimators(animation);
				ApplyEffectiveAnimators(animation, invokeTriggers: false);
			} else {
				var frameTime = animation.NextMarkerOrTriggerTime.Value;
				var frameIndex = AnimationUtils.SecondsToFrames(frameTime);
				animation.NextMarkerOrTriggerTime = null;
				var currentMarker = animation.Markers.GetByFrame(frameIndex);
				if (currentMarker != null) {
					ProcessMarker(animation, currentMarker);
				}
				CalcEffectiveAnimators(animation);
				ApplyEffectiveAnimators(animation, invokeTriggers: true, animationTimeCorrection: previousTime - frameTime);
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

		public override void CalcEffectiveAnimators(Animation animation)
		{
			if (animation.IsCompound) {
				CalcEffectiveAnimatorsForCompoundAnimation(animation);
			} else {
				CalcEffectiveAnimatorsForPlainAnimation(animation);
			}
		}

		private static void CalcEffectiveAnimatorsForCompoundAnimation(Animation animation)
		{
			(animation.EffectiveAnimators ?? (animation.EffectiveAnimators = new List<IAnimator>())).Clear();
			(animation.CollisionMap ?? (animation.CollisionMap = new AnimationCollisionMap())).Clear();
			int frame = animation.Frame;
			foreach (var track in animation.Tracks) {
				foreach (var a in track.Animators) {
					a.CalcAndApply(animation.Time); // Animate track weight and so on...
				}
				foreach (var clip in track.Clips) {
					if (frame < clip.Begin || frame >= clip.End) {
						continue;
					}
					var clipAnimation = clip.Animation;
					clipAnimation.TimeInternal = clip.RemapTime(animation.Time);
					clipAnimation.AnimationEngine.CalcEffectiveAnimators(clipAnimation);
					foreach (var a in clipAnimation.EffectiveAnimators) {
						if (!animation.CollisionMap.TryGetAnimator(a, out var masterAnimator)) {
							a.Weight = track.Weight;
							animation.CollisionMap.AddAnimator(a);
							animation.EffectiveAnimators.Add(a);
						} else {
							masterAnimator.BlendWith(a, track.Weight);
						}
					}
				}
			}
		}

		private static void CalcEffectiveAnimatorsForPlainAnimation(Animation animation)
		{
			foreach (var a in GetEffectiveAnimators()) {
				a.CalcValue(animation.Time);
			}

			List<IAnimator> GetEffectiveAnimators()
			{
				if (animation.Owner.DescendantAnimatorsVersion == animation.EffectiveAnimatorsVersion && animation.EffectiveAnimators != null) {
					return animation.EffectiveAnimators;
				}
				(animation.EffectiveAnimators ?? (animation.EffectiveAnimators = new List<IAnimator>())).Clear();
				AddEffectiveAnimatorsRecursively(animation.Owner);
				animation.EffectiveAnimatorsVersion = animation.Owner.DescendantAnimatorsVersion;
				return animation.EffectiveAnimators;

				void AddEffectiveAnimatorsRecursively(Node node)
				{
					for (var child = node.FirstChild; child != null; child = child.NextSibling) {
						if (child.Animators.Count > 0) { // Optimization: Animators.GetEnumerator() creates internal storage
							foreach (var a in child.Animators) {
								if (a.AnimationId == animation.Id) {
									animation.EffectiveAnimators.Add(a);
								}
							}
						}
						if (animation.Id != null) {
							AddEffectiveAnimatorsRecursively(child);
						}
					}
				}
			}
		}

		public override void ApplyEffectiveAnimators(Animation animation, bool invokeTriggers, double animationTimeCorrection = 0)
		{
			foreach (var a in animation.EffectiveAnimators) {
				a.Apply();
				if (invokeTriggers) {
					a.InvokeTrigger(animation.Frame, animation.Id, animationTimeCorrection);
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
				CalcEffectiveAnimators(animation);
				ApplyEffectiveAnimators(animation, invokeTriggers: true, animationTimeCorrection: previousTime - frameTime);
				if (!animation.IsRunning) {
					animation.RaiseStopped();
				}
			}
		}
	}
}
