using System;
using ProtoBuf;

namespace Lime
{
	public delegate void SliderChangeHandler(Slider slider);

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
			get { return Mathf.Clamp(value, RangeMin, RangeMax); }
			set { this.value = value; }
		}

		public SliderChangeHandler ValueChanged;

		float value;
		float offset0;
		float delta0;

		public Slider()
		{
			RangeMin = 0;
			RangeMax = 100;
			Value = 0;
		}

		void UpdateHelper(int delta)
		{
			var thumb = Find<Widget>("SliderThumb", false);
			if (thumb != null) {
				if (thumb.HitTest(Input.MousePosition) && Input.WasKeyPressed(Key.Mouse0)) {
					if (RootFrame.Instance.ActiveWidget == null) {
						thumb.RunAnimation("Focus");
						RootFrame.Instance.ActiveWidget = this;
					}
				} else {
					if (RootFrame.Instance.ActiveWidget == this && !Input.IsKeyPressed(Key.Mouse0)) {
						thumb.RunAnimation("Normal");
						RootFrame.Instance.ActiveWidget = null;
					}
				}
				if (RootFrame.Instance.ActiveWidget == this) {
					if (Input.WasKeyPressed(Key.Mouse0)) {
						Input.ConsumeKeyEvent(Key.Mouse0, true);
						ScrollSlider(true);
					} else if (Input.IsKeyPressed(Key.Mouse0)) {
						ScrollSlider(false);
					}
				}
				Marker m1, m2;
				if (RootFrame.Instance.ActiveWidget == this) {
					m1 = Markers.Get("FocusLow");
					m2 = Markers.Get("FocusHigh");
				} else {
					m1 = Markers.Get("NormalLow");
					m2 = Markers.Get("NormalHigh");
				}
				if (m1 != null && m2 != null) {
					if (RangeMax > RangeMin) {
						float t1 = Animator.FramesToMsecs(m1.Frame);
						float t2 = Animator.FramesToMsecs(m2.Frame);
						AnimationTime = (int)(t1 + (Value - RangeMin) / (RangeMax - RangeMin) * (t2 - t1));
					}
				}
			}
			if (RootFrame.Instance.ActiveWidget != this && thumb.CurrentAnimation != "Normal") {
				thumb.RunAnimation("Normal");
			}
			if (RootFrame.Instance.ActiveWidget == this) {
				RootFrame.Instance.ActiveWidgetUpdated = true;
			}
		}

		public override void Update(int delta)
		{
			if (globallyVisible) {
				UpdateHelper(delta);
			}
			base.Update(delta);
		}

		void ScrollSlider(bool begin)
		{
			Spline rail = Find<Spline>("Rail", false);
			if (rail != null) {
				float railLength = rail.CalcLength();
				if (railLength > 0) {
					Matrix32 transform = rail.GlobalMatrix.CalcInversed();
					Vector2 p = transform.TransformVector(Input.MousePosition);
					float offset = rail.CalcOffset(p) / railLength;
					if (RangeMax > RangeMin) {
						float v = offset * (RangeMax - RangeMin) + RangeMin;
						if (begin) {
							delta0 = Value - v;
							offset0 = offset;
						} else {
							if (offset > offset0 && offset0 < 1)
								Value = v + delta0 * (1 - (offset - offset0) / (1 - offset0));
							else if (offset < offset0 && offset0 > 0)
								Value = v + delta0 * (1 - (offset0 - offset) / offset0);
							else
								Value = v + delta0;
							if (ValueChanged != null) {
								ValueChanged(this);
							}
						}
					}
				}
			}
		}
	}
}
