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

		public event Action Changed;
		public bool Enabled;

		private float value;

		public Slider()
		{
			RangeMin = 0;
			RangeMax = 100;
			Value = 0;
			Enabled = true;
		}

		private Widget Thumb
		{
			get { return GetThumb(); }
		}

		private Widget ActiveBar
		{
			get { return GetActiveBar(); }
		}

		private Widget GetThumb()
		{
			var thumb = Nodes.TryFind("SliderThumb") as Widget;
			if (thumb == null) {
				thumb = Nodes.TryFind("Thumb") as Widget;
			}
			return thumb;
		}

		private Widget GetActiveBar()
		{
			var activeBar = Nodes.TryFind("SliderActive") as Widget;
			return activeBar;
		}

		private Spline Rail
		{
			get { return Nodes.TryFind("Rail") as Spline; }
		}

		protected override void SelfUpdate(int delta)
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
				TryRunAnimation("Press");
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
			if (RangeMax > RangeMin) {
				var t = (Value - RangeMin) / (RangeMax - RangeMin);
				var pos = Rail.CalcPoint(t * Rail.CalcLengthRough());
				Thumb.Position = Rail.CalcTransitionToSpaceOf(this) * pos;
				if (ActiveBar != null) {
					ActiveBar.Visible = true;
					ActiveBar.Scale = new Vector2(t, 1.0f);
				}
			}
		}

		private void Release()
		{
			TryRunAnimation("Normal");
			if (Input.IsMouseOwner()) {
				Input.ReleaseMouse();
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
	}
}
