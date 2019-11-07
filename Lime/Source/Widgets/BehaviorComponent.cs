using System;
using System.Collections.Generic;

namespace Lime
{
	/// <summary>
	/// BehaviorComponent is NodeComponent that used for scripting node behavior.
	/// </summary>
	public class BehaviorComponent : NodeComponent
	{
		internal LinkedListNode<BehaviorComponent> StartQueueNode;
		internal BehaviorUpdateFamily UpdateFamily;
		internal int IndexInUpdateFamily = -1;

		/// <summary>
		/// Determines if behavior is suspended.
		/// </summary>
		protected internal bool Suspended { get; private set; }

		/// <summary>
		/// Start is called on the frame in which the component had added just before Update method is called first time.
		/// Note: Start doesn't called immediately at the time it is adding.
		/// </summary>
		protected internal virtual void Start() { }

		/// <summary>
		/// Stop is called immediately after component had removed
		/// </summary>
		/// <param name="owner">Previous owner node.</param>
		protected internal virtual void Stop(Node owner) { }

		/// <summary>
		/// Update is called on every frame if the owner node isn't frozen and the behavior isn't suspended.
		/// </summary>
		/// <param name="delta">Elapsed time between frames.</param>
		protected internal virtual void Update(float delta) { }

		/// <summary>
		/// Occurs when the GloballyFrozen property of the owner node changes.
		/// </summary>
		protected internal virtual void OnOwnerFrozenChanged() { }

		/// <summary>
		/// Stops the Update calls.
		/// </summary>
		public void Suspend()
		{
			if (!Suspended) {
				Suspended = true;
				UpdateFamily.Filter(this);
			}
		}

		/// <summary>
		/// Resumes the Update calls.
		/// </summary>
		public void Resume()
		{
			if (Suspended) {
				Suspended = false;
				UpdateFamily.Filter(this);
			}
		}
	}

	/// <summary>
	/// Identifier for pre-early update stage.
	/// </summary>
	public class PreEarlyUpdateStage { }

	/// <summary>
	/// Identifier for early update stage.
	/// </summary>
	public class EarlyUpdateStage { }

	/// <summary>
	/// Identifier for post-early update stage.
	/// </summary>
	public class PostEarlyUpdateStage { }

	/// <summary>
	/// Identifier for after advance animation update stage.
	/// </summary>
	public class AfterAnimationStage { }

	/// <summary>
	/// Identifier for pre-late update stage.
	/// </summary>
	public class PreLateUpdateStage { }

	/// <summary>
	/// Identifier for late update stage.
	/// </summary>
	public class LateUpdateStage { }

	/// <summary>
	/// Identifier for post-late update stage.
	/// </summary>
	public class PostLateUpdateStage { }

	/// <summary>
	/// Specifies update stage for BehaviorComponent.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class UpdateStageAttribute : Attribute
	{
		/// <summary>
		/// Update stage identifier.
		/// </summary>
		public Type StageType { get; }

		/// <summary>
		/// Initializes a new instance of UpdateStageAttribute with an update stage identifier.
		/// </summary>
		/// <param name="stageType">An identifier of update stage.</param>
		public UpdateStageAttribute(Type stageType)
		{
			StageType = stageType;
		}
	}

	/// <summary>
	/// Defines dependencies between behaviors to control update order inside stage.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class UpdateAfterBehaviorAttribute : Attribute
	{
		/// <summary>
		/// The behavior type that target behavior depenent on.
		/// </summary>
		public Type BehaviorType { get; }

		/// <summary>
		/// Initializes a new instance of UpdateAfterBehaviorAttribute.
		/// </summary>
		/// <param name="behaviorType">A behavior type that target behavior dependent on.</param>
		public UpdateAfterBehaviorAttribute(Type behaviorType)
		{
			BehaviorType = behaviorType;
		}
	}

	/// <summary>
	/// Defines dependencies between behaviors to control update order inside stage.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class UpdateBeforeBehaviorAttribute : Attribute
	{
		/// <summary>
		/// The behavior type that target behavior depenent on.
		/// </summary>
		public Type BehaviorType { get; }

		/// <summary>
		/// Initializes a new instance of UpdateBeforeBehaviorAttribute.
		/// </summary>
		/// <param name="behaviorType">A behavior type that target behavior dependent on.</param>
		public UpdateBeforeBehaviorAttribute(Type behaviorType)
		{
			BehaviorType = behaviorType;
		}
	}

	/// <summary>
	/// Specifies that behavior should be updated even if node is frozen.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class UpdateFrozenAttribute : Attribute
	{
	}
}
