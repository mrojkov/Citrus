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

	public class DragRecognizer : GestureRecognizer
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
		private PollableEvent dragged;
		private PollableEvent ended;

		/// <summary>
		/// Occurs if a user started gesture in a valid direction.
		/// </summary>
		public event Action Began { add { began.Handler += value; } remove { began.Handler -= value; } }
		/// <summary>
		/// Occurs when a user moved his finger.
		/// </summary>
		public event Action Dragged { add { dragged.Handler += value; } remove { dragged.Handler -= value; } }
		/// <summary>
		/// Occurs when a user lifted up all fingers.
		/// </summary>
		public event Action Ended { add { ended.Handler += value; } remove { ended.Handler -= value; } }

		public bool WasBegan() => began.HasOccurred();
		public bool WasMoved() => dragged.HasOccurred();
		public bool WasEnded() => ended.HasOccurred();

		public bool IsDragging() => state == State.Dragging; 

		public DragRecognizer(int buttonIndex = 0, DragDirection direction = DragDirection.Any, float dragThreshold = DefaultDragThreshold)
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

		internal protected override void Update(IEnumerable<GestureRecognizer> recognizers)
		{
			if (state == State.Initial && Input.WasMousePressed(ButtonIndex)) {
				state = State.Recognizing;
				MousePressPosition = Input.MousePosition;
			}
			if (state == State.Recognizing) {
				if (!Input.IsMousePressed(ButtonIndex)) {
					state = State.Initial;
				} else if (Recognize()) {
					CancelClickRecognition(recognizers);
					state = State.Dragging;
					prevMousePosition = Input.MousePosition;
					began.Raise();
				}
			}
			if (state == State.Dragging) {
				var curMousePos = Input.MousePosition;
				if (prevMousePosition != curMousePos) {
					dragged.Raise();
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

		private void CancelClickRecognition(IEnumerable<GestureRecognizer> recognizers)
		{
			foreach (var r in recognizers.OfType<MulticlickRecognizer>().Where(r => r.ButtonIndex == ButtonIndex)) {
				r.Cancel();
			}
		}
	}
}
