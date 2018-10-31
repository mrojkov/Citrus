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

		private readonly TimeSpan DoubleClickTimeout =
#if WIN
			TimeSpan.FromMilliseconds(System.Windows.Forms.SystemInformation.DoubleClickTime);
#else // WIN
			TimeSpan.FromSeconds(0.3);
#endif // WIN

		private readonly Size DoubleClickSize =
#if WIN
			new Size(System.Windows.Forms.SystemInformation.DoubleClickSize.Width, System.Windows.Forms.SystemInformation.DoubleClickSize.Height);
#else // WIN
			new Size(5, 5);
#endif // WIN

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
		{ }

		internal protected override void Cancel()
		{ }

		internal protected override void Update(IEnumerable<Gesture> gestures)
		{
			var now = DateTime.Now;
			var delta = now - pressTime;
			if (state != State.Initial && delta > DoubleClickTimeout) {
				state = State.Initial;
			}
			if (state == State.Initial && Input.WasMousePressed(ButtonIndex)) {
				pressTime = now;
				pressPosition = Input.MousePosition;
				state = State.FirstPress;
			}
			if (state == State.FirstPress && !Input.IsMousePressed(ButtonIndex)) {
				state = State.WaitSecondPress;
			}
			if (state == State.WaitSecondPress && Input.WasMousePressed(ButtonIndex)) {
				state = State.Initial;
				var r = new Rectangle(pressPosition, pressPosition + (Vector2)DoubleClickSize);
				if (Input.GetNumTouches() == 1 && r.Contains(Input.MousePosition)) {
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
