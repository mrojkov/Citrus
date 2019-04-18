using System;
using System.Collections.Generic;

namespace Lime
{
	public class AnimationEngine
	{
		public virtual bool TryRunAnimation(Animation animation, string markerId, double animationTimeCorrection = 0) { return false; }
		public virtual void AdvanceAnimation(Animation animation, float delta) { }
		/// <summary>
		/// 1. Refreshes animation.EffectiveAnimators;
		/// 2. Applies each animator at currentTime;
		/// 3. Executes triggers in given range.
		/// The range is [previousTime, currentTime) or [previousTime, currentTime] depending on executeTriggersAtCurrentTime flag.
		/// This method doesn't depend on animation.Time value.
		/// </summary>
		public virtual void ApplyAnimatorsAndExecuteTriggers(Animation animation, double previousTime, double currentTime, bool executeTriggersAtCurrentTime) { }
		public virtual bool AreEffectiveAnimatorsValid(Animation animation) => false;
		public virtual void BuildEffectiveAnimators(Animation animation) { }
	}

	public class AnimationEngineDelegate : AnimationEngine
	{
		public Func<Animation, string, double, bool> OnRunAnimation;
		public Action<Animation, float> OnAdvanceAnimation;
		public Action<Animation, double, double, bool> OnApplyEffectiveAnimatorsAndBuildTriggersList;

		public override bool TryRunAnimation(Animation animation, string markerId, double animationTimeCorrection = 0)
		{
			return (OnRunAnimation != null) && OnRunAnimation(animation, markerId, animationTimeCorrection);
		}

		public override void AdvanceAnimation(Animation animation, float delta)
		{
			OnAdvanceAnimation?.Invoke(animation, delta);
		}

		public override void ApplyAnimatorsAndExecuteTriggers(Animation animation, double previousTime, double currentTime, bool executeTriggersAtCurrentTime)
		{
			OnApplyEffectiveAnimatorsAndBuildTriggersList?.Invoke(animation, previousTime, currentTime, executeTriggersAtCurrentTime);
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
			// Easings may give huge animationTimeCorrection values, clamp it.
			animationTimeCorrection = Mathf.Clamp(animationTimeCorrection, -AnimationUtils.SecondsPerFrame, 0);
			animation.Time = AnimationUtils.FramesToSeconds(frame) + animationTimeCorrection;
			animation.RunningMarkerId = markerId;
			animation.IsRunning = true;
			return true;
		}

		public override void AdvanceAnimation(Animation animation, float delta)
		{
			var previousTime = animation.Time;
			var currentTime = previousTime + delta;
			animation.TimeInternal = currentTime;
			animation.MarkerAhead = animation.MarkerAhead ?? FindMarkerAhead(animation, previousTime);
			if (animation.MarkerAhead == null || currentTime < animation.MarkerAhead.Time) {
				ApplyAnimatorsAndExecuteTriggers(animation, previousTime, currentTime, executeTriggersAtCurrentTime: false);
			} else {
				var marker = animation.MarkerAhead;
				animation.MarkerAhead = null;
				ProcessMarker(animation, marker);
				if (marker.Action == MarkerAction.Stop) {
					ApplyAnimatorsAndExecuteTriggers(animation, previousTime, animation.Time, executeTriggersAtCurrentTime: true);
					animation.RaiseStopped();
				} else if (marker.Action == MarkerAction.Play) {
					ApplyAnimatorsAndExecuteTriggers(animation, previousTime, currentTime, executeTriggersAtCurrentTime: false);
				}
			}
		}

		protected static Marker FindMarkerAhead(Animation animation, double time)
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

		public override void ApplyAnimatorsAndExecuteTriggers(Animation animation, double previousTime, double currentTime, bool executeTriggersAtCurrentTime)
		{
			if (!AreEffectiveAnimatorsValid(animation)) {
				BuildEffectiveAnimators(animation);
			}
			foreach (var a in animation.EffectiveAnimators) {
				a.Apply(currentTime);
			}
			foreach (var a in animation.EffectiveTriggerableAnimators) {
				a.ExecuteTriggersInRange(previousTime, currentTime, executeTriggersAtCurrentTime);
			}
#if TANGERINE
			foreach (var track in animation.Tracks) {
				// To edit the track weight in the inspector, it must be animated
				track.Animators.Apply(animation.Time, animation.Id);
			}
#endif
		}

		public override bool AreEffectiveAnimatorsValid(Animation animation)
		{
			if (animation.IsCompound) {
				if (animation.EffectiveAnimators == null) {
					return false;
				}
				foreach (var track in animation.Tracks) {
					foreach (var clip in track.Clips) {
						var clipAnimation = clip.CachedAnimation;
						if (clipAnimation == null) {
							if (clip.FindAnimation() != null) {
								return false;
							}
						} else if (
							(clipAnimation.IdComparisonCode != clip.AnimationIdComparisonCode) ||
							(clipAnimation.Owner != animation.Owner) ||
							(clipAnimation != null && !AreEffectiveAnimatorsValid(clipAnimation))
						) {
							return false;
						}
					}
				}
				return true;
			} else {
				return
					animation.Owner.DescendantAnimatorsVersion == animation.EffectiveAnimatorsVersion &&
					animation.EffectiveAnimators != null;
			}
		}

		public override void BuildEffectiveAnimators(Animation animation)
		{
			if (animation.IsCompound) {
				BuildEffectiveAnimatorsForCompoundAnimation(animation);
			} else {
				BuildEffectiveAnimatorsForSimpleAnimation(animation);
			}
		}

		private static void BuildEffectiveAnimatorsForCompoundAnimation(Animation animation)
		{
			(animation.EffectiveAnimators ?? (animation.EffectiveAnimators = new List<IAbstractAnimator>())).Clear();
			(animation.EffectiveTriggerableAnimators ?? (animation.EffectiveTriggerableAnimators = new List<IAbstractAnimator>())).Clear();
			var animationBindings = new Dictionary<AnimatorBinding, (IAbstractAnimator Animator, AnimationTrack Track)>();
			var trackBindings = new Dictionary<AnimatorBinding, IChainedAnimator>();
			foreach (var track in animation.Tracks) {
				trackBindings.Clear();
				foreach (var clip in track.Clips) {
					var clipAnimation = clip.FindAnimation();
					clip.CachedAnimation = clipAnimation;
					if (clipAnimation == null) {
						continue;
					}
					if (!clipAnimation.AnimationEngine.AreEffectiveAnimatorsValid(clipAnimation)) {
						clipAnimation.AnimationEngine.BuildEffectiveAnimators(clipAnimation);
					}
					foreach (var a in clipAnimation.EffectiveAnimators) {
						if (trackBindings.TryGetValue(new AnimatorBinding(a), out var chained)) {
							chained.Add(clip, a);
						} else {
							chained = AnimatorRegistry.Instance.CreateChainedAnimator(a.ValueType);
							chained.Add(clip, a);
							trackBindings.Add(new AnimatorBinding(a), chained);
						}
					}
				}
				foreach (var a in trackBindings.Values) {
					if (animationBindings.TryGetValue(new AnimatorBinding(a), out var i)) {
						if (i.Animator is IBlendedAnimator blended) {
							blended.Add(track, a);
						} else {
							animationBindings.Remove(new AnimatorBinding(a));
							blended = AnimatorRegistry.Instance.CreateBlendedAnimator(a.ValueType);
							blended.Add(i.Track, i.Animator);
							blended.Add(track, a);
							animationBindings.Add(new AnimatorBinding(a), (blended, track));
						}
					} else {
						animationBindings.Add(new AnimatorBinding(a), (a, track));
					}
				}
			}
			foreach (var b in animationBindings.Values) {
				var a = b.Animator;
				if (animation.HasEasings()) {
					var a2 = AnimatorRegistry.Instance.CreateEasedAnimator(a.ValueType);
					a2.Initialize(animation, a);
					a = a2;
				}
				animation.EffectiveAnimators.Add(a);
				if (a.IsTriggerable) {
					animation.EffectiveTriggerableAnimators.Add(a);
				}
			}
		}

		private struct AnimatorBinding : IEquatable<AnimatorBinding>
		{
			public IAnimable Animable;
			public int TargetPropertyPathComparisonCode;

			public AnimatorBinding(IAbstractAnimator animator)
			{
				Animable = animator.Animable;
				TargetPropertyPathComparisonCode = animator.TargetPropertyPathComparisonCode;
			}

			public bool Equals(AnimatorBinding other)
			{
				return Animable == other.Animable && TargetPropertyPathComparisonCode == other.TargetPropertyPathComparisonCode;
			}

			public override int GetHashCode()
			{
				unchecked {
					var r = -511344;
					r = r * -1521134295 + Animable.GetHashCode();
					r = r * -1521134295 + TargetPropertyPathComparisonCode;
					return r;
				}
			}
		}

		private static void BuildEffectiveAnimatorsForSimpleAnimation(Animation animation)
		{
			(animation.EffectiveAnimators ?? (animation.EffectiveAnimators = new List<IAbstractAnimator>())).Clear();
			(animation.EffectiveTriggerableAnimators ?? (animation.EffectiveTriggerableAnimators = new List<IAbstractAnimator>())).Clear();
			animation.EffectiveAnimatorsVersion = animation.Owner.DescendantAnimatorsVersion;
			AddEffectiveAnimatorsRecursively(animation.Owner);

			void AddEffectiveAnimatorsRecursively(Node node)
			{
				foreach (var child in node.Nodes) {
					// Optimization: avoid calling Animators.GetEnumerator() for empty collection since it allocates memory
					if (child.Animators.Count > 0) {
						foreach (var a in child.Animators) {
							if (a.AnimationId == animation.Id) {
								var a2 = (IAbstractAnimator)a;
								if (animation.HasEasings()) {
									var a3 = AnimatorRegistry.Instance.CreateEasedAnimator(a.ValueType);
									a3.Initialize(animation, a);
									a2 = a3;
								}
								animation.EffectiveAnimators.Add(a2);
								if (a2.IsTriggerable) {
									animation.EffectiveTriggerableAnimators.Add(a2);
								}
							}
						}
					}
					var stopRecursion = animation.Id == null;
					foreach (var a in child.Animations) {
						if (a.IdComparisonCode == animation.IdComparisonCode) {
							stopRecursion = true;
							break;
						}
					}
					if (!stopRecursion) {
						AddEffectiveAnimatorsRecursively(child);
					}
				}
			}
		}
	}

	public class FastForwardAnimationEngine : DefaultAnimationEngine
	{
		public override void AdvanceAnimation(Animation animation, float delta)
		{
			var previousTime = animation.Time;
			var currentTime = previousTime + delta;
			animation.TimeInternal = currentTime;
			animation.MarkerAhead = animation.MarkerAhead ?? FindMarkerAhead(animation, previousTime);
			if (animation.MarkerAhead == null || currentTime < animation.MarkerAhead.Time) {
				// Do nothing
			} else {
				var marker = animation.MarkerAhead;
				animation.MarkerAhead = null;
				ProcessMarker(animation, marker);
				if (marker.Action == MarkerAction.Stop) {
					ApplyAnimatorsAndExecuteTriggers(animation, previousTime, animation.Time, executeTriggersAtCurrentTime: true);
					animation.RaiseStopped();
				} else if (marker.Action == MarkerAction.Play) {
					ApplyAnimatorsAndExecuteTriggers(animation, previousTime, currentTime, executeTriggersAtCurrentTime: false);
				}
			}
		}
	}
}
