namespace Lime.KineticMotionStrategy
{
	/// <summary>
	/// Uniformly slow motion, occurring within a specified time.
	/// </summary>
	public class FixedTimeKineticMotionStrategy : IKineticMotionStrategy
	{
		private readonly float duration;

		private Vector2 fromPosition;
		private Vector2 direction;
		private float speed;
		private float deceleration;
		private float timeElapsed;

		public Vector2 Position { get; private set; }
		public bool InProgress { get; private set; }

		/// <param name="kineticDuration">The time during which the movement slows down.</param>
		public FixedTimeKineticMotionStrategy(float kineticDuration)
		{
			duration = kineticDuration;
		}

		public virtual void Start(in Vector2 startPosition, in Vector2 startDirection, float startSpeed)
		{
			fromPosition = startPosition;
			direction = startDirection.Normalized;
			speed = startSpeed;
			deceleration = -startSpeed / duration;
			timeElapsed = 0.0f;
			Position = startPosition;
			InProgress = true;
		}

		public void Update(float delta)
		{
			if (!InProgress) {
				return;
			}
			timeElapsed += delta;
			Position = fromPosition + direction *
				(speed * timeElapsed + deceleration * timeElapsed.Sqr() / 2.0f);
			InProgress = timeElapsed < duration;
		}

		public void Stop()
		{
			InProgress = false;
		}
	}
}
