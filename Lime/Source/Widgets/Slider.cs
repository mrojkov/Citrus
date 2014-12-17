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

		[ProtoMember(4)]
		public float Step { get; set; }

		public event Action DragStarted;
		public event Action DragEnded;
		public event Action Changed;
		public bool Enabled;

		private float value;

		public Slider()
		{
			RangeMin = 0;
			RangeMax = 100;
			Value = 0;
			Step = 0;
			Enabled = true;
		}

		private Widget Thumb
		{
			get { return GetThumb(); }
		}

		private Widget GetThumb()
		{
			var thumb = Nodes.TryFind("SliderThumb") as Widget;
			if (thumb == null) {
				thumb = Nodes.TryFind("Thumb") as Widget;
			}
			return thumb;
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
			if (Input.WasMousePressed() && Thumb.IsMouseOver()) {
				RunThumbAnimation("Press");
				Input.CaptureMouse();
				RaiseDragStarted();
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
			InterpolateGraphicsBetweenMinAndMaxMarkers();
			RefreshThumbPosition();
			if (!Input.IsMouseOwner()) {
				Release();
			}
		}

		private void RaiseDragEnded()
		{
			if (DragEnded != null) {
				DragEnded();
			}
		}

		private void RaiseDragStarted()
		{
			if (DragStarted != null) {
				DragStarted();
			}
		}

		private void InterpolateGraphicsBetweenMinAndMaxMarkers()
		{
			if (RangeMax - RangeMin < float.Epsilon) {
				return;
			}
			var mn = this.Markers.TryFind("NormalMin");
			var mx = this.Markers.TryFind("NormalMax");
			if (mn != null && mx != null) {
				var t = (Value - RangeMin) / (RangeMax - RangeMin);
				AnimationTime = Mathf.Lerp(t, mn.Time, mx.Time).Round();
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
			if (Input.IsMouseOwner()) {
				Input.ReleaseMouse();
			}
		}

		private void RunThumbAnimation(string name)
		{
			if (Thumb != null) {
				Thumb.TryRunAnimation(name);
			}
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
			if (Changed != null) {
				Changed();
			}
		}

		public void SetValue(float newValue)
		{
			if (Value == newValue) return;
			value = newValue;
			RaiseChanged();
		}
	}
}
