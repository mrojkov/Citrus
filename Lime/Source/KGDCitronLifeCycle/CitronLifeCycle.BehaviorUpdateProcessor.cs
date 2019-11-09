using System;

namespace Lime.KGDCitronLifeCycle
{
	public static partial class CitronLifeCycle
	{
		private class BehaviorUpdateProcessor : NodeProcessor
		{
			public readonly Type UpdateStageType;

			private BehaviorSystem behaviorSystem;
			private BehaviorUpdateStage updateStage;

			public BehaviorUpdateProcessor(Type updateStageType)
			{
				UpdateStageType = updateStageType;
			}

			protected internal override void Start()
			{
				behaviorSystem = Manager.ServiceProvider.RequireService<BehaviorSystem>();
				updateStage = behaviorSystem.GetUpdateStage(UpdateStageType);
			}

			protected internal override void Stop()
			{
				updateStage = null;
				behaviorSystem = null;
			}

			protected internal override void Update(float delta)
			{
				updateStage.Update(delta);
			}
		}
	}
}
