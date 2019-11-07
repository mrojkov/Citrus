namespace Lime.KGDCitronLifeCycle
{
	public static partial class CitronLifeCycle
	{
		private class PostAnimationProcessor : NodeProcessor
		{
			private bool updateRepeatRequired;
			private bool updateActive;

			private BehaviorSystem behaviorSystem;
			private BehaviorUpdateStage updateStage;

			protected internal override void Start()
			{
				behaviorSystem = Manager.ServiceProvider.RequireService<BehaviorSystem>();
				updateStage = behaviorSystem.GetUpdateStage(typeof(AfterAnimationStage));
			}

			protected internal override void Stop()
			{
				behaviorSystem = null;
				updateStage = null;
			}

			protected internal override void Update(float delta)
			{
				if (updateActive) {
					// ReSharper disable once CompareOfFloatsByEqualityOperator
					if (delta != 0) {
						throw new System.Exception("Nested Update with not zero delta is not allowed");
					}
					updateRepeatRequired = true;
					return;
				}

				behaviorSystem.StartPendingBehaviors();
				updateActive = true;
				try {
					do {
						updateRepeatRequired = false;
						updateStage.Update(delta);
						delta = 0;
					} while (updateRepeatRequired);
				} finally {
					updateActive = false;
				}
				behaviorSystem.StartPendingBehaviors();
			}

			public void UpdateManually()
			{
				if (updateActive) {
					updateRepeatRequired = true;
					return;
				}

				behaviorSystem.StartPendingBehaviors();
				updateActive = true;
				try {
					do {
						updateRepeatRequired = false;
						updateStage.Update(0);
					} while (updateRepeatRequired);
				} finally {
					updateActive = false;
				}
				behaviorSystem.StartPendingBehaviors();
			}
		}
	}
}
