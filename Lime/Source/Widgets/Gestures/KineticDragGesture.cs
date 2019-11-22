using System;
using System.Collections.Generic;
using Lime.KineticMotionStrategy;

namespace Lime
{
	public class KineticDragGesture : DragGesture
	{
		private const int MaxTouchHistorySize = 3;

		private readonly Queue<(Vector2 distance, float duration)> touchHistory;
		private readonly IKineticMotionStrategy kineticMotion;
		private PollableEvent kineticEnded;

		private Vector2 previousKineticPosition;
		private Vector2 lastDragDelta;

		/// <summary>
		/// Occurs when the drag is completed and the kinetic movement begins.
		/// </summary>
		public virtual event Action Ending
		{
			add => base.Ended += value;
			remove => base.Ended -= value;
		}

		/// <summary>
		/// Occurs when kinetic movement is completed.
		/// </summary>
		public override event Action Ended
		{
			add => kineticEnded.Handler += value;
			remove => kineticEnded.Handler -= value;
		}

		public override bool IsActive => base.IsActive || kineticMotion.InProgress;

		public override Vector2 MousePosition =>
			kineticMotion.InProgress ? kineticMotion.Position : base.MousePosition;

		protected override Vector2 PreviousMousePosition =>
			kineticMotion.InProgress ? previousKineticPosition : base.PreviousMousePosition;

		public KineticDragGesture(IKineticMotionStrategy motionStrategy)
			: this(motionStrategy, buttonIndex: 0, exclusive: false, DragDirection.Any, DefaultDragThreshold)
		{
		}

		public KineticDragGesture(IKineticMotionStrategy motionStrategy, int buttonIndex)
			: this(motionStrategy, buttonIndex, exclusive: false, DragDirection.Any, DefaultDragThreshold)
		{
		}

		public KineticDragGesture(IKineticMotionStrategy motionStrategy, int buttonIndex, bool exclusive)
			: this(motionStrategy, buttonIndex, exclusive, DragDirection.Any, DefaultDragThreshold)
		{
		}

		public KineticDragGesture(
			IKineticMotionStrategy motionStrategy,
			int buttonIndex,
			bool exclusive,
			DragDirection direction,
			float dragThreshold) : base(buttonIndex, direction, exclusive, dragThreshold)
		{
			kineticMotion = motionStrategy;
			touchHistory = new Queue<(Vector2 distance, float duration)>();
		}

		public virtual bool WasEnding() => base.WasEnded();
		public override bool WasEnded() => kineticEnded.HasOccurred;

		protected void RaiseKineticEnded() => kineticEnded.Raise();

		protected internal override void Update(float delta)
		{
			if (kineticMotion.InProgress) {
				if (Input.WasMousePressed(ButtonIndex)) {
					StopKineticMovement();
					RaiseKineticEnded();
				} else {
					previousKineticPosition = kineticMotion.Position;
					kineticMotion.Update(delta);

					if (kineticMotion.InProgress) {
						RaiseChanged();
					} else {
						RaiseKineticEnded();
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
				StartKineticMovement();
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
			if (kineticMotion.InProgress) {
				if (sender == null) {
					return false;
				}
				StopKineticMovement();
			}
			return base.Cancel(sender);
		}

		private void StartKineticMovement()
		{
			var direction = Vector2.Zero;
			float totalDuration = 0.0f;
			foreach ((var distance, float duration) in touchHistory) {
				direction += distance;
				totalDuration += duration;
			}

			if (totalDuration > 0.0f) {
				float speed = direction.Length / totalDuration;
				kineticMotion.Start(base.MousePosition, direction, speed);
			}
		}

		private void StopKineticMovement()
		{
			kineticMotion.Stop();
		}
	}
}
