using System;

namespace Lime
{
	public class BehaviorSetupProcessor : NodeComponentProcessor<BehaviorComponent>
	{
		private BehaviorSystem behaviorSystem;

		public override void Start()
		{
			behaviorSystem = Manager.ServiceProvider.RequireService<BehaviorSystem>();
		}

		public override void Stop(NodeManager manager)
		{
			behaviorSystem = null;
		}

		protected override void Add(BehaviorComponent component, Node owner)
		{
			behaviorSystem.Add(component);
		}

		protected override void Remove(BehaviorComponent component, Node owner)
		{
			behaviorSystem.Remove(component, owner);
		}

		protected override void OnOwnerFrozenChanged(BehaviorComponent component, Node owner)
		{
			behaviorSystem.OnOwnerFrozenChanged(component);
		}
	}
}
