using System;
using System.Collections.Generic;

namespace Lime
{
	public class DoubleClickGesture : Gesture
	{
		enum State
		{
			Initial,
			FirstPress,
			WaitSecondPress,
		};

		private readonly TimeSpan DoubleClickTimeout = TimeSpan.FromSeconds(0.3);
		private readonly float MaxDistanceBetweenPresses = 5;

		private State state;
		private DateTime pressTime;
		private Vector2 pressPosition;
		private PollableEvent recognized;

		public event Action Recognized { add { recognized.Handler += value; } remove { recognized.Handler -= value; } }
		public bool WasRecognized() => recognized.HasOccurred();

		public int ButtonIndex { get; }

		public DoubleClickGesture(int buttonIndex, Action recognized = null)
		{
			ButtonIndex = buttonIndex;
			if (recognized != null) {
				Recognized += recognized;
			}
		}

		public DoubleClickGesture(Action recognized = null)
			: this(0, recognized)
		{
		}

		internal protected override void Cancel()
		{
		}

		internal protected override void Update(IEnumerable<Gesture> gestures)
		{
			if (state != State.Initial && DateTime.Now - pressTime > DoubleClickTimeout) {
				state = State.Initial;
			}
			if (state == State.Initial && Input.WasMousePressed(ButtonIndex)) {
				pressTime = DateTime.Now;
				pressPosition = MousePosition;
				state = State.FirstPress;
			}
			if (state == State.FirstPress && !Input.IsMousePressed(ButtonIndex)) {
				state = State.WaitSecondPress;
			}
			if (state == State.WaitSecondPress && Input.WasMousePressed(ButtonIndex)) {
				state = State.Initial;
				if (Input.GetNumTouches() == 1 && (MousePosition - pressPosition).Length < MaxDistanceBetweenPresses) {
					CancelOtherGestures(gestures);
					recognized.Raise();
				}
			}
		}

		void CancelOtherGestures(IEnumerable<Gesture> gestures)
		{
			foreach (var r in gestures) {
				if (r != this)
					r.Cancel();
			}
		}
	}
}
