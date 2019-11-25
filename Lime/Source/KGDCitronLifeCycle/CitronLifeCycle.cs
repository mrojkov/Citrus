using System;

namespace Lime.KGDCitronLifeCycle
{
	public static partial class CitronLifeCycle
	{
		private static bool initialized;
		[ThreadStatic]
		private static int advanceAnimationActiveCount;
		[ThreadStatic]
		private static int pipelinedAdvanceAnimationActiveCount;

		public static void Initialize()
		{
			initialized = true;
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
				pendingSystem.PendingCustomUpdates[(int) customUpdatePhase].AddLast(customUpdate);
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
					pendingSystem.PendingCustomUpdates[(int) customUpdatePhase].Remove(customUpdate);
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
			if (advanceAnimationActiveCount > 0) {
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

			if (managerPhase >= NodeManagerPhase.Layout) {
				runAnimationDuringUpdatedPhaseCase.FixLog();
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
					if (pipelinedAdvanceAnimationActiveCount > 0) {
						animationSystem.OnAnimationStoppedRisen(animation.Stopped);
					} else {
						// Пусть события, которые вызываны ручной прокруткой анимации, срабатывают сразу.
						// В целом это не очень прозрачно, но так было всегда.
						// Если захочется изменить, то надо выпилить полность поле pipelinedAdvanceAnimationActiveCount.
						animation.Stopped();
					}
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
					if (pipelinedAdvanceAnimationActiveCount > 0) {
						animationSystem.OnAnimationStoppedRisen(savedAction);
					} else {
						// Пусть события, которые вызываны ручной прокруткой анимации, срабатывают сразу.
						// В целом это не очень прозрачно, но так было всегда.
						// Если захочется изменить, то надо выпилить полность поле pipelinedAdvanceAnimationActiveCount.
						savedAction();
					}
				}
			}
		}
	}
}
