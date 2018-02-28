using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations
{
	public class SetCurrentColumn : Operation
	{
		protected int Column;
		protected Node Container;

		public override bool IsChangingDocument => false;

		public static void Perform(int column, Node container)
		{
			Document.Current.History.Perform(new SetCurrentColumn(column, container));
		}

		public static void Perform(int column)
		{
			Document.Current.History.Perform(new SetCurrentColumn(column, Document.Current.Container));
		}

		private SetCurrentColumn(int column, Node node)
		{
			Column = column;
			Container = node;
		}

		public class Processor : OperationProcessor<SetCurrentColumn>
		{
			class Backup { public int Column; }

			public class AnimationsStatesComponent : NodeComponent
			{
				public struct AnimationState
				{
					public bool IsRunning;
					public double Time;
				}

				public AnimationState[] AnimationsStates;
				public int Column => AnimationUtils.SecondsToFrames(AnimationsStates[0].Time);

				public static void Create(Node node, int column)
				{
					var component = new AnimationsStatesComponent {
						AnimationsStates = new AnimationState[node.Animations.Count]
					};
					node.Components.Add(component);
					var i = 0;
					foreach (var animation in node.Animations) {
						var state = new AnimationState {
							IsRunning = animation.IsRunning,
							Time = animation.Time
						};
						component.AnimationsStates[i] = state;
						i++;
					}

					foreach (var child in node.Nodes) {
						Create(child, column);
					}
				}

				public static void Restore(Node node)
				{
					var component = node.Components.Get<AnimationsStatesComponent>();
					var i = 0;
					foreach (var animation in node.Animations) {
						var state = component.AnimationsStates[i];
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

			public static bool CacheAnimationsStates
			{
				get { return cacheAnimationsStates; }
				set
				{
					cacheAnimationsStates = value;
					if (!cacheAnimationsStates && AnimationsStatesComponent.Exists(Document.Current.Container)) {
						AnimationsStatesComponent.Remove(Document.Current.Container);
					}
				}
			}

			protected override void InternalRedo(SetCurrentColumn op)
			{
				op.Save(new Backup { Column = Timeline.Instance.CurrentColumn });
				SetColumn(op.Column, op.Container);
			}

			protected override void InternalUndo(SetCurrentColumn op)
			{
				SetColumn(op.Restore<Backup>().Column, op.Container);
			}

			void SetColumn(int value, Node node)
			{
				Audio.GloballyEnable = false;
				try {
					var doc = Document.Current;
					if (TimelineUserPreferences.Instance.AnimationMode && doc.AnimationFrame != value) {
						node.SetTangerineFlag(TangerineFlags.IgnoreMarkers, true);
						var cacheFrame = node.Components.Get<AnimationsStatesComponent>()?.Column;
						if (cacheFrame.HasValue && (cacheFrame.Value > value || value > cacheFrame.Value + OptimalRollbackForCacheAnimationsStates * 2)) {
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
							cacheFrame = value - OptimalRollbackForCacheAnimationsStates;
							if (cacheFrame.Value > 0) {
								FastForwardToFrame(node, cacheFrame.Value);
								AnimationsStatesComponent.Create(node, cacheFrame.Value);
							}
						}
						FastForwardToFrame(node, value);
						StopAnimationRecursive(node);
						node.SetTangerineFlag(TangerineFlags.IgnoreMarkers, false);
					} else {
						node.AnimationFrame = value;
						node.Update(0);
						ClearParticlesRecursive(node);
					}
					Timeline.Instance.EnsureColumnVisible(value);
				} finally {
					Audio.GloballyEnable = true;
				}
			}

			static void FastForwardToFrame(Node node, int frame)
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
				if (Document.Current.Format == DocumentFormat.Scene) {
					forwardDelta *= 2f;
				}
				return (float)forwardDelta;
			}

			static void SetTimeRecursive(Node node, double time)
			{
				foreach (var animation in node.Animations) {
					animation.Time = time;
				}
				foreach (var child in node.Nodes) {
					SetTimeRecursive(child, time);
				}
			}

			static void StopAnimationRecursive(Node node)
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
					var e = (ParticleEmitter)node;
					e.ClearParticles();
				}
				foreach (var child in node.Nodes) {
					ClearParticlesRecursive(child);
				}
			}
		}
	}
}