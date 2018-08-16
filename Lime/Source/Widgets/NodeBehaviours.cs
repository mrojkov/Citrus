using System;

namespace Lime
{
	[NodeComponentDontSerialize]
	public class AwakeBehavior : NodeBehavior
	{
		public event Action<Node> Action;

		public override int Order => -1000100;

		public bool IsAwoken { get; private set; }

		public override void Update(float delta)
		{
			if (IsAwoken) {
				return;
			}
			Action?.Invoke(Owner);
			IsAwoken = true;
		}

		protected override void OnOwnerChanged(Node oldOwner)
		{
			base.OnOwnerChanged(oldOwner);
			IsAwoken = false;
		}

		public override NodeComponent Clone()
		{
			var clone = (AwakeBehavior)base.Clone();
			clone.IsAwoken = false;
			return clone;
		}
	}

	[NodeComponentDontSerialize]
	internal class UpdateBehaviour : NodeBehavior
	{
		public UpdateHandler Updating;

		public override void Update(float delta) => Updating?.Invoke(delta);
		public override int Order => -1000000;

		public override NodeComponent Clone()
		{
			var clone = (UpdateBehaviour)base.Clone();
			clone.Updating = null;
			return clone;
		}
	}

	[NodeComponentDontSerialize]
	internal class UpdatedBehaviour : NodeBehavior
	{
		public UpdateHandler Updated;

		public override void LateUpdate(float delta) => Updated?.Invoke(delta);
		public override int Order => 1000000;

		public override NodeComponent Clone()
		{
			var clone = (UpdatedBehaviour)base.Clone();
			clone.Updated = null;
			return clone;
		}
	}

	[NodeComponentDontSerialize]
	internal class TasksBehaviour : NodeBehavior
	{
		public TaskList Tasks { get; private set; }

		public override void Update(float delta) => Tasks.Update(delta);
		public override int Order => -900000;

		protected override void OnOwnerChanged(Node oldOwner)
		{
			Tasks?.Stop();
			Tasks = Owner == null ? null : new TaskList(Owner);
		}

		public override NodeComponent Clone()
		{
			var clone = (TasksBehaviour)base.Clone();
			clone.Tasks = null;
			return clone;
		}

		public override void Dispose()
		{
			Tasks?.Stop();
		}
	}

	[NodeComponentDontSerialize]
	internal class LateTasksBehaviour : NodeBehavior
	{
		public TaskList Tasks { get; private set; }

		public override void LateUpdate(float delta) => Tasks.Update(delta);
		public override int Order => 900000;

		protected override void OnOwnerChanged(Node oldOwner)
		{
			Tasks?.Stop();
			Tasks = Owner == null ? null : new TaskList(Owner);
		}

		public override NodeComponent Clone()
		{
			var clone = (LateTasksBehaviour)base.Clone();
			clone.Tasks = null;
			return clone;
		}

		public override void Dispose()
		{
			Tasks?.Stop();
		}
	}
}
