using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public class MulticlickRecognizer : GestureRecognizer
	{
		private Vector2 pressPosition;
		private DateTime pressTime;
		private int pressCounter;
		private bool recognizing;

		private readonly TimeSpan ClickBeginDelay = TimeSpan.FromSeconds(0.15);
		private readonly TimeSpan MaxTimeBetweenClicks = TimeSpan.FromSeconds(0.3);

		public int ButtonIndex { get; private set; }
		public int NumClicks { get; private set; }

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

		public MulticlickRecognizer(int buttonIndex, int numClicks, Action recognized = null)
		{
			ButtonIndex = buttonIndex;
			NumClicks = numClicks;
			if (recognized != null) {
				Recognized += recognized;
			}
		}

		internal protected override void Cancel()
		{
			pressCounter = 0;
			if (recognizing) {
				recognizing = false;
				canceled.Raise();
			}
		}

		internal protected override void Update(IEnumerable<GestureRecognizer> recognizers)
		{
			if (pressCounter > 0 && pressCounter < NumClicks && DateTime.Now - pressTime > MaxTimeBetweenClicks) {
				Cancel();
			}
			if (Input.WasMousePressed(ButtonIndex)) {
				pressCounter++;
				pressTime = DateTime.Now;
				pressPosition = Input.MousePosition;
			}
			if (!recognizing && pressCounter > 0) {
				// Defer began event if there are any drag gesture.
				if (!recognizers.Any(r => r.ShouldDeferClicks(ButtonIndex)) || (DateTime.Now - pressTime > ClickBeginDelay)) {
					recognizing = true;
					began.Raise();
				}
			}
			if (Input.WasMouseReleased(ButtonIndex)) {
				if (pressCounter == NumClicks) {
					if (Owner.IsMouseOverThisOrDescendant() && Input.GetNumTouches() == 0) {
						CancelOtherRecognizers(recognizers);
						pressCounter = 0;
						if (!recognizing) {
							began.Raise();
						}
						recognizing = false;
						recognized.Raise();
					} else {
						Cancel();
					}
				}
			}
		}

		void CancelOtherRecognizers(IEnumerable<GestureRecognizer> recognizers)
		{
			foreach (var r in recognizers) {
				var ok = r == this || 
					(r as MulticlickRecognizer)?.NumClicks > NumClicks ||
				                                (r as MulticlickRecognizer)?.ButtonIndex != ButtonIndex;
				if (!ok) {
					r.Cancel();
				}
			}
		}
	}

	public class ClickRecognizer : MulticlickRecognizer
	{
		public ClickRecognizer(Action recognized = null)
			: base(0, 1, recognized)
		{
		}

		public ClickRecognizer(int buttonIndex, Action recognized = null)
			: base(buttonIndex, 1, recognized)
		{
		}
	}

	public class DoubleClickRecognizer : MulticlickRecognizer
	{
		public DoubleClickRecognizer(Action recognized = null)
			: base(0, 2, recognized)
		{
		}

		public DoubleClickRecognizer(int buttonIndex, Action recognized = null)
			: base(buttonIndex, 2, recognized)
		{
		}
	}
}
