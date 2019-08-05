using System;

namespace Lime
{
	public class BehaviorSetupProcessor : NodeComponentProcessor<BehaviorComponent>
	{
		private BehaviorSystem behaviorSystem;

		protected internal override void Start()
		{
			behaviorSystem = Manager.ServiceProvider.RequireService<BehaviorSystem>();
		}

		protected internal override void Stop()
		{
			behaviorSystem = null;
		}

		protected override void Add(BehaviorComponent component)
		{
			behaviorSystem.Add(component);
		}

		protected override void Remove(BehaviorComponent component)
		{
			behaviorSystem.Remove(component);
		}

		protected override void OnOwnerFrozenChanged(BehaviorComponent component)
		{
			behaviorSystem.OnOwnerFrozenChanged(component);
		}

		protected internal override void Update(float delta)
		{
			behaviorSystem.StartPendingBehaviors();
		}
	}
}
