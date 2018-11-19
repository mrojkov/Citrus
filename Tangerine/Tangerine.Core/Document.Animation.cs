using Lime;
using System.Collections.Generic;

namespace Tangerine.Core
{
	public sealed partial class Document
	{
		private List<double> savedAnimationTimes;

		public bool SlowMotion { get; set; }

		public static bool CacheAnimationsStates
		{
			get { return CurrentFrameSetter.CacheAnimationsStates; }
			set { CurrentFrameSetter.CacheAnimationsStates = value; }
		}

		public static void SetCurrentFrameToNode(int frameIndex, Animation animation, bool animationMode, bool isForced = false)
		{
			CurrentFrameSetter.SetCurrentFrameToNode(frameIndex, animation, animationMode, isForced);
		}

		private static void FastForwardToFrame(Animation animation, int frameIndex)
		{
			var node = animation.Owner;
			node.SetTangerineFlag(TangerineFlags.IgnoreMarkers, true);
			try {
				CurrentFrameSetter.FastForwardToFrame(animation, frameIndex);
			} finally {
				node.SetTangerineFlag(TangerineFlags.IgnoreMarkers, false);
			}
		}

		public void TogglePreviewAnimation(bool animationMode, bool triggerMarkersBeforeCurrentFrame)
		{
			if (PreviewAnimation) {
				PreviewAnimation = false;
				Animation.IsRunning = false;
				CurrentFrameSetter.StopAnimationsRecursive(PreviewAnimationContainer);
				if (!CoreUserPreferences.Instance.StopAnimationOnCurrentFrame) {
					var i = 0;
					foreach (var node in PreviewAnimationContainer.Descendants) {
						node.DefaultAnimation.Time = animationMode ? 0 : savedAnimationTimes[i++];
					}
					SetCurrentFrameToNode(
						PreviewAnimationBegin, Animation, animationMode
					);
				}
				AudioSystem.StopAll();
				ForceAnimationUpdate();
			} else {
				savedAnimationTimes = new List<double>();
				foreach (var node in Container.Descendants) {
					savedAnimationTimes.Add(node.DefaultAnimation.Time);
				}
				foreach (var node in RootNode.Descendants) {
					if (node is ITangerinePreviewAnimationListener t) {
						t.OnStart();
					}
				}
				int savedAnimationFrame = AnimationFrame;
				PreviewAnimation = true;
				if (triggerMarkersBeforeCurrentFrame) {
					SetCurrentFrameToNode(0, Animation, true);
				}
				Animation.IsRunning = PreviewAnimation;
				if (triggerMarkersBeforeCurrentFrame) {
					FastForwardToFrame(Animation, savedAnimationFrame);
				}
				PreviewAnimationBegin = savedAnimationFrame;
				PreviewAnimationContainer = Container;
			}
			Application.InvalidateWindows();
		}


		public static void ForceAnimationUpdate()
		{
			if (Current == null) {
				return;
			}
			SetCurrentFrameToNode(
				Current.AnimationFrame,
				Current.Animation,
				CoreUserPreferences.Instance.AnimationMode,
				isForced: true
			);
		}

		private static class CurrentFrameSetter
		{

			private class AnimationStateComponent : NodeComponent
			{
				private struct AnimationState
				{
					public bool IsRunning;
					public double Time;
					public string AnimationId;
				}

				private AnimationState state;
				public int Column => AnimationUtils.SecondsToFrames(state.Time);

				public static void Create(Node node, string animationId)
				{
					var component = new AnimationStateComponent {
						state = new AnimationState()
					};
					node.Components.Add(component);
					if (node.Animations.TryFind(animationId, out var animation)) {
						var state = new AnimationState {
							IsRunning = animation.IsRunning,
							Time = animation.Time,
							AnimationId = animationId
						};
						component.state = state;
					}
					foreach (var child in node.Nodes) {
						Create(child, animationId);
					}
				}

				internal static void Restore(Node node)
				{
					var component = node.Components.Get<AnimationStateComponent>();
					if (node.Animations.TryFind(component.state.AnimationId, out var animation)) {
						var state = component.state;
						animation.IsRunning = state.IsRunning;
						animation.Time = state.Time;
					}

					foreach (var child in node.Nodes) {
						Restore(child);
					}
				}

				public static bool Exists(Node node)
				{
					return node.Components.Contains<AnimationStateComponent>();
				}

				public static void Remove(Node node)
				{
					node.Components.Remove<AnimationStateComponent>();
					foreach (var child in node.Nodes) {
						Remove(child);
					}
				}
			}

			const int OptimalRollbackForCacheAnimationsStates = 150;
			static bool cacheAnimationsStates;

			internal static bool CacheAnimationsStates
			{
				get { return cacheAnimationsStates; }
				set {
					cacheAnimationsStates = value;
					if (!cacheAnimationsStates && AnimationStateComponent.Exists(Current.Container)) {
						AnimationStateComponent.Remove(Current.Container);
					}
				}
			}

			internal static void SetCurrentFrameToNode(int frameIndex, Animation animation, bool animationMode, bool isForced)
			{
				Audio.GloballyEnable = false;
				try {
					var doc = Current;
					var node = animation.Owner;
					if (animationMode && (doc.AnimationFrame != frameIndex || isForced)) {
						node.SetTangerineFlag(TangerineFlags.IgnoreMarkers, true);
						var cacheFrame = node.Components.Get<AnimationStateComponent>()?.Column;
						if (cacheFrame.HasValue && (cacheFrame.Value > frameIndex ||
							frameIndex > cacheFrame.Value + OptimalRollbackForCacheAnimationsStates * 2)) {
							AnimationStateComponent.Remove(node);
							cacheFrame = null;
						}
						if (!cacheFrame.HasValue) {
							StopAnimationsRecursive(node);
							SetTimeRecursive(node, animation.Id, 0);
						} else {
							AnimationStateComponent.Restore(node);
						}
						ClearParticlesRecursive(doc.RootNode);
						animation.IsRunning = true;
						if (CacheAnimationsStates && !cacheFrame.HasValue) {
							cacheFrame = frameIndex - OptimalRollbackForCacheAnimationsStates;
							if (cacheFrame.Value > 0) {
								FastForwardToFrame(animation, cacheFrame.Value);
								AnimationStateComponent.Create(node, animation.Id);
							}
						}
						FastForwardToFrame(animation, frameIndex);
						StopAnimationsRecursive(node);
						node.SetTangerineFlag(TangerineFlags.IgnoreMarkers, false);

						// Force update to reset Animation.NextMarkerOrTriggerTime for parents.
						animation.Frame = doc.Animation.Frame;
						doc.RootNode.Update(0);
					} else {
						animation.Frame = frameIndex;
						node.Update(0);
						ClearParticlesRecursive(node);
					}
				} finally {
					Audio.GloballyEnable = true;
				}
			}

			internal static void FastForwardToFrame(Animation animation, int frame)
			{
				// Try to decrease error in node.AnimationTime by call node.Update several times
				const float OptimalDelta = 10;
				float forwardDelta;
				do {
					forwardDelta = CalcDeltaToFrame(animation, frame);
					var delta = Mathf.Min(forwardDelta, OptimalDelta);
					animation.Owner.Update(delta);
				} while (forwardDelta > OptimalDelta);
			}

			static float CalcDeltaToFrame(Animation animation, int frame)
			{
				var forwardDelta = AnimationUtils.SecondsPerFrame * frame - animation.Time;
				// Make sure that animation engine will invoke triggers on last frame
				forwardDelta += 0.00001;
				// Hack: CompatibilityAnimationEngine workaround
				if (Current.Format == DocumentFormat.Scene) {
					forwardDelta *= 2f;
				}
				return (float) forwardDelta;
			}

			internal static void SetTimeRecursive(Node node, string animationId, double time)
			{
				if (node.Animations.TryFind(animationId, out var animation)) {
					animation.Time = time;
				}
				foreach (var child in node.Nodes) {
					SetTimeRecursive(child, animationId, time);
				}
			}

			internal static void StopAnimationsRecursive(Node node)
			{
				foreach (var animation in node.Animations) {
					animation.IsRunning = false;
				}
				foreach (var child in node.Nodes) {
					StopAnimationsRecursive(child);
				}
			}

			static void ClearParticlesRecursive(Node node)
			{
				if (node is ParticleEmitter) {
					var e = (ParticleEmitter) node;
					e.ClearParticles();
				}
				foreach (var child in node.Nodes) {
					ClearParticlesRecursive(child);
				}
			}

		}

	}
}
