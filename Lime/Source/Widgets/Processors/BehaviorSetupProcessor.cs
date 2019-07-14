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

		protected override void Freeze(BehaviorComponent component)
		{
			OnFilterChanged(component);
		}

		protected override void Unfreeze(BehaviorComponent component)
		{
			OnFilterChanged(component);
		}

		private void OnFilterChanged(BehaviorComponent component)
		{
			behaviorSystem.OnFilterChanged(ub);
		}

		protected internal override void Update(float delta)
		{
			behaviorSystem.StartPendingBehaviors();
		}
	}
}
