using System;

namespace Lime.KGDCitronLifeCycle
{
	public static partial class CitronLifeCycle
	{
		private static bool initialized;
		[ThreadStatic]
		private static int automateAdvanceAnimationActiveCount;

		public static void Initialize()
		{
			if (initialized) {
				return;
			}
			initialized = true;

			Lime.DefaultAnimationEngine.Instance = new DefaultAnimationEngine();
			Lime.BlendAnimationEngine.Instance = new BlendAnimationEngine();
			WindowWidget.NodeManagerFactory = CreateNodeManager;
		}

		public static Action OnStart(this Node node, Action<float> customUpdate, NodeManagerPhase customUpdatePhase)
		{
			var nodeManager = node?.Manager;
			if (nodeManager == null) {
				return null;
			}

			var managerPhase = nodeManager.GetPhase();
			if (managerPhase == NodeManagerPhase.None) {
				managerNoneStateCase.FixLog();
				return null;
			}

			var pendingSystem = nodeManager.ServiceProvider.GetService<PendingSystem>();

			bool updatePended = false;
			bool tasksPended = false;
			bool advanceAnimationPended = false;

			if (managerPhase >= customUpdatePhase && customUpdate != null) {
				updateOnStartCase.FixLog(customUpdatePhase + " -> " + node);
				pendingSystem.PendingCustomUpdates.AddLast(customUpdate);
				updatePended = true;
			}
			if (managerPhase >= NodeManagerPhase.EarlyUpdate) {
				tasksUpdateOnStartCase.FixLog(node.ToString());
				pendingSystem.PendingTasksUpdate.AddLast(node);
				tasksPended = true;
			}
			if (managerPhase >= NodeManagerPhase.Animation) {
				advanceAnimationsRecursiveOnStartCase.FixLog(node.ToString());
				pendingSystem.PendingAdvanceAnimation.AddLast(node);
				advanceAnimationPended = true;
			}

			return () => {
				if (updatePended) {
					pendingSystem.PendingCustomUpdates.Remove(customUpdate);
				}
				if (tasksPended) {
					pendingSystem.PendingTasksUpdate.Remove(node);
				}
				if (advanceAnimationPended) {
					pendingSystem.PendingAdvanceAnimation.Remove(node);
				}
			};
		}

		private static void ProcessAfterRunAnimation(this Animation animation)
		{
			if (automateAdvanceAnimationActiveCount > 0) {
				return;
			}

			var ownerNode = animation.OwnerNode;
			var nodeManager = ownerNode?.Manager;

			if (nodeManager == null) {
				return;
			}

			var managerPhase = nodeManager.GetPhase();

			if (managerPhase == NodeManagerPhase.None) {
				managerNoneStateCase.FixLog();
				return;
			}

			if (managerPhase > NodeManagerPhase.Animation) {
				advanceAnimationsRecursiveAfterRunAnimationCase.FixLog();

				var pendingSystem = nodeManager.ServiceProvider.GetService<PendingSystem>();
				pendingSystem.PendingAdvanceAnimation.AddLast(ownerNode);
			}
		}

		private static void RaiseStopped(Animation animation)
		{
			AnimationSystem animationSystem;
			if (animation.Stopped != null) {
				if ((
					animationSystem = animation.OwnerNode.Manager?.ServiceProvider.GetService<AnimationSystem>()
				) == null) {
					immediatelyOnStoppedCase.FixLog();
					animation.Stopped();
				} else {
					animationSystem.OnAnimationStoppedRisen(animation.Stopped);
				}
			}

			var savedAction = animation.AssuredStopped;
			animation.AssuredStopped = null;
			if (savedAction != null) {
				if ((
					animationSystem = animation.OwnerNode.Manager?.ServiceProvider.GetService<AnimationSystem>()
				) == null) {
					immediatelyOnStoppedCase.FixLog();
					savedAction();
				} else {
					animationSystem.OnAnimationStoppedRisen(savedAction);
				}
			}
		}
	}
}
