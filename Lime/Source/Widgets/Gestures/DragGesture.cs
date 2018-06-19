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
			Changing
		}

		private const float DefaultDragThreshold = 5;

		private State state;
		private Vector2 prevMousePosition;

		public int ButtonIndex { get; }
		public DragDirection Direction { get; }
		public float DragThreshold { get; }
		// Will cancel any other drag gestures
		public bool Exclusive { get; set; }

		public Vector2 MousePressPosition { get; private set; }
		/// <summary>
		/// MousePosition depends on DragDirection so (MousePosition - DragDirection).X or Y
		/// will always be zero if DragDirection is Vertical or Horizontal respectively.
		/// This fact is used when checking for threshold.
		/// </summary>
		public override Vector2 MousePosition
		{
			get
			{
				switch (Direction) {
				case DragDirection.Horizontal:
					return new Vector2(base.MousePosition.X, MousePressPosition.Y);
				case DragDirection.Vertical:
					return new Vector2(MousePressPosition.X, base.MousePosition.Y);
				case DragDirection.Any:
					return base.MousePosition;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}

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
		public bool IsChanging() => state == State.Changing;

		public DragGesture(int buttonIndex = 0, DragDirection direction = DragDirection.Any, bool exclusive = false, float dragThreshold = DefaultDragThreshold)
		{
			ButtonIndex = buttonIndex;
			Direction = direction;
			DragThreshold = dragThreshold;
			Exclusive = exclusive;
		}

		internal protected override void Cancel()
		{
			if (state == State.Changing) {
				ended.Raise();
			}
			state = State.Initial;
		}

		internal protected override bool ShouldDeferClicks(int buttonIndex) => buttonIndex == ButtonIndex;

		internal protected override void Update(IEnumerable<Gesture> gestures)
		{
			if (state == State.Initial && Input.WasMousePressed(ButtonIndex)) {
				state = State.Recognizing;
				MousePressPosition = base.MousePosition;
				began.Raise();
			}
			if (state == State.Recognizing) {
				if (!Input.IsMousePressed(ButtonIndex)) {
					ended.Raise();
					state = State.Initial;
				} else if ((MousePosition - MousePressPosition).SqrLength > DragThreshold.Sqr()) {
					CancelOtherGestures(gestures);
					state = State.Changing;
					prevMousePosition = base.MousePosition;
					recognized.Raise();
				}
			}
			if (state == State.Changing) {
				var curMousePos = base.MousePosition;
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

		private void CancelOtherGestures(IEnumerable<Gesture> gestures)
		{
			foreach (var r in gestures) {
				var clickGesture = r as ClickGesture;
				if (clickGesture?.ButtonIndex == ButtonIndex) {
					r.Cancel();
				}
				if (Exclusive) {
					(r as DragGesture)?.Cancel();
				}
			}
		}
	}
}
