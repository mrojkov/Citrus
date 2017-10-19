using System;
using Yuzu;

namespace Lime
{
	[AllowedChildrenTypes(typeof(Node))]
	public class Slider : Widget
	{
		[YuzuMember]
		public float RangeMin { get; set; }

		[YuzuMember]
		public float RangeMax { get; set; }

		[YuzuMember]
		public float Value
		{
			get { return value.Clamp(RangeMin, RangeMax); }
			set { this.value = value; }
		}

		[YuzuMember]
		public float Step { get; set; }

		public event Action DragStarted;
		public event Action DragEnded;
		public event Action Changed;
		public bool Enabled;

		private float value;
		private Widget thumb;
		private DragRecognizer dragThumbRecognizer;
		private DragRecognizer dragSliderRecognizer;
		private ClickRecognizer clickRecognizer;
		private DragRecognizer activeDragRecognizer;

		public Slider()
		{
			HitTestTarget = true;
			RangeMin = 0;
			RangeMax = 100;
			Value = 0;
			Step = 0;
			Enabled = true;
		}

		protected override void Awake()
		{
			Thumb?.GestureRecognizers.Add(dragThumbRecognizer = new DragRecognizer());
			GestureRecognizers.Add(dragSliderRecognizer = new DragRecognizer());
			GestureRecognizers.Add(clickRecognizer = new ClickRecognizer());
		}

		public Widget Thumb
		{
			get { return thumb ?? MaybeThumb("SliderThumb") ?? MaybeThumb("Thumb"); }
		}

		private Widget MaybeThumb(string name)
		{
			if (thumb == null) {
				thumb = Nodes.TryFind(name) as Widget;
				if (thumb != null)
					thumb.HitTestTarget = true;
			}
			return thumb;
		}

		public override Node Clone()
		{
			var result = base.Clone() as Slider;
			result.thumb = null;
			return result;
		}

		private Spline Rail
		{
			get { return Nodes.TryFind("Rail") as Spline; }
		}

		protected override void SelfUpdate(float delta)
		{
			if (GloballyVisible) {
				Advance();
			}
		}

		void Advance()
		{
			if (Thumb == null) {
				return;
			}
			var draggingJustBegun = false;
			if (Enabled && RangeMax > RangeMin) {
				if (dragThumbRecognizer.WasBegan()) {
					activeDragRecognizer = dragThumbRecognizer;
					StartDrag();
					draggingJustBegun = true;
				} else {
					if (clickRecognizer.WasBegan()) {
						StartDrag();
						SetValueFromCurrentMousePosition(false);
					}
					if (clickRecognizer.WasRecognizedOrCanceled() && !dragSliderRecognizer.WasBegan()) {
						Release();
					}
					if (dragSliderRecognizer.WasBegan()) {
						activeDragRecognizer = dragSliderRecognizer;
						StartDrag();
						dragInitialDelta = 0;
						dragInitialOffset = (Value - RangeMin) / (RangeMax - RangeMin);
						draggingJustBegun = false;
					}
				}
			}
			if (activeDragRecognizer?.WasEnded() ?? false) {
				Release();
			}
			if (Enabled && (activeDragRecognizer?.IsDragging() ?? false)) {
				SetValueFromCurrentMousePosition(draggingJustBegun);
			}
			InterpolateGraphicsBetweenMinAndMaxMarkers();
			RefreshThumbPosition();
		}

		private void RaiseDragEnded()
		{
			DragEnded?.Invoke();
		}

		private void StartDrag()
		{
			RunThumbAnimation("Press");
			DragStarted?.Invoke();
		}

		private void InterpolateGraphicsBetweenMinAndMaxMarkers()
		{
			if (RangeMax - RangeMin < float.Epsilon) {
				return;
			}
			var mn = Markers.TryFind("NormalMin");
			var mx = Markers.TryFind("NormalMax");
			if (mn != null && mx != null) {
				var t = (Value - RangeMin) / (RangeMax - RangeMin);
				AnimationTime = mn.Time + (mx.Time - mn.Time) * t;
			}
		}

		private void RefreshThumbPosition()
		{
			if (RangeMax - RangeMin < float.Epsilon) {
				return;
			}
			var t = (Value - RangeMin) / (RangeMax - RangeMin);
			var pos = Rail.CalcPoint(t * Rail.CalcLengthRough());
			Thumb.Position = Rail.CalcTransitionToSpaceOf(this) * pos;
		}

		private void Release()
		{
			RaiseDragEnded();
			RunThumbAnimation("Normal");
		}

		private void RunThumbAnimation(string name)
		{
			Thumb?.TryRunAnimation(name);
		}

		private float dragInitialOffset;
		private float dragInitialDelta;

		private void SetValueFromCurrentMousePosition(bool draggingJustBegun)
		{
			if (Rail == null) {
				return;
			}
			float railLength = Rail.CalcLengthRough();
			if (railLength <= 0) {
				return;
			}
			Matrix32 transform = Rail.LocalToWorldTransform.CalcInversed();
			Vector2 p = transform.TransformVector(Input.MousePosition);
			float offset = Rail.CalcSplineLengthToNearestPoint(p) / railLength;
			if (RangeMax <= RangeMin) {
				return;
			}
			float v = offset * (RangeMax - RangeMin) + RangeMin;
			if (draggingJustBegun) {
				dragInitialDelta = Value - v;
				dragInitialOffset = offset;
				return;
			}
			float prevValue = Value;
			if (offset > dragInitialOffset && dragInitialOffset < 1) {
				Value = v + dragInitialDelta * (1 - (offset - dragInitialOffset) / (1 - dragInitialOffset));
			} else if (offset < dragInitialOffset && dragInitialOffset > 0) {
				Value = v + dragInitialDelta * (1 - (dragInitialOffset - offset) / dragInitialOffset);
			} else {
				Value = v + dragInitialDelta;
			}
			if (Step > 0) {
				Value = (float)Math.Round(Value / Step) * Step;
			}
			if (Value != prevValue) {
				RaiseChanged();
			}
		}

		private void RaiseChanged()
		{
			Changed?.Invoke();
		}

		public void SetValue(float newValue)
		{
			if (Value == newValue) return;
			value = newValue;
			RaiseChanged();
		}
	}
}
