using System;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class Slider : Widget
	{
		[ProtoMember(1)]
		public float RangeMin { get; set; }

		[ProtoMember(2)]
		public float RangeMax { get; set; }

		[ProtoMember(3)]
		public float Value
		{
			get { return value.Clamp(RangeMin, RangeMax); }
			set { this.value = value; }
		}

		public event Action Released;
		public event Action Changed;

		public bool Enabled;

		float value;

		Widget thumb;
		Spline rail;

		public Slider()
		{
			RangeMin = 0;
			RangeMax = 100;
			Value = 0;
			Enabled = true;
		}

		private Widget Thumb
		{
			get {
				if (thumb == null) {
					TryFind("SliderThumb", out thumb);
				}
				return thumb;
			}
		}

		private Spline Rail
		{
			get {
				if (rail == null) {
					TryFind("Rail", out rail);
				}
				return rail;
			}
		}

		public override void Update(int delta)
		{
			if (GloballyVisible) {
				Advance();
			}
			base.Update(delta);
		}

		void Advance()
		{
			if (Thumb == null) {
				return;
			}
			if (Input.WasMousePressed() && Thumb.IsMouseOver()) {
				Thumb.TryRunAnimation("Focus");
				Input.CaptureMouse();
			} else if (Input.IsMouseOwner() && !Input.IsMousePressed()) {
				Release();
			}
			if (Input.IsMouseOwner() && Enabled) {
				if (Input.WasMousePressed()) {
					SetValueFromCurrentMousePosition(draggingJustBegun: true);
				} else if (Input.IsMousePressed()) {
					SetValueFromCurrentMousePosition(draggingJustBegun: false);
				}
			}
			RefreshThumbPosition();
			if (!Input.IsMouseOwner()) {
				Release();
			}
		}

		private void RefreshThumbPosition()
		{
			Marker startMarker, endMarker;
			GetStartEndMarkers(out startMarker, out endMarker);
			if (startMarker != null && endMarker != null) {
				if (RangeMax > RangeMin) {
					float t1 = AnimationUtils.FramesToMsecs(startMarker.Frame);
					float t2 = AnimationUtils.FramesToMsecs(endMarker.Frame);
					AnimationTime = (t1 + (Value - RangeMin) / (RangeMax - RangeMin) * (t2 - t1)).Round();
				}
			}
		}

		private void Release()
		{
			if (Thumb.CurrentAnimation != "Normal") {
				Input.ReleaseMouse();
				Thumb.TryRunAnimation("Normal");
				if (Released != null) {
					Released();
				}
			}
		}

		private void GetStartEndMarkers(out Marker startMarker, out Marker endMarker)
		{
			if (Input.IsMouseOwner()) {
				startMarker = Markers.TryFind("FocusLow");
				endMarker = Markers.TryFind("FocusHigh");
			} else {
				startMarker = Markers.TryFind("NormalLow");
				endMarker = Markers.TryFind("NormalHigh");
			}
		}

		float dragInitialOffset;
		float dragInitialDelta;

		public void SetValueFromCurrentMousePosition(bool draggingJustBegun)
		{
			if (Rail == null) {
				return;
			}
			float railLength = Rail.CalcLength();
			if (railLength <= 0) {
				return;
			}
			Matrix32 transform = Rail.LocalToWorldTransform.CalcInversed();
			Vector2 p = transform.TransformVector(Input.MousePosition);
			float offset = Rail.CalcOffset(p) / railLength;
			if (RangeMax <= RangeMin) {
				return;
			}
			float v = offset * (RangeMax - RangeMin) + RangeMin;
			if (draggingJustBegun) {
				dragInitialDelta = Value - v;
				dragInitialOffset = offset;
				return;
			}
			if (offset > dragInitialOffset && dragInitialOffset < 1) {
				Value = v + dragInitialDelta * (1 - (offset - dragInitialOffset) / (1 - dragInitialOffset));
			} else if (offset < dragInitialOffset && dragInitialOffset > 0) {
				Value = v + dragInitialDelta * (1 - (dragInitialOffset - offset) / dragInitialOffset);
			} else {
				Value = v + dragInitialDelta;
			}
			if (Changed != null) {
				Changed();
			}
		}
	}
}
