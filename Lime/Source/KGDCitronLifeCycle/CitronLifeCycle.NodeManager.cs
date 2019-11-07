using System;
using System.Reflection;

namespace Lime.KGDCitronLifeCycle
{
	public static partial class CitronLifeCycle
	{
		public enum NodeManagerPhase
		{
			None,
			Gesture,
			BehaviorSetup,
			PreEarlyUpdate,
			EarlyUpdate,
			PostEarlyUpdate,
			Animation,
			OnAnimationStopped,
			AfterAnimation,
			Layout,
			BoundingRect,
			PreLateUpdate,
			LateUpdate,
			PostLateUpdate
		}

		private static NodeManager CreateNodeManager(LayoutManager layoutManager, WidgetContext widgetContext)
		{
			var services = new ServiceRegistry();
			services.Add(new BehaviorSystem());
			services.Add(new AnimationSystem());
			services.Add(layoutManager);
			services.Add(widgetContext);

			var manager = new NodeManager(services);
			manager.Processors.Add(new GestureProcessor());
			manager.Processors.Add(new BehaviorSetupProcessor());
			manager.Processors.Add(new BehaviorUpdateProcessor(typeof(PreEarlyUpdateStage)));
			manager.Processors.Add(new BehaviorUpdateProcessor(typeof(EarlyUpdateStage)));
			manager.Processors.Add(new BehaviorUpdateProcessor(typeof(PostEarlyUpdateStage)));
			manager.Processors.Add(new AnimationProcessor());
			manager.Processors.Add(new AnimationStoppedProcessor());
			manager.Processors.Add(new PostAnimationProcessor());
			manager.Processors.Add(new LayoutProcessor());
			manager.Processors.Add(new BoundingRectProcessor());
			manager.Processors.Add(new BehaviorUpdateProcessor(typeof(PreLateUpdateStage)));
			manager.Processors.Add(new BehaviorUpdateProcessor(typeof(LateUpdateStage)));
			manager.Processors.Add(new BehaviorUpdateProcessor(typeof(PostLateUpdateStage)));
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
			if (updateStageType == typeof(AfterAnimationStage)) {
				return NodeManagerPhase.AfterAnimation;
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
			return NodeManagerPhase.None;
		}

		private static NodeManagerPhase GetPhase(this NodeManager manager)
		{
			// WARNING Fragile code!
			var activeProcessor = manager.ActiveProcessor;
			switch (activeProcessor) {
				case BehaviorUpdateProcessor updateProcessor:
					return ConvertUpdateStageTypeToNodeManagerPhase(updateProcessor.UpdateStageType);
				case GestureProcessor _:
					return NodeManagerPhase.Gesture;
				case BehaviorSetupProcessor _:
					return NodeManagerPhase.BehaviorSetup;
				case AnimationProcessor _:
					return NodeManagerPhase.Animation;
				case PostAnimationProcessor _:
					return NodeManagerPhase.AfterAnimation;
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
