using Lime;

namespace Tangerine.Core
{
	public sealed partial class Document
	{

		public static bool CacheAnimationsStates
		{
			get { return CurrentFrameSetter.CacheAnimationsStates; }
			set { CurrentFrameSetter.CacheAnimationsStates = value; }
		}

		public static void SetCurrentFrameToNode(int frameIndex, Node node, bool animationMode)
		{
			CurrentFrameSetter.SetCurrentFrameToNode(frameIndex, node, animationMode);
		}

		private static void FastForwardToFrame(Node node, int frameIndex)
		{
			node.SetTangerineFlag(TangerineFlags.IgnoreMarkers, true);
			try {
				CurrentFrameSetter.FastForwardToFrame(node, frameIndex);
			} finally {
				node.SetTangerineFlag(TangerineFlags.IgnoreMarkers, false);
			}
		}

		public void TogglePreviewAnimation(bool animationMode, bool triggerMarkersBeforeCurrentFrame)
		{
			if (PreviewAnimation) {
				PreviewAnimation = false;
				CurrentFrameSetter.StopAnimationRecursive(PreviewAnimationContainer);
				CurrentFrameSetter.SetTimeRecursive(PreviewAnimationContainer, 0);
				SetCurrentFrameToNode(
					PreviewAnimationBegin, Container, animationMode
				);
				AudioSystem.StopAll();
			} else {
				int savedAnimationFrame = Container.AnimationFrame;
				PreviewAnimation = true;
				if (triggerMarkersBeforeCurrentFrame) {
					SetCurrentFrameToNode(0, Container, true);
				}
				Container.IsRunning = PreviewAnimation;
				if (triggerMarkersBeforeCurrentFrame) {
					FastForwardToFrame(Container, savedAnimationFrame);
				}
				PreviewAnimationBegin = savedAnimationFrame;
				PreviewAnimationContainer = Container;
			}
			Application.InvalidateWindows();
		}

		private static class CurrentFrameSetter
		{

			private class AnimationsStatesComponent : NodeComponent
			{
				private struct AnimationState
				{
					public bool IsRunning;
					public double Time;
				}

				private AnimationState[] animationsStates;
				public int Column => AnimationUtils.SecondsToFrames(animationsStates[0].Time);

				public static void Create(Node node)
				{
					var component = new AnimationsStatesComponent {
						animationsStates = new AnimationState[node.Animations.Count]
					};
					node.Components.Add(component);
					var i = 0;
					foreach (var animation in node.Animations) {
						var state = new AnimationState {
							IsRunning = animation.IsRunning,
							Time = animation.Time
						};
						component.animationsStates[i] = state;
						i++;
					}

					foreach (var child in node.Nodes) {
						Create(child);
					}
				}

				internal static void Restore(Node node)
				{
					var component = node.Components.Get<AnimationsStatesComponent>();
					var i = 0;
					foreach (var animation in node.Animations) {
						var state = component.animationsStates[i];
						animation.IsRunning = state.IsRunning;
						animation.Time = state.Time;
						i++;
					}

					foreach (var child in node.Nodes) {
						Restore(child);
					}
				}

				public static bool Exists(Node node)
				{
					return node.Components.Contains<AnimationsStatesComponent>();
				}

				public static void Remove(Node node)
				{
					node.Components.Remove<AnimationsStatesComponent>();
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
					if (!cacheAnimationsStates && AnimationsStatesComponent.Exists(Current.Container)) {
						AnimationsStatesComponent.Remove(Current.Container);
					}
				}
			}

			internal static void SetCurrentFrameToNode(int frameIndex, Node node, bool animationMode)
			{
				Audio.GloballyEnable = false;
				try {
					var doc = Current;
					if (animationMode && doc.AnimationFrame != frameIndex) {
						node.SetTangerineFlag(TangerineFlags.IgnoreMarkers, true);
						var cacheFrame = node.Components.Get<AnimationsStatesComponent>()?.Column;
						if (cacheFrame.HasValue && (cacheFrame.Value > frameIndex ||
							frameIndex > cacheFrame.Value + OptimalRollbackForCacheAnimationsStates * 2)) {
							AnimationsStatesComponent.Remove(node);
							cacheFrame = null;
						}
						if (!cacheFrame.HasValue) {
							StopAnimationRecursive(node);
							SetTimeRecursive(node, 0);
						} else {
							AnimationsStatesComponent.Restore(node);
						}
						ClearParticlesRecursive(doc.RootNode);
						node.IsRunning = true;

						if (CacheAnimationsStates && !cacheFrame.HasValue) {
							cacheFrame = frameIndex - OptimalRollbackForCacheAnimationsStates;
							if (cacheFrame.Value > 0) {
								FastForwardToFrame(node, cacheFrame.Value);
								AnimationsStatesComponent.Create(node);
							}
						}
						FastForwardToFrame(node, frameIndex);
						StopAnimationRecursive(node);
						node.SetTangerineFlag(TangerineFlags.IgnoreMarkers, false);

						// Force update to reset Animation.NextMarkerOrTriggerTime for parents.
						doc.Container.AnimationFrame = doc.Container.AnimationFrame;
						doc.RootNode.Update(0);
					} else {
						node.AnimationFrame = frameIndex;
						node.Update(0);
						ClearParticlesRecursive(node);
					}
				} finally {
					Audio.GloballyEnable = true;
				}
			}

			internal static void FastForwardToFrame(Node node, int frame)
			{
				// Try to decrease error in node.AnimationTime by call node.Update several times
				const float OptimalDelta = 10;
				float forwardDelta;
				do {
					forwardDelta = CalcDeltaToFrame(node, frame);
					var delta = Mathf.Min(forwardDelta, OptimalDelta);
					node.Update(delta);
				} while (forwardDelta > OptimalDelta);
			}

			static float CalcDeltaToFrame(Node node, int frame)
			{
				var forwardDelta = AnimationUtils.SecondsPerFrame * frame - node.AnimationTime;
				// Make sure that animation engine will invoke triggers on last frame
				forwardDelta += 0.00001;
				// Hack: CompatibilityAnimationEngine workaround
				if (Current.Format == DocumentFormat.Scene) {
					forwardDelta *= 2f;
				}
				return (float) forwardDelta;
			}

			internal static void SetTimeRecursive(Node node, double time)
			{
				foreach (var animation in node.Animations) {
					animation.Time = time;
				}
				foreach (var child in node.Nodes) {
					SetTimeRecursive(child, time);
				}
			}

			internal static void StopAnimationRecursive(Node node)
			{
				foreach (var animation in node.Animations) {
					animation.IsRunning = false;
				}
				foreach (var child in node.Nodes) {
					StopAnimationRecursive(child);
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
