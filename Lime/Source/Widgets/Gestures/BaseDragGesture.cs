using System;

namespace Lime
{
	public abstract class BaseDragGesture : Gesture
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

		/// <summary>
		/// Will cancel any other drag gestures.
		/// </summary>
		public virtual bool Exclusive { get; set; }

		/// <summary>
		/// MousePosition depends on DragDirection so (MousePosition - DragDirection).X or Y
		/// will always be zero if DragDirection is Vertical or Horizontal respectively.
		/// This fact is used when checking for threshold.
		/// </summary>
		public override bool IsActive => state != State.Idle;

		public virtual Vector2 MousePosition => ClampMousePositionByDirection(Direction);
		public Vector2 MousePressPosition { get; private set; }
		public Vector2 TotalDragDistance => MousePosition - MousePressPosition;
		public Vector2 LastDragDistance => MousePosition - PreviousMousePosition;
		public DragDirection Direction { get; private set; }

		protected virtual Vector2 PreviousMousePosition { get; private set; }
		public float DragThresholdSqr { get; set; }

		/// <summary>
		/// Occurs when dragged.
		/// </summary>
		public virtual event Action Changed
		{
			add => changed.Handler += value;
			remove => changed.Handler -= value;
		}

		public BaseDragGesture(
			DragDirection direction = DragDirection.Any,
			bool exclusive = false,
			float dragThreshold = DefaultDragThreshold)
		{
			Direction = direction;
			Exclusive = exclusive;
			DragThresholdSqr = dragThreshold.Sqr();
		}

		public virtual bool WasChanged() => changed.HasOccurred;

		public bool IsRecognizing() => state == State.Recognizing;
		public bool IsChanging() => state == State.Changing;

		protected void RaiseChanged() => changed.Raise();

		protected abstract bool TryGetStartDragPosition(out Vector2 position);
		protected abstract bool TryGetDragPosition(out Vector2 position);
		protected abstract bool IsDragging();
		protected abstract bool CanStartDrag();

		protected internal override bool Cancel(Gesture sender = null)
		{
			if (sender != null && (sender as BaseDragGesture)?.Exclusive != true) {
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
						PreviousMousePosition = mousePosition;
					}
					RaiseRecognized();
				}
			}

			if (state == State.Changing) {
				if (
					TryGetDragPosition(out var mousePosition) &&
					(PreviousMousePosition != mousePosition)
				) {
					RaiseChanged();
					PreviousMousePosition = mousePosition;
				}

				if (!IsDragging()) {
					RaiseEnded();
					state = State.Idle;
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
	}
}
