namespace Lime
{
	public class PinchGesture : BaseDragGesture
	{
		public float TouchDistance { get; private set; }
		public float TouchPressDistance { get; private set; }
		public float TotalPinchScale => TouchDistance / TouchPressDistance;
		public float LastPinchScale { get; private set; }

		private Vector2 Touch0 => Input.GetTouchPosition(0);
		private Vector2 Touch1 => Input.GetTouchPosition(1);


		public PinchGesture() : this(DragDirection.Any, exclusive: false, DefaultDragThreshold)
		{
		}

		public PinchGesture(DragDirection dragDirection)
			: this(dragDirection, exclusive: false, DefaultDragThreshold)
		{
		}

		public PinchGesture(bool exclusive)
			: this(DragDirection.Any, exclusive, DefaultDragThreshold)
		{
		}

		public PinchGesture(DragDirection direction, bool exclusive, float dragThreshold)
			: base(direction, exclusive, dragThreshold)
		{
		}

		protected internal override void Update(float delta)
		{
			base.Update(delta);

			if (WasBegan()) {
				TouchPressDistance = Vector2.Distance(Touch0, Touch1);
				TouchDistance = TouchPressDistance;
			} else if (IsChanging()) {
				var previousTouchDistance = TouchDistance;
				TouchDistance = Vector2.Distance(Touch0, Touch1);
				LastPinchScale = TouchDistance / previousTouchDistance;
				if (!WasChanged() && TouchDistance != previousTouchDistance) {
					RaiseChanged();
				}
			}
		}

		protected override bool TryGetStartDragPosition(out Vector2 position)
		{
			return TryGetDragPosition(out position);
		}

		protected override bool TryGetDragPosition(out Vector2 position)
		{
			if (IsDragging()) {
				position = Midpoint(Touch0, Touch1);
				return true;
			}
			position = Vector2.NaN;
			return false;

			Vector2 Midpoint(in Vector2 point1, in Vector2 point2) =>
				new Vector2((point1.X + point2.X) / 2.0f, (point1.Y + point2.Y) / 2.0f);
		}

		protected override bool IsDragging()
		{
			return Input.GetNumTouches() == 2;
		}

		protected override bool CanStartDrag()
		{
			return (MousePosition - MousePressPosition).SqrLength > DragThresholdSqr;
		}
	}
}
