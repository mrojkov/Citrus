using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public class ClickRecognizer : GestureRecognizer
	{
		enum State
		{
			Initial,
			WaitDrags,
			Began
		};

		private readonly TimeSpan ClickBeginDelay = TimeSpan.FromSeconds(0.15);

		private State state;
		private DateTime pressTime;
		private PollableEvent began;
		private PollableEvent canceled;
		private PollableEvent recognized;

		/// <summary>
		/// Occurs if a user has touched upon the widget.
		/// If the widget lies within a scrollable panel,
		/// the began event might be deferred in order to give the priority to drag gesture.
		/// </summary>
		public event Action Began { add { began.Handler += value; } remove { began.Handler -= value; } }
		/// <summary>
		/// Occurs if click was canceled by drag gesture.
		/// </summary>
		public event Action Canceled { add { canceled.Handler += value; } remove { canceled.Handler -= value; } }
		/// <summary>
		/// Occurs when the gesture is fully recognized.
		/// </summary>
		public event Action Recognized { add { recognized.Handler += value; } remove { recognized.Handler -= value; } }

		public bool WasBegan() => began.HasOccurred();
		public bool WasCanceled() => canceled.HasOccurred();
		public bool WasRecognized() => recognized.HasOccurred();
		public bool WasRecognizedOrCanceled() => WasCanceled() || WasRecognized();

		public int ButtonIndex { get; }

		public ClickRecognizer(int buttonIndex, Action recognized = null)
		{
			ButtonIndex = buttonIndex;
			if (recognized != null) {
				Recognized += recognized;
			}
		}

		public ClickRecognizer(Action recognized = null)
			: this(0, recognized)
		{
		}

		internal protected override void Cancel()
		{
			if (state == State.Began) {
				canceled.Raise();
			}
			state = State.Initial;
		}

		internal protected override void Update(IEnumerable<GestureRecognizer> recognizers)
		{
			if (Input.GetNumTouches() > 1) {
				Cancel();
				return;
			}
			if (state == State.Initial && Input.WasMousePressed(ButtonIndex)) {
				pressTime = DateTime.Now;
				state = State.WaitDrags;
			}
			if (state == State.WaitDrags) {
				// Defer began event if there are any drag gesture.
				if (
					!recognizers.Any(r => r.ShouldDeferClicks(ButtonIndex)) ||
					(DateTime.Now - pressTime > ClickBeginDelay) ||
					!Input.IsMousePressed(ButtonIndex)
				) {
					state = State.Began;
					began.Raise();
				}
			}
			if (state == State.Began) {
				if (!Input.IsMousePressed(ButtonIndex)) {
					state = State.Initial;
					if (Owner.IsMouseOverThisOrDescendant()) {
						recognized.Raise();
					} else {
						canceled.Raise();
					}
				}
			}
		}
	}

	public class DoubleClickRecognizer : GestureRecognizer
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

		public DoubleClickRecognizer(int buttonIndex, Action recognized = null)
		{
			ButtonIndex = buttonIndex;
			if (recognized != null) {
				Recognized += recognized;
			}
		}

		public DoubleClickRecognizer(Action recognized = null)
			: this(0, recognized)
		{
		}

		internal protected override void Cancel()
		{
		}

		internal protected override void Update(IEnumerable<GestureRecognizer> recognizers)
		{
			if (state != State.Initial && DateTime.Now - pressTime > DoubleClickTimeout) {
				state = State.Initial;
			}
			if (state == State.Initial && Input.WasMousePressed(ButtonIndex)) {
				pressTime = DateTime.Now;
				pressPosition = Input.MousePosition;
				state = State.FirstPress;
			}
			if (state == State.FirstPress && !Input.IsMousePressed(ButtonIndex)) {
					state = State.WaitSecondPress;
			}
			if (state == State.WaitSecondPress && Input.WasMousePressed(ButtonIndex)) {
				state = State.Initial;
				if (Input.GetNumTouches() == 1 && (Input.MousePosition - pressPosition).Length < MaxDistanceBetweenPresses) {
					CancelOtherGestures(recognizers);
					recognized.Raise();
				}
			}
		}

		void CancelOtherGestures(IEnumerable<GestureRecognizer> recognizers)
		{
			foreach (var r in recognizers) {
				if (r != this)
					r.Cancel();
			}
		}
	}
}
