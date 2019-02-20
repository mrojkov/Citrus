namespace Lime.KineticMotionStrategy
{
	/// <summary>
	/// Defines a kinetic movement strategy.
	/// </summary>
	public interface IKineticMotionStrategy
	{
		Vector2 Position { get; }
		bool InProgress { get; }

		/// <summary>
		/// Start position calculation.
		/// </summary>
		/// <param name="startPosition">The position in which the movement began.</param>
		/// <param name="startDirection">The direction in which the movement began.</param>
		/// <param name="startSpeed">The initial speed of movement.</param>
		void Start(in Vector2 startPosition, in Vector2 startDirection, float startSpeed);

		/// <summary>
		/// Recalculates position.
		/// </summary>
		/// <param name="delta">Elapsed time.</param>
		void Update(float delta);

		/// <summary>
		/// Stops position calculation.
		/// </summary>
		void Stop();
	}
}
