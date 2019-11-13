using System;

namespace Lime.KGDCitronLifeCycle
{
	public static partial class CitronLifeCycle
	{
		/// <summary>
		/// Отличается от стандартного тем, что не дергает behaviorSystem.StartPendingBehaviors,
		/// а этим занимется PendingProcessor.
		/// </summary>
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

			protected internal override void Stop(NodeManager manager)
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
