using System;
using System.Collections.Generic;

namespace Lime
{
	public class DragGesture : Gesture
	{
		private enum State
		{
			Idle,
			Recognizing,
			Changing
		}

		protected const float DefaultDragThreshold = 5;

		private PollableEvent changed;
		private State state;
		private readonly IMotionStrategy motionStrategy;
		private const int MaxTouchHistorySize = 3;

		private readonly Queue<(Vector2 distance, float duration)> touchHistory;
		private PollableEvent ending;

		private Vector2 previousSpecialPosition;
		private Vector2 lastDragDelta;


		/// <summary>
		/// Will cancel any other drag gestures.
		/// </summary>
		public virtual bool Exclusive { get; set; }

		/// <summary>
		/// MousePosition depends on DragDirection so (MousePosition - DragDirection).X or Y
		/// will always be zero if DragDirection is Vertical or Horizontal respectively.
		/// This fact is used when checking for threshold.
		/// </summary>
		public override bool IsActive => state != State.Idle || (motionStrategy?.InProgress ?? false);

		public virtual Vector2 MousePosition => (motionStrategy?.InProgress ?? false) ? motionStrategy.Position : ClampMousePositionByDirection(Direction);
		public Vector2 MousePressPosition { get; private set; }
		public Vector2 TotalDragDistance => MousePosition - MousePressPosition;
		public Vector2 LastDragDistance => MousePosition - PreviousMousePosition;
		public DragDirection Direction { get; private set; }
		public int ButtonIndex { get; }

		private Vector2 previousMousePosition;
		protected virtual Vector2 PreviousMousePosition => (motionStrategy?.InProgress ?? false) ? previousSpecialPosition : previousMousePosition;
		protected float DragThresholdSqr { get; }

		/// <summary>
		/// Occurs when dragged.
		/// </summary>
		public virtual event Action Changed
		{
			add => changed.Handler += value;
			remove => changed.Handler -= value;
		}

		/// <summary>
		/// Occurs when the drag is completed and the special movement begins.
		/// </summary>
		public virtual event Action Ending
		{
			add => ending.Handler += value;
			remove => ending.Handler -= value;
		}

		public DragGesture(
			int buttonIndex = 0,
			DragDirection direction = DragDirection.Any,
			bool exclusive = false,
			float dragThreshold = DefaultDragThreshold)
		{
			ButtonIndex = buttonIndex;
			Direction = direction;
			Exclusive = exclusive;
			DragThresholdSqr = dragThreshold.Sqr();
		}

		public DragGesture(IMotionStrategy motionStrategy)
		{
			this.motionStrategy = motionStrategy;
			if (motionStrategy != null) {
				touchHistory = new Queue<(Vector2 distance, float duration)>();
			}
		}

		public virtual bool WasChanged() => changed.HasOccurred;
		public bool WasEnding() => ending.HasOccurred;
		public bool IsRecognizing() => state == State.Recognizing;
		public bool IsChanging() => state == State.Changing;
		public void RaiseEnding() => ending.Raise();
		protected void RaiseChanged() => changed.Raise();

		protected virtual bool TryGetStartDragPosition(out Vector2 position)
		{
			if (Input.WasMousePressed(ButtonIndex)) {
				return TryGetDragPosition(out position);
			}
			position = Vector2.NaN;
			return false;
		}
		protected virtual bool TryGetDragPosition(out Vector2 position)
		{
			position = Input.MousePosition;
			return true;
		}
		protected virtual bool IsDragging()
		{
			return Input.IsMousePressed(ButtonIndex);
		}
		protected virtual bool CanStartDrag()
		{
			return (MousePosition - MousePressPosition).SqrLength > DragThresholdSqr;
		}

		protected internal override bool Cancel(Gesture sender = null)
		{
			if (motionStrategy != null && motionStrategy.InProgress) {
				if (sender == null) {
					return false;
				}
				StopSpecialMotion();
			}
			if (sender != null && (sender as DragGesture)?.Exclusive != true) {
				return false;
			}
			if (state != State.Idle) {
				RaiseCanceled();
				state = State.Idle;
			}
			return true;
		}

		protected internal override void Update(float delta)
		{
			if (motionStrategy != null && motionStrategy.InProgress) {
				if (Input.WasMousePressed(ButtonIndex)) {
					StopSpecialMotion();
					RaiseEnded();
				} else {
					previousSpecialPosition = motionStrategy.Position;
					motionStrategy.Update(delta);

					if (motionStrategy.InProgress) {
						RaiseChanged();
					} else {
						RaiseEnded();
					}
				}
				return;
			}

			var savedPreviousMousePosition = previousMousePosition;
			bool wasChanging = IsChanging();

			if (state == State.Idle && TryGetStartDragPosition(out var startPosition)) {
				state = State.Recognizing;
				MousePressPosition = startPosition;
				RaiseBegan();
			}

			if (state == State.Recognizing) {
				if (!IsDragging()) {
					state = State.Idle;
					RaiseCanceled();
				} else if (CanStartDrag()) {
					state = State.Changing;
					if (TryGetDragPosition(out var mousePosition)) {
						previousMousePosition = mousePosition;
					}
					RaiseRecognized();
				}
			}

			if (state == State.Changing) {
				if (
					TryGetDragPosition(out var mousePosition) &&
					(previousMousePosition != mousePosition)
				) {
					RaiseChanged();
					previousMousePosition = mousePosition;
				}

				if (!IsDragging()) {
					if (motionStrategy != null) {
						RaiseEnding();
					} else {
						RaiseEnded();
					}
					state = State.Idle;
				}
			}
			if (motionStrategy != null && wasChanging) {
				if (WasEnding()) {
					StartSpecialMotion();
					touchHistory.Clear();
				} else {
					lastDragDelta = ClampMousePositionByDirection(Direction) - savedPreviousMousePosition;
					touchHistory.Enqueue((lastDragDelta, delta));
					if (touchHistory.Count > MaxTouchHistorySize) {
						touchHistory.Dequeue();
					}
				}
			}
		}

		private Vector2 ClampMousePositionByDirection(DragDirection direction)
		{
			if (!TryGetDragPosition(out var position)) {
				return Vector2.NaN;
			}

			switch (direction) {
				case DragDirection.Horizontal:
					return new Vector2(position.X, MousePressPosition.Y);
				case DragDirection.Vertical:
					return new Vector2(MousePressPosition.X, position.Y);
				case DragDirection.Any:
					return position;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void StartSpecialMotion()
		{
			var direction = Vector2.Zero;
			float totalDuration = 0.0f;
			foreach (var (distance, duration) in touchHistory) {
				direction += distance;
				totalDuration += duration;
			}

			if (totalDuration > 0.0f) {
				float speed = direction.Length / totalDuration;
				motionStrategy.Start(ClampMousePositionByDirection(Direction), direction, speed);
			}
		}

		private void StopSpecialMotion()
		{
			motionStrategy.Stop();
		}

		/// <summary>
		/// Defines a motion strategy.
		/// </summary>
		public interface IMotionStrategy
		{
			Vector2 Position { get; }
			bool InProgress { get; }

			/// <summary>
			/// Start position calculation.
			/// </summary>
			/// <param name="startPosition">The position in which the movement began.</param>
			/// <param name="startDirection">The direction in which the movement began.</param>
			/// <param name="startSpeed">The initial speed of movement.</param>
			void Start(Vector2 startPosition, Vector2 startDirection, float startSpeed);

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
		public class DampingMotionStrategy : IMotionStrategy
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
			public DampingMotionStrategy(
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

			public void Start(Vector2 startPosition, Vector2 startDirection, float startSpeed)
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
		public class FixedTimeMotionStrategy : IMotionStrategy
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
			public FixedTimeMotionStrategy(float specialDuration)
			{
				duration = specialDuration;
			}

			public virtual void Start(Vector2 startPosition, Vector2 startDirection, float startSpeed)
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
