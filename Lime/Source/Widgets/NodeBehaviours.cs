using System;

namespace Lime
{
	[NodeComponentDontSerialize]
	public class AwakeBehavior : BehaviourComponent
	{
		public event Action<Node> Action;

		protected internal override void Start()
		{
			Action?.Invoke(Owner);
		}
	}

	[NodeComponentDontSerialize]
	public class UpdateBehaviour : BehaviourComponent
	{
		public UpdateHandler Updating;

		protected internal override void Update(float delta) => Updating?.Invoke(delta);

		public override NodeComponent Clone()
		{
			var clone = (UpdateBehaviour)base.Clone();
			clone.Updating = null;
			return clone;
		}
	}

	[NodeComponentDontSerialize]
	[UpdateAfterBehaviour(typeof(UpdateBehaviour))]
	public class TasksBehaviour : BehaviourComponent
	{
		public TaskList Tasks { get; private set; }

		protected internal override void Update(float delta) => Tasks.Update(delta);

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
	[UpdateAfterBehaviour(typeof(TasksBehaviour))]
	public class AnimationBehaviour : BehaviourComponent
	{
		internal int RunningAnimationCount;

		public AnimationCollection Animations { get; private set; }

		public Animation DefaultAnimation
		{
			get {
				foreach (var a in Animations) {
					if (a.IsLegacy) {
						return a;
					}
				}
				var newAnimation = new Animation() { IsLegacy = true };
				Animations.Add(newAnimation);
				return newAnimation;
			}
		}

		public AnimationBehaviour()
		{
			Animations = new AnimationCollection(this);
		}

		protected internal override void Update(float delta)
		{
			if (RunningAnimationCount > 0) {
				foreach (var a in Animations) {
					a.Advance(delta);
				}
			}
		}

		public override NodeComponent Clone()
		{
			var clone = (AnimationBehaviour)base.Clone();
			clone.RunningAnimationCount = 0;
			clone.Animations = new AnimationCollection(clone);
			foreach (var a in Animations) {
				clone.Animations.Add(a.Clone());
			}
			return clone;
		}
	}

	[NodeComponentDontSerialize]
	[UpdateAfterBehaviour(typeof(AnimationBehaviour))]
	public class LateTasksBehaviour : BehaviourComponent
	{
		public TaskList Tasks { get; private set; }

		protected internal override void Update(float delta) => Tasks.Update(delta);

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

	[NodeComponentDontSerialize]
	[UpdateAfterBehaviour(typeof(LateTasksBehaviour))]
	public class UpdatedBehaviour : BehaviourComponent
	{
		public UpdateHandler Updated;

		protected internal override void Update(float delta) => Updated?.Invoke(delta);

		public override NodeComponent Clone()
		{
			var clone = (UpdatedBehaviour)base.Clone();
			clone.Updated = null;
			return clone;
		}
	}
}
