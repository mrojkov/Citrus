using System.Linq;

namespace Lime.KGDCitronLifeCycle {
	public static partial class CitronLifeCycle
	{
		/// <summary>
		/// Очень важен, именно он восстанавливает жизненный цикл до желаемого.
		/// </summary>
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

			protected internal override void Stop(NodeManager manager)
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

				if (ProcessCustomUpdates((int) NodeManagerPhase.PendingPreEarlyUpdate)) {
					goto Repeat;
				}

				if (ProcessCustomUpdates((int) NodeManagerPhase.PendingEarlyUpdate)) {
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

				if (ProcessCustomUpdates((int) NodeManagerPhase.PendingPostEarlyUpdate)) {
					goto Repeat;
				}

				if (pendingSystem.PendingAdvanceAnimation.Count > 0) {
					pipelinedAdvanceAnimationActiveCount++;

					while (pendingSystem.PendingAdvanceAnimation.Count > 0) {
						var node = pendingSystem.PendingAdvanceAnimation.First.Value;
						pendingSystem.PendingAdvanceAnimation.RemoveFirst();
						node.AdvanceAnimationsRecursive(0);
					}

					pipelinedAdvanceAnimationActiveCount--;

					goto Repeat;
				}

				onAnimatedUpdateStage.Update(0);
				if (
					behaviorSystem.HasStartPendingBehaviors() ||
					pendingSystem.PendingCustomUpdates[(int) NodeManagerPhase.PendingPreEarlyUpdate].Count > 0 ||
					pendingSystem.PendingCustomUpdates[(int) NodeManagerPhase.PendingEarlyUpdate].Count > 0 ||
					pendingSystem.PendingCustomUpdates[(int) NodeManagerPhase.PendingPostEarlyUpdate].Count > 0 ||
					pendingSystem.PendingTasksUpdate.Count > 0 ||
					pendingSystem.PendingAdvanceAnimation.Count > 0
				) {
					goto Repeat;
				}

				if (ProcessCustomUpdates((int) NodeManagerPhase.PendingOnAnimated)) {
					goto Repeat;
				}

				if (animationSystem.ConsumePendingStoppedActions()) {
					goto Repeat;
				}

				if (ProcessCustomUpdates((int) NodeManagerPhase.PendingPreLateUpdate)) {
					goto Repeat;
				}

				if (ProcessCustomUpdates((int) NodeManagerPhase.PendingLateUpdate)) {
					goto Repeat;
				}

				if (ProcessCustomUpdates((int) NodeManagerPhase.PendingPostLateUpdate)) {
					goto Repeat;
				}

				bool ProcessCustomUpdates(int updatePhase)
				{
					var customUpdatesList = pendingSystem.PendingCustomUpdates[updatePhase];

					if (customUpdatesList.Count <= 0) {
						return false;
					}

					while (customUpdatesList.Count > 0) {
						var customUpdate = customUpdatesList.First.Value;
						customUpdatesList.RemoveFirst();
						customUpdate(0);
					}

					return true;
				}
			}
		}
	}
}
