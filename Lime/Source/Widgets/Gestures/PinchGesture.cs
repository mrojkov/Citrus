namespace Lime
{
	public class PinchGesture : DragGesture
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
			: base(0, direction, exclusive, dragThreshold)
		{
		}

		protected internal override void Update(float delta)
		{
			base.Update(delta);

			if (WasBegan()) {
				TouchPressDistance = Vector2.Distance(Touch0, Touch1);
				TouchDistance = TouchPressDistance;
			} else if (IsChanging()) {
				float previousTouchDistance = TouchDistance;
				TouchDistance = Vector2.Distance(Touch0, Touch1);
				LastPinchScale = TouchDistance / previousTouchDistance;
				if (LastPinchScale != 1.0f) {
					RaiseChanged();
					LastPinchScale = 1.0f;
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
				position = (Touch0 + Touch1) * 0.5f;
				return true;
			}
			position = Vector2.NaN;
			return false;
		}

		protected override bool IsDragging()
		{
			return Input.GetNumTouches() == 2;
		}

		protected override bool CanStartDrag()
		{
			return
				(MousePosition - MousePressPosition).SqrLength > DragThresholdSqr ||
				TouchDistance != Vector2.Distance(Touch0, Touch1);
		}
	}
}
