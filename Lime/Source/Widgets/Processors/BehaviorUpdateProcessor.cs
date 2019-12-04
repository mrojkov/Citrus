using System;

namespace Lime
{
	public class BehaviorUpdateProcessor : NodeProcessor
	{
		public readonly Type UpdateStageType;

		private BehaviorSystem behaviorSystem;
		private BehaviorUpdateStage updateStage;

		public BehaviorUpdateProcessor(Type updateStageType)
		{
			UpdateStageType = updateStageType;
		}

		public override void Start()
		{
			behaviorSystem = Manager.ServiceProvider.RequireService<BehaviorSystem>();
			updateStage = behaviorSystem.GetUpdateStage(UpdateStageType);
		}

		public override void Stop(NodeManager manager)
		{
			updateStage = null;
			behaviorSystem = null;
		}

		public override void Update(float delta)
		{
			behaviorSystem.StartPendingBehaviors();
			updateStage.Update(delta);
			behaviorSystem.StartPendingBehaviors();
		}
	}
}
