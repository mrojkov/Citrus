using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public enum DragDirection
	{
		Any,
		Horizontal,
		Vertical
	}

	public class DragGesture : Gesture
	{
		enum State
		{
			Initial,
			Recognizing,
			Dragging
		}

		private const float DefaultDragThreshold = 5;

		private State state;
		private Vector2 prevMousePosition;

		public int ButtonIndex { get; private set; }
		public DragDirection Direction { get; private set; }
		public float DragThreshold { get; private set; }

		public Vector2 MousePressPosition { get; private set; }

		private PollableEvent began;
		private PollableEvent recognized;
		private PollableEvent changed;
		private PollableEvent ended;

		/// <summary>
		/// Occurs if a user started gesture in a valid direction.
		/// </summary>
		public event Action Recognized { add { recognized.Handler += value; } remove { recognized.Handler -= value; } }
		/// <summary>
		/// Occurs when a user moved his finger.
		/// </summary>
		public event Action Changed { add { changed.Handler += value; } remove { changed.Handler -= value; } }
		/// <summary>
		/// Occurs when a user lifted up all fingers.
		/// </summary>
		public event Action Ended { add { ended.Handler += value; } remove { ended.Handler -= value; } }

		public bool WasBegan() => began.HasOccurred();
		public bool WasRecognized() => recognized.HasOccurred();
		public bool WasChanged() => changed.HasOccurred();
		public bool WasEnded() => ended.HasOccurred();

		public bool IsRecognizing() => state == State.Recognizing;
		public bool IsDragging() => state == State.Dragging;

		public DragGesture(int buttonIndex = 0, DragDirection direction = DragDirection.Any, float dragThreshold = DefaultDragThreshold)
		{
			ButtonIndex = buttonIndex;
			Direction = direction;
			DragThreshold = dragThreshold;
		}

		internal protected override void Cancel()
		{
			if (state == State.Dragging) {
				ended.Raise();
			}
			state = State.Initial;
		}

		internal protected override bool ShouldDeferClicks(int buttonIndex) => buttonIndex == ButtonIndex;

		internal protected override void Update(IEnumerable<Gesture> gestures)
		{
			if (state == State.Initial && Input.WasMousePressed(ButtonIndex)) {
				state = State.Recognizing;
				MousePressPosition = Input.MousePosition;
				began.Raise();
			}
			if (state == State.Recognizing) {
				if (!Input.IsMousePressed(ButtonIndex)) {
					state = State.Initial;
				} else if (Recognize()) {
					CancelClickRecognition(gestures);
					state = State.Dragging;
					prevMousePosition = Input.MousePosition;
					recognized.Raise();
				}
			}
			if (state == State.Dragging) {
				var curMousePos = Input.MousePosition;
				if (prevMousePosition != curMousePos) {
					changed.Raise();
				}
				prevMousePosition = curMousePos;
				if (!Input.IsMousePressed(ButtonIndex)) {
					ended.Raise();
					state = State.Initial;
				}
			}
		}

		private bool Recognize()
		{
			var d = Input.MousePosition - MousePressPosition;
			var dx = Math.Abs(d.X);
			var dy = Math.Abs(d.Y);
			return
				Direction == DragDirection.Any && (dx > DragThreshold || dy > DragThreshold) ||
				Direction == DragDirection.Horizontal && dx > DragThreshold ||
				Direction == DragDirection.Vertical && dy > DragThreshold;
		}

		private void CancelClickRecognition(IEnumerable<Gesture> gestures)
		{
			foreach (var r in gestures.OfType<ClickGesture>().Where(r => r.ButtonIndex == ButtonIndex)) {
				r.Cancel();
			}
		}
	}
}
