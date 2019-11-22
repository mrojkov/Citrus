using System;
using System.Reflection;

namespace Lime.KGDCitronLifeCycle
{
	public static partial class CitronLifeCycle
	{
		/// <summary>
		/// Identifier for post-post-late update stage.
		/// </summary>

		public class PreAfterAnimationStage { }
		public class PostAfterAnimationStage { }
		public class PostPostLateUpdateStage { }

		public enum NodeManagerPhase
		{
			None = 0,
			Gesture,
			BehaviorSetup,
			PendingBehaviorSetup,
			PreEarlyUpdate,
			PendingPreEarlyUpdate,
			EarlyUpdate,
			PendingEarlyUpdate,
			PostEarlyUpdate,
			PendingPostEarlyUpdate,
			Animation,
			PreOnAnimated,
			PendingPreOnAnimated,
			OnAnimated,
			PendingOnAnimated,
			PostOnAnimated,
			PendingPostOnAnimated,
			OnAnimationStopped,
			PendingOnAnimationStopped,
			Layout,
			BoundingRect,
			PreLateUpdate,
			PendingPreLateUpdate,
			LateUpdate,
			PendingLateUpdate,
			PostLateUpdate,
			PendingPostLateUpdate,
			PostPostLateUpdate,
			PendingPostPostLateUpdate,
		}

		private static NodeManager CreateNodeManager(LayoutManager layoutManager, WidgetContext widgetContext)
		{
			var services = new ServiceRegistry();
			services.Add(new BehaviorSystem());
			services.Add(new AnimationSystem());
			services.Add(new PendingSystem(Enum.GetValues(typeof(NodeManagerPhase)).Length));
			services.Add(layoutManager);
			services.Add(widgetContext);

			// Самое главное - это свой BehaviorUpdateProcessor, и добавленный везде BehaviorUpdateProcessor,
			// который длеает очень много работы, чтобы восстановить жизненный цикл в рамках одного кадра.
			var manager = new NodeManager(services);
			manager.Processors.Add(new GestureProcessor());
			manager.Processors.Add(new BehaviorSetupProcessor());
			manager.Processors.Add(new PendingProcessor(NodeManagerPhase.PendingBehaviorSetup));
			manager.Processors.Add(new BehaviorUpdateProcessor(typeof(PreEarlyUpdateStage)));
			manager.Processors.Add(new PendingProcessor(NodeManagerPhase.PendingPreEarlyUpdate));
			manager.Processors.Add(new BehaviorUpdateProcessor(typeof(EarlyUpdateStage)));
			manager.Processors.Add(new PendingProcessor(NodeManagerPhase.PendingEarlyUpdate));
			manager.Processors.Add(new BehaviorUpdateProcessor(typeof(PostEarlyUpdateStage)));
			manager.Processors.Add(new PendingProcessor(NodeManagerPhase.PendingPostEarlyUpdate));
			manager.Processors.Add(new AnimationProcessor());
			manager.Processors.Add(new BehaviorUpdateProcessor(typeof(PreAfterAnimationStage)));
			manager.Processors.Add(new PendingProcessor(NodeManagerPhase.PendingPreOnAnimated));
			manager.Processors.Add(new BehaviorUpdateProcessor(typeof(AfterAnimationStage)));
			manager.Processors.Add(new PendingProcessor(NodeManagerPhase.PendingOnAnimated));
			manager.Processors.Add(new BehaviorUpdateProcessor(typeof(PostAfterAnimationStage)));
			manager.Processors.Add(new PendingProcessor(NodeManagerPhase.PendingPostOnAnimated));
			manager.Processors.Add(new AnimationStoppedProcessor());
			manager.Processors.Add(new PendingProcessor(NodeManagerPhase.PendingOnAnimationStopped));
			manager.Processors.Add(new LayoutProcessor());
			manager.Processors.Add(new BoundingRectProcessor());
			manager.Processors.Add(new BehaviorUpdateProcessor(typeof(PreLateUpdateStage)));
			manager.Processors.Add(new PendingProcessor(NodeManagerPhase.PendingPreLateUpdate));
			manager.Processors.Add(new BehaviorUpdateProcessor(typeof(LateUpdateStage)));
			manager.Processors.Add(new PendingProcessor(NodeManagerPhase.PendingLateUpdate));
			manager.Processors.Add(new BehaviorUpdateProcessor(typeof(PostLateUpdateStage)));
			manager.Processors.Add(new PendingProcessor(NodeManagerPhase.PendingPostLateUpdate));
			manager.Processors.Add(new BehaviorUpdateProcessor(typeof(PostPostLateUpdateStage)));
			manager.Processors.Add(new PendingProcessor(NodeManagerPhase.PendingPostPostLateUpdate));
			return manager;
		}

		private static NodeManagerPhase ConvertUpdateStageTypeToNodeManagerPhase(Type updateStageType)
		{
			if (updateStageType == typeof(PreEarlyUpdateStage)) {
				return NodeManagerPhase.PreEarlyUpdate;
			}
			if (updateStageType == typeof(EarlyUpdateStage)) {
				return NodeManagerPhase.EarlyUpdate;
			}
			if (updateStageType == typeof(PostEarlyUpdateStage)) {
				return NodeManagerPhase.PostEarlyUpdate;
			}
			if (updateStageType == typeof(PreAfterAnimationStage)) {
				return NodeManagerPhase.PreOnAnimated;
			}
			if (updateStageType == typeof(AfterAnimationStage)) {
				return NodeManagerPhase.OnAnimated;
			}
			if (updateStageType == typeof(PostAfterAnimationStage)) {
				return NodeManagerPhase.PostOnAnimated;
			}
			if (updateStageType == typeof(PreLateUpdateStage)) {
				return NodeManagerPhase.PreLateUpdate;
			}
			if (updateStageType == typeof(LateUpdateStage)) {
				return NodeManagerPhase.LateUpdate;
			}
			if (updateStageType == typeof(PostLateUpdateStage)) {
				return NodeManagerPhase.PostLateUpdate;
			}
			if (updateStageType == typeof(PostPostLateUpdateStage)) {
				return NodeManagerPhase.PostPostLateUpdate;
			}
			return NodeManagerPhase.None;
		}

		private static NodeManagerPhase GetPhase(this NodeManager manager)
		{
			// WARNING Fragile code!
			var activeProcessor = manager.ActiveProcessor;
			switch (activeProcessor) {
				case BehaviorUpdateProcessor updateProcessor:
					return ConvertUpdateStageTypeToNodeManagerPhase(updateProcessor.UpdateStageType);
				case PendingProcessor pendingProcessor:
					return pendingProcessor.ManagerPhase;
				case GestureProcessor _:
					return NodeManagerPhase.Gesture;
				case BehaviorSetupProcessor _:
					return NodeManagerPhase.BehaviorSetup;
				case AnimationProcessor _:
					return NodeManagerPhase.Animation;
				case AnimationStoppedProcessor _:
					return NodeManagerPhase.OnAnimationStopped;
				case LayoutProcessor _:
					return NodeManagerPhase.Layout;
				case BoundingRectProcessor _:
					return NodeManagerPhase.BoundingRect;
			}

			return NodeManagerPhase.None;
		}

		public static NodeManagerPhase CalcNodeManagerPhaseUpdateFor(this BehaviorComponent behaviorComponent)
		{
			var updateStageAttribute = behaviorComponent.GetType().GetCustomAttribute<UpdateStageAttribute>(true);
			if (updateStageAttribute == null) {
				throw new System.Exception("UpdateStageAttribute not found");
			}
			return ConvertUpdateStageTypeToNodeManagerPhase(updateStageAttribute.StageType);
		}
	}
}
