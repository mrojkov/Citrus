using System;

namespace Lime
{
	public class BehaviorUpdateProcessor : NodeProcessor
	{
		private Type updateStageType;

		private BehaviorSystem behaviorSystem;
		private BehaviorUpdateStage updateStage;

		public BehaviorUpdateProcessor(Type updateStageType)
		{
			this.updateStageType = updateStageType;
		}

		protected internal override void Start()
		{
			behaviorSystem = Manager.ServiceProvider.RequireService<BehaviorSystem>();
			updateStage = behaviorSystem.GetUpdateStage(updateStageType);
		}

		protected internal override void Stop()
		{
			updateStage = null;
			behaviorSystem = null;
		}

		protected internal override void Update(float delta)
		{
			behaviorSystem.StartPendingBehaviors();
			updateStage.Update(delta);
			behaviorSystem.StartPendingBehaviors();
		}
	}
}
