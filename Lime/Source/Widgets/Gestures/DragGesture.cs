namespace Lime
{
	public class DragGesture : BaseDragGesture
	{
		public int ButtonIndex { get; }

		public DragGesture(
			int buttonIndex = 0,
			DragDirection direction = DragDirection.Any,
			bool exclusive = false,
			float dragThreshold = DefaultDragThreshold) : base(direction, exclusive, dragThreshold)
		{
			ButtonIndex = buttonIndex;
			Exclusive = exclusive;
		}

		protected override bool TryGetStartDragPosition(out Vector2 position)
		{
			if (Input.WasMousePressed(ButtonIndex)) {
				return TryGetDragPosition(out position);
			}
			position = Vector2.NaN;
			return false;
		}

		protected override bool TryGetDragPosition(out Vector2 position)
		{
			position = Input.MousePosition;
			return true;
		}

		protected override bool IsDragging()
		{
			return Input.IsMousePressed(ButtonIndex);
		}

		protected override bool CanStartDrag()
		{
			return (MousePosition - MousePressPosition).SqrLength > DragThresholdSqr;
		}
	}
}
