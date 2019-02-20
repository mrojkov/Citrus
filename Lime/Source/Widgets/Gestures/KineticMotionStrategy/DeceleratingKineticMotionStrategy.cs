namespace Lime.KineticMotionStrategy
{
	/// <summary>
	/// Non-uniformly slow motion, occurring to a full stop.
	/// </summary>
	public class DeceleratingKineticMotionStrategy : IKineticMotionStrategy
	{
		private readonly float decelerationFactor;
		private readonly float decelerationIncreaseFactor;
		private readonly float minSpeed;
		private readonly float maxSpeed;

		private Vector2 direction;
		private float speed;
		private float deceleration;

		public Vector2 Position { get; private set; }
		public bool InProgress { get; private set; }

		/// <param name="decelerationFactor">Initial deceleration factor.</param>
		/// <param name="decelerationIncreaseFactor">Increase of the deceleration factor.</param>
		/// <param name="minSpeed">The minimum speed that is considered a full stop.</param>
		/// <param name="maxStartSpeed">Maximum initial speed of movement.</param>
		public DeceleratingKineticMotionStrategy(
			float decelerationFactor,
			float decelerationIncreaseFactor,
			float minSpeed = 5.0f,
			float maxStartSpeed = float.PositiveInfinity)
		{
			this.decelerationFactor = decelerationFactor;
			this.decelerationIncreaseFactor = decelerationIncreaseFactor;
			this.minSpeed = minSpeed;
			maxSpeed = maxStartSpeed;
		}

		public void Start(in Vector2 startPosition, in Vector2 startDirection, float startSpeed)
		{
			direction = startDirection.Normalized;
			speed = Mathf.Min(startSpeed, maxSpeed);
			deceleration = decelerationFactor;
			Position = startPosition;
			InProgress = true;
		}

		public void Update(float delta)
		{
			if (!InProgress) {
				return;
			}
			Position += delta * speed * direction;
			speed *= deceleration;
			deceleration /= decelerationIncreaseFactor;
			InProgress = speed > minSpeed;
		}

		public void Stop()
		{
			InProgress = false;
		}
	}
}
