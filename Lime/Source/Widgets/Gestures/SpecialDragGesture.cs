using System;
using System.Collections.Generic;

namespace Lime
{
	public class SpecialDragGesture : DragGesture
	{
		private const int MaxTouchHistorySize = 3;

		private readonly Queue<(Vector2 distance, float duration)> touchHistory;
		private readonly IMotion motion;
		private PollableEvent specialEnded;

		private Vector2 previousSpecialPosition;
		private Vector2 lastDragDelta;

		/// <summary>
		/// Occurs when the drag is completed and the special movement begins.
		/// </summary>
		public virtual event Action Ending
		{
			add => base.Ended += value;
			remove => base.Ended -= value;
		}

		/// <summary>
		/// Occurs when motion is completed.
		/// </summary>
		public override event Action Ended
		{
			add => specialEnded.Handler += value;
			remove => specialEnded.Handler -= value;
		}

		public override bool IsActive => base.IsActive || motion.InProgress;

		public override Vector2 MousePosition =>
			motion.InProgress ? motion.Position : base.MousePosition;

		protected override Vector2 PreviousMousePosition =>
			motion.InProgress ? previousSpecialPosition : base.PreviousMousePosition;

		public SpecialDragGesture(IMotion motion)
			: this(motion, buttonIndex: 0, exclusive: false, DragDirection.Any, DefaultDragThreshold)
		{
		}

		public SpecialDragGesture(IMotion motion, int buttonIndex)
			: this(motion, buttonIndex, exclusive: false, DragDirection.Any, DefaultDragThreshold)
		{
		}

		public SpecialDragGesture(IMotion motion, int buttonIndex, bool exclusive)
			: this(motion, buttonIndex, exclusive, DragDirection.Any, DefaultDragThreshold)
		{
		}

		public SpecialDragGesture(
			IMotion motion,
			int buttonIndex,
			bool exclusive,
			DragDirection direction,
			float dragThreshold) : base(buttonIndex, direction, exclusive, dragThreshold)
		{
			this.motion = motion;
			touchHistory = new Queue<(Vector2 distance, float duration)>();
		}

		public virtual bool WasEnding() => base.WasEnded();
		public override bool WasEnded() => specialEnded.HasOccurred;

		protected void RaiseSpecialEnded() => specialEnded.Raise();

		protected internal override void Update(float delta)
		{
			if (motion.InProgress) {
				if (Input.WasMousePressed(ButtonIndex)) {
					StopSpecialMotion();
					RaiseSpecialEnded();
				} else {
					previousSpecialPosition = motion.Position;
					motion.Update(delta);

					if (motion.InProgress) {
						RaiseChanged();
					} else {
						RaiseSpecialEnded();
					}
				}
				return;
			}

			if (!IsChanging()) {
				base.Update(delta);
				return;
			}

			var previousMousePosition = PreviousMousePosition;
			base.Update(delta);

			if (base.WasEnded()) {
				StartSpecialMotion();
				touchHistory.Clear();
			} else {
				lastDragDelta = base.MousePosition - previousMousePosition;
				touchHistory.Enqueue((lastDragDelta, delta));
				if (touchHistory.Count > MaxTouchHistorySize) {
					touchHistory.Dequeue();
				}
			}
		}

		protected internal override bool Cancel(Gesture sender)
		{
			if (motion.InProgress) {
				if (sender == null) {
					return false;
				}
				StopSpecialMotion();
			}
			return base.Cancel(sender);
		}

		private void StartSpecialMotion()
		{
			var direction = Vector2.Zero;
			float totalDuration = 0.0f;
			foreach ((var distance, float duration) in touchHistory) {
				direction += distance;
				totalDuration += duration;
			}

			if (totalDuration > 0.0f) {
				float speed = direction.Length / totalDuration;
				motion.Start(base.MousePosition, direction, speed);
			}
		}

		private void StopSpecialMotion()
		{
			motion.Stop();
		}

		/// <summary>
		/// Defines a motion strategy.
		/// </summary>
		public interface IMotion
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

		/// <summary>
		/// Non-uniformly slow motion, occurring to a full stop.
		/// </summary>
		public class DampingMotion : IMotion
		{
			private readonly float initialDamping;
			private readonly float dampingDamping;
			private readonly float minSpeed;
			private readonly float maxSpeed;

			private Vector2 direction;
			private float speed;
			private float damping;

			public Vector2 Position { get; private set; }
			public bool InProgress { get; private set; }

			/// <param name="initialDamping">Initial deceleration factor.</param>
			/// <param name="dampingDamping">Increase of the deceleration factor.</param>
			/// <param name="minSpeed">The minimum speed that is considered a full stop.</param>
			/// <param name="maxStartSpeed">Maximum initial speed of movement.</param>
			public DampingMotion(
				float initialDamping,
				float dampingDamping,
				float minSpeed = 5.0f,
				float maxStartSpeed = float.PositiveInfinity)
			{
				this.initialDamping = initialDamping;
				this.dampingDamping = dampingDamping;
				this.minSpeed = minSpeed;
				maxSpeed = maxStartSpeed;
			}

			public void Start(in Vector2 startPosition, in Vector2 startDirection, float startSpeed)
			{
				direction = startDirection.Normalized;
				speed = Mathf.Min(startSpeed, maxSpeed);
				damping = initialDamping;
				Position = startPosition;
				InProgress = true;
			}

			public void Update(float delta)
			{
				if (!InProgress) {
					return;
				}
				Position += delta * speed * direction;
				speed *= damping;
				damping *= dampingDamping;
				InProgress = speed > minSpeed;
			}

			public void Stop()
			{
				InProgress = false;
			}
		}

		/// <summary>
		/// Uniformly slow motion, occurring within a specified time.
		/// </summary>
		public class FixedTimeMotion : IMotion
		{
			private readonly float duration;

			private Vector2 fromPosition;
			private Vector2 direction;
			private float speed;
			private float deceleration;
			private float timeElapsed;

			public Vector2 Position { get; private set; }
			public bool InProgress { get; private set; }

			/// <param name="specialDuration">The time during which the movement slows down.</param>
			public FixedTimeMotion(float specialDuration)
			{
				duration = specialDuration;
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
}
