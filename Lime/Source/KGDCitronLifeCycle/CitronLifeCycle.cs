using System;
using System.Collections.Generic;
using System.Linq;

namespace Lime.KGDCitronLifeCycle
{
	public static partial class CitronLifeCycle
	{
		private static bool initialized;
		private static Action<Node, float> savedAdvanceAnimationsRecursive;
		[ThreadStatic]
		private static int afterAdvanceAnimationSuppressedCount;

		public static void Initialize()
		{
			if (initialized) {
				return;
			}
			initialized = true;

			Lime.DefaultAnimationEngine.Instance = new DefaultAnimationEngine();
			Lime.BlendAnimationEngine.Instance = new BlendAnimationEngine();
			savedAdvanceAnimationsRecursive = NodeCompatibilityExtensions.AdvanceAnimationsRecursiveHook;
			NodeCompatibilityExtensions.AdvanceAnimationsRecursiveHook = AdvanceAnimationsRecursive;
			WindowWidget.NodeManagerFactory = CreateNodeManager;
		}

		public static void OnStart(this Node node, Action<float> customUpdate, NodeManagerPhase customUpdatePhase)
		{
			var nodeManager = node?.Manager;
			if (nodeManager == null) {
				return;
			}

			var managerPhase = nodeManager.GetPhase();
			if (managerPhase == NodeManagerPhase.None) {
				managerNoneStateCase.FixLog();
				return;
			}

			if (
				managerPhase >= customUpdatePhase &&
				customUpdatePhase <= NodeManagerPhase.EarlyUpdate
			) {
				updateOnStartCase.FixLog(node.ToString());
				customUpdate?.Invoke(0);
			}
			if (managerPhase >= NodeManagerPhase.EarlyUpdate) {
				tasksUpdateOnStartCase.FixLog(node.ToString());
				node.Tasks.Update(0);
			}
			if (
				managerPhase >= customUpdatePhase &&
				customUpdatePhase > NodeManagerPhase.EarlyUpdate &&
				customUpdatePhase < NodeManagerPhase.Animation
			) {
				updateOnStartCase.FixLog(node.ToString());
				customUpdate?.Invoke(0);
			}
			if (managerPhase >= NodeManagerPhase.Animation) {
				advanceAnimationsRecursiveOnStartCase.FixLog(node.ToString());
				node.AdvanceAnimationsRecursive(0);
			}
			if (
				managerPhase >= customUpdatePhase &&
				customUpdatePhase > NodeManagerPhase.Animation
			) {
				updateOnStartCase.FixLog(node.ToString());
				customUpdate?.Invoke(0);
			}
		}

		private static void AdvanceAnimationsRecursive(Node node, float delta)
		{
			if (afterAdvanceAnimationSuppressedCount > 0) {
				throw new System.Exception("Nested AdvanceAnimationsRecursive call");
			}

			afterAdvanceAnimationSuppressedCount++;
			try {
				savedAdvanceAnimationsRecursive(node, delta);
			} finally {
				afterAdvanceAnimationSuppressedCount--;
			}

			node.ProcessAfterAdvanceAnimation();
		}

		private static void ProcessAfterAdvanceAnimation(this Node ownerNode)
		{
			if (afterAdvanceAnimationSuppressedCount > 0) {
				return;
			}

			var nodeManager = ownerNode?.Manager;
			if (nodeManager != null) {
				var managerPhase = nodeManager.GetPhase();

				if (managerPhase == NodeManagerPhase.None) {
					managerNoneStateCase.FixLog();
					return;
				}

				if (managerPhase >= NodeManagerPhase.OnAnimationStopped) {
					onStoppedAfterAdvanceAnimationCase.FixLog();
					var animationSystem = nodeManager.ServiceProvider.RequireService<AnimationSystem>();

					animationSystem.ConsumePendingActions();
				}
				if (managerPhase >= NodeManagerPhase.AfterAnimation) {
					onAnimatedAfterAdvanceAnimationCase.FixLog();
					var postAnimationProcessor = nodeManager.Processors.OfType<PostAnimationProcessor>().First();
					postAnimationProcessor.UpdateManually();
				}
			}
		}

		private static void ProcessAfterRunAnimation(this Animation animation)
		{
			var ownerNode = animation.OwnerNode;
			var nodeManager = ownerNode?.Manager;
			if (nodeManager != null) {
				var managerPhase = nodeManager.GetPhase();

				if (managerPhase == NodeManagerPhase.None) {
					managerNoneStateCase.FixLog();
					return;
				}

				if (managerPhase > NodeManagerPhase.Animation && afterAdvanceAnimationSuppressedCount == 0) {
					advanceAnimationsRecursiveAfterRunAnimationCase.FixLog();
					ownerNode.AdvanceAnimationsRecursive(0);
				}
			}
		}

		private static void RaiseStopped(Animation animation)
		{
			AnimationSystem animationSystem;
			if (animation.Stopped != null) {
				if ((
					animationSystem = animation.OwnerNode.Manager.ServiceProvider.GetService<AnimationSystem>()
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
					animationSystem = animation.OwnerNode.Manager.ServiceProvider.GetService<AnimationSystem>()
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
