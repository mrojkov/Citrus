using System;

namespace Lime
{
	public class BehaviorUpdateProcessor : NodeProcessor
	{
		private Type updateStageType;
		private BehaviorUpdateStage updateStage;

		public BehaviorUpdateProcessor(Type updateStageType)
		{
			this.updateStageType = updateStageType;
		}

		protected internal override void Start()
		{
			updateStage = Manager.ServiceProvider
				.RequireService<BehaviorSystem>()
				.GetUpdateStage(updateStageType);
		}

		protected internal override void Stop()
		{
			updateStage = null;
		}

		protected internal override void Update(float delta)
		{
			updateStage.Update(delta);
		}
	}
}
