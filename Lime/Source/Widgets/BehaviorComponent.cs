using System;
using System.Collections.Generic;

namespace Lime
{
	public class BehaviorComponent : NodeComponent
	{
		internal LinkedListNode<BehaviorComponent> StartQueueNode;
		internal BehaviorUpdateFamily UpdateFamily;
		internal int IndexInUpdateFamily = -1;

		protected internal bool Suspended { get; private set; }

		protected internal virtual void Start() { }

		protected internal virtual void Stop() { }

		protected internal virtual void Update(float delta) { }

		protected internal virtual void OnOwnerFrozenChanged() { }

		protected void Suspend()
		{
			Suspended = true;
			UpdateFamily.Filter(this);
		}

		public void Resume()
		{
			Suspended = false;
			UpdateFamily.Filter(this);
		}

		public override NodeComponent Clone()
		{
			var clone = (BehaviorComponent)base.Clone();
			clone.StartQueueNode = null;
			clone.Suspended = false;
			clone.UpdateFamily = null;
			clone.IndexInUpdateFamily = -1;
			return clone;
		}
	}

	public class PreEarlyUpdateStage { }

	public class EarlyUpdateStage { }

	public class PostEarlyUpdateStage { }

	public class PreLateUpdateStage { }

	public class LateUpdateStage { }

	public class PostLateUpdateStage { }

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class UpdateStageAttribute : Attribute
	{
		public Type StageType { get; }

		public UpdateStageAttribute(Type stageType)
		{
			StageType = stageType;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class UpdateAfterBehaviorAttribute : Attribute
	{
		public Type BehaviorType { get; }

		public UpdateAfterBehaviorAttribute(Type behaviorType)
		{
			BehaviorType = behaviorType;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class UpdateBeforeBehaviorAttribute : Attribute
	{
		public Type BehaviorType { get; }

		public UpdateBeforeBehaviorAttribute(Type behaviorType)
		{
			BehaviorType = behaviorType;
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class UpdateFrozenAttribute : Attribute
	{
	}
}
