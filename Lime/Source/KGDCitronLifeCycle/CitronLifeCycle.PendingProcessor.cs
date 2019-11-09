namespace Lime.KGDCitronLifeCycle {
	public static partial class CitronLifeCycle
	{
		public class PendingProcessor : NodeProcessor
		{
			public readonly NodeManagerPhase ManagerPhase;

			private BehaviorSystem behaviorSystem;
			private PendingSystem pendingSystem;
			private AnimationSystem animationSystem;
			private BehaviorUpdateStage onAnimatedUpdateStage;

			public PendingProcessor(NodeManagerPhase nodeManagerPhase)
			{
				ManagerPhase = nodeManagerPhase;
			}

			protected internal override void Start()
			{
				behaviorSystem = Manager.ServiceProvider.RequireService<BehaviorSystem>();
				animationSystem = Manager.ServiceProvider.RequireService<AnimationSystem>();
				pendingSystem = Manager.ServiceProvider.RequireService<PendingSystem>();
				onAnimatedUpdateStage = behaviorSystem.GetUpdateStage(typeof(AfterAnimationStage));
			}

			protected internal override void Stop()
			{
				behaviorSystem = null;
				animationSystem = null;
				pendingSystem = null;
				onAnimatedUpdateStage = null;
			}

			protected internal override void Update(float delta)
			{
				Repeat:

				if (behaviorSystem.HasStartPendingBehaviors()) {
					behaviorSystem.StartPendingBehaviors();
					goto Repeat;
				}

				if (pendingSystem.PendingCustomUpdates.Count > 0) {
					while (pendingSystem.PendingCustomUpdates.Count > 0) {
						var customUpdate = pendingSystem.PendingCustomUpdates.First.Value;
						pendingSystem.PendingCustomUpdates.RemoveFirst();
						customUpdate(0);
					}
					goto Repeat;
				}

				if (pendingSystem.PendingTasksUpdate.Count > 0) {
					while (pendingSystem.PendingTasksUpdate.Count > 0) {
						var node = pendingSystem.PendingTasksUpdate.First.Value;
						pendingSystem.PendingTasksUpdate.RemoveFirst();
						node.Tasks.Update(0);
					}
					goto Repeat;
				}

				if (pendingSystem.PendingAdvanceAnimation.Count > 0) {
					while (pendingSystem.PendingAdvanceAnimation.Count > 0) {
						var node = pendingSystem.PendingAdvanceAnimation.First.Value;
						pendingSystem.PendingAdvanceAnimation.RemoveFirst();
						node.AdvanceAnimationsRecursive(0);
					}

					goto Repeat;
				}

				onAnimatedUpdateStage.Update(0);
				if (
					behaviorSystem.HasStartPendingBehaviors() ||
					pendingSystem.PendingCustomUpdates.Count > 0 ||
					pendingSystem.PendingTasksUpdate.Count > 0 ||
					pendingSystem.PendingAdvanceAnimation.Count > 0
				) {
					goto Repeat;
				}

				if (animationSystem.ConsumePendingActions()) {
					goto Repeat;
				}
			}
		}
	}
}