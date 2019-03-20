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
		/// 2. Calculates each animator at currTime;
		/// 3. Adds triggers in given range to animation.Triggers.
		/// The range is [prevTime, currTime) or [prevTime, currTime] depending on inclusiveRange flag.
		/// This method doesn't depend on animation.Time value.
		/// </summary>
		public virtual void CalcEffectiveAnimatorsAndTriggers(Animation animation, double prevTime, double currTime, bool inclusiveRange) { }
		public virtual double EaseTime(Animation animation, double time) => time;

		public void ApplyEffectiveAnimators(Animation animation)
		{
			foreach (var a in animation.EffectiveAnimators) {
				a.Apply();
			}
		}

		public void InvokeTriggers(Animation animation)
		{
			foreach (var i in animation.EffectiveTriggers) {
				i();
			}
		}
	}

	public class AnimationEngineDelegate : AnimationEngine
	{
		public Func<Animation, string, double, bool> OnRunAnimation;
		public Action<Animation, float> OnAdvanceAnimation;
		public Action<Animation, double, double, bool> OnCalcEffectiveAnimatorsAndTriggers;

		public override bool TryRunAnimation(Animation animation, string markerId, double animationTimeCorrection = 0)
		{
			return (OnRunAnimation != null) && OnRunAnimation(animation, markerId, animationTimeCorrection);
		}

		public override void AdvanceAnimation(Animation animation, float delta)
		{
			OnAdvanceAnimation?.Invoke(animation, delta);
		}

		public override void CalcEffectiveAnimatorsAndTriggers(Animation animation, double prevTime, double currTime, bool inclusiveRange)
		{
			OnCalcEffectiveAnimatorsAndTriggers?.Invoke(animation, prevTime, currTime, inclusiveRange);
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
			var prevTime = animation.Time;
			var currTime = prevTime + delta;
			animation.TimeInternal = currTime;
			animation.NextMarker = animation.NextMarker ?? GetNextMarker(animation, prevTime);
			if (animation.NextMarker == null || currTime < animation.NextMarker.Time) {
				AdvanceAnimationHelper(animation, prevTime, currTime, inclusiveRange: false);
			} else {
				var marker = animation.NextMarker;
				animation.NextMarker = null;
				ProcessMarker(animation, marker);
				if (marker.Action == MarkerAction.Stop) {
					AdvanceAnimationHelper(animation, prevTime, animation.Time, inclusiveRange: true);
					animation.RaiseStopped();
				} else if (marker.Action == MarkerAction.Play) {
					AdvanceAnimationHelper(animation, prevTime, currTime, inclusiveRange: false);
				}
			}
		}

		protected void AdvanceAnimationHelper(Animation animation, double prevTime, double currTime, bool inclusiveRange)
		{
			CalcEffectiveAnimatorsAndTriggers(animation,
				EaseTime(animation, prevTime),
				EaseTime(animation, currTime),
				inclusiveRange);
			ApplyEffectiveAnimators(animation);
			InvokeTriggers(animation);
		}

		protected static Marker GetNextMarker(Animation animation, double time)
		{
			if (animation.Markers.Count == 0) {
				return null;
			}
			var frame = AnimationUtils.SecondsToFramesCeiling(time);
			foreach (var marker in animation.Markers) {
				if (marker.Frame >= frame) {
					return marker;
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

		public override void CalcEffectiveAnimatorsAndTriggers(Animation animation, double prevTime, double currTime, bool inclusiveRange)
		{
			if (animation.IsCompound) {
				CalcEffectiveAnimatorsAndTriggersForCompoundAnimation(animation, prevTime, currTime, inclusiveRange);
			} else {
				CalcEffectiveAnimatorsAndTriggersForSimpleAnimation(animation, prevTime, currTime, inclusiveRange);
			}
		}

		private static void CalcEffectiveAnimatorsAndTriggersForCompoundAnimation(Animation animation, double prevTime, double currTime, bool inclusiveRange)
		{
			(animation.EffectiveAnimators ?? (animation.EffectiveAnimators = new List<IAnimator>())).Clear();
			(animation.EffectiveTriggers ?? (animation.EffectiveTriggers = new List<Action>())).Clear();
			(animation.CollisionMap ?? (animation.CollisionMap = new AnimationCollisionMap())).Clear();
			int frame = AnimationUtils.SecondsToFrames(currTime);
			var totalWeight = 0f;
			foreach (var track in animation.Tracks) {
				totalWeight += track.Weight;
				var blendFactor = totalWeight > 0 ? track.Weight / totalWeight : 0;
				foreach (var a in track.Animators) {
					a.CalcAndApply(currTime); // Animate track weight and so on...
				}
				foreach (var clip in track.Clips) {
					if (frame < clip.Begin || frame >= clip.End) {
						continue;
					}
					var clipEngine = clip.Animation.AnimationEngine;
					var clipPrevTime = clipEngine.EaseTime(clip.Animation, clip.RemapTime(prevTime));
					var clipCurrTime = clipEngine.EaseTime(clip.Animation, clip.RemapTime(currTime));
					clipEngine.CalcEffectiveAnimatorsAndTriggers(clip.Animation, clipPrevTime, clipCurrTime, inclusiveRange);
					foreach (var a in clip.Animation.EffectiveAnimators) {
						if (!animation.CollisionMap.TryGetAnimator(a, out var masterAnimator)) {
							animation.CollisionMap.AddAnimator(a);
							animation.EffectiveAnimators.Add(a);
						} else {
							masterAnimator.BlendWith(a, blendFactor);
						}
					}
					foreach (var t in clip.Animation.EffectiveTriggers) {
						animation.EffectiveTriggers.Add(t);
					}
				}
			}
		}

		private static void CalcEffectiveAnimatorsAndTriggersForSimpleAnimation(Animation animation, double prevTime, double currTime, bool inclusiveRange)
		{
			(animation.EffectiveTriggers ?? (animation.EffectiveTriggers = new List<Action>())).Clear();
			foreach (var a in GetEffectiveAnimators()) {
				a.CalcValue(currTime);
				if (a.IsTriggerable) {
					a.AddTriggersInRange(animation.EffectiveTriggers, prevTime, currTime, inclusiveRange);
				}
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
	}

	public class FastForwardAnimationEngine : DefaultAnimationEngine
	{
		public override void AdvanceAnimation(Animation animation, float delta)
		{
			var prevTime = animation.Time;
			var currTime = prevTime + delta;
			animation.TimeInternal = currTime;
			animation.NextMarker = animation.NextMarker ?? GetNextMarker(animation, prevTime);
			if (animation.NextMarker == null || currTime < animation.NextMarker.Time) {
				// Do nothing
			} else {
				var marker = animation.NextMarker;
				animation.NextMarker = null;
				ProcessMarker(animation, marker);
				if (marker.Action == MarkerAction.Stop) {
					AdvanceAnimationHelper(animation, prevTime, animation.Time, inclusiveRange: true);
					animation.RaiseStopped();
				} else if (marker.Action == MarkerAction.Play) {
					AdvanceAnimationHelper(animation, prevTime, currTime, inclusiveRange: false);
				}
			}
		}
	}
}
