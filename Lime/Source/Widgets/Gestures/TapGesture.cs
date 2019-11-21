using System;

namespace Lime
{
	public struct TapGestureOptions
	{
		public int ButtonIndex;
		public float MinTapDuration;
		public bool CanRecognizeByTapDuration;
	}

	public abstract class TapGesture : Gesture
	{
		private enum State
		{
			Idle,
			Scheduled,
			Began
		}

		public static float Threshold = 3;

		private readonly TapGestureOptions options;

		internal Action InternalRecognized;
		private float tapDuration;
		private State state;

		public override bool IsActive => state != State.Idle;

		public Vector2 MousePressPosition { get; private set; }

		public int ButtonIndex => options.ButtonIndex;

		protected TapGesture(Action onRecognized, TapGestureOptions tapOptions) : base(onRecognized)
		{
			options = tapOptions;
		}

		protected internal override bool Cancel(Gesture sender)
		{
			if (state == State.Began) {
				RaiseCanceled();
			}
			state = State.Idle;
			return true;
		}

		protected internal override void Update(float delta)
		{
			if (Input.GetNumTouches() > 1) {
				Cancel(this);
				return;
			}

			if ((state == State.Idle) && Input.WasMousePressed(ButtonIndex)) {
				tapDuration = 0.0f;
				MousePressPosition = Input.MousePosition;
				state = State.Scheduled;
				return;
			}

			tapDuration += delta;
			if (state == State.Scheduled) {
				state = State.Began;
				RaiseBegan();
			}

			if (state == State.Began) {
				bool isTapTooShort = tapDuration < options.MinTapDuration;
				if (!Input.IsMousePressed(ButtonIndex)) {
					if (isTapTooShort) {
						RaiseCanceled();
					} else {
						Finish();
					}
				} else if (options.CanRecognizeByTapDuration && !isTapTooShort) {
					Finish();
				}
			}

			void Finish()
			{
				state = State.Idle;
				if (
					(Input.MousePosition - MousePressPosition).SqrLength < Threshold.Sqr() ||
					Owner.IsMouseOverThisOrDescendant()
				) {
					InternalRecognized?.Invoke();
					RaiseRecognized();
				} else {
					RaiseCanceled();
				}
			}
		}
	}
}
