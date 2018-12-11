using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public class ClickGesture : Gesture
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

		internal Action InternalRecognized;
		public static float Threshold;

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

		public Vector2 MousePressPosition { get; private set; }

		public bool WasBegan() => began.HasOccurred();
		public bool WasCanceled() => canceled.HasOccurred();
		public bool WasRecognized() => recognized.HasOccurred();
		public bool WasRecognizedOrCanceled() => WasCanceled() || WasRecognized();

		public int ButtonIndex { get; }

		public ClickGesture(int buttonIndex, Action recognized = null)
		{
			ButtonIndex = buttonIndex;
			if (recognized != null) {
				Recognized += recognized;
			}
		}

		public ClickGesture(Action recognized = null)
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

		internal protected override void Update(IEnumerable<Gesture> gestures)
		{
			if (Input.GetNumTouches() > 1) {
				Cancel();
				return;
			}
			if (state == State.Initial && Input.WasMousePressed(ButtonIndex)) {
				pressTime = DateTime.Now;
				state = State.WaitDrags;
				MousePressPosition = Input.MousePosition;
			}
			if (state == State.WaitDrags) {
				// Defer began event if there are any drag gesture.
				if (
					!gestures.Any(r => r.ShouldDeferClicks(ButtonIndex)) ||
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
					if ((Input.MousePosition - MousePressPosition).SqrLength < Threshold.Sqr() ||
						 Owner.IsMouseOverThisOrDescendant()
					) {
						InternalRecognized?.Invoke();
						recognized.Raise();
					} else {
						canceled.Raise();
					}
				}
			}
		}
	}
}
