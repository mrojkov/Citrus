﻿using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class Slider : Widget
	{
		[ProtoMember (1)]
		public float RangeMin { get; set; }

		[ProtoMember (2)]
		public float RangeMax { get; set; }

		[ProtoMember (3)]
		public float Value
		{
			get { return Utils.Clamp (value, RangeMin, RangeMax); }
			set { this.value = value; }
		}

		float value;
		float delta;

		public Slider ()
		{
			RangeMin = 0;
			RangeMax = 100;
			Value = 0;
		}

		void UpdateHelper (int delta)
		{
			var rootFrame = RootFrame.Instance;
			var thumb = Find<Widget> ("SliderThumb", false);
			if (thumb != null) {
				if (thumb.HitTest (Input.MousePosition)) {
					if (rootFrame.ActiveWidget == null) {
						thumb.PlayAnimation ("Focus");
						rootFrame.ActiveWidget = this;
					}
				} else {
					if (rootFrame.ActiveWidget == this && !Input.GetKey (Key.Mouse0)) {
						thumb.PlayAnimation ("Normal");
						rootFrame.ActiveWidget = null;
					}
				}
				if (rootFrame.ActiveWidget == this) {
					if (Input.GetKeyDown (Key.Mouse0)) {
						Input.ConsumeKeyEvent (Key.Mouse0, true);
						ScrollSlider (true);
					} else if (Input.GetKey (Key.Mouse0)) {
						ScrollSlider (false);
					}
				}
				Marker m1, m2;
				if (rootFrame.ActiveWidget == this) {
					m1 = Markers.Get ("FocusLow");
					m2 = Markers.Get ("FocusHigh");
				} else {
					m1 = Markers.Get ("NormalLow");
					m2 = Markers.Get ("NormalHigh");
				}
				if (m1 != null && m2 != null) {
					if (RangeMax > RangeMin) {
						float t1 = Animator.FramesToMsecs (m1.Frame);
						float t2 = Animator.FramesToMsecs (m2.Frame);
						AnimationTime = (int)(t1 + (Value - RangeMin) / (RangeMax - RangeMin) * (t2 - t1));
					}
				}
			}
			if (rootFrame.ActiveWidget != this && thumb.CurrentAnimation != "Normal") {
				thumb.PlayAnimation ("Normal");
			}
		}

		public override void Update (int delta)
		{
			if (worldShown) {
				UpdateHelper (delta);
			}
			base.Update (delta);
		}

		void ScrollSlider (bool begin)
		{
			Spline rail = Find<Spline>("Rail", false);
			if (rail != null) {
				float railLength = rail.CalcLength ();
				if (railLength > 0) {
					Matrix32 transform = rail.WorldMatrix.CalcInversed ();
					Vector2 p = transform.TransformVector (Input.MousePosition);
					float offset = rail.CalcOffset (p) / railLength;
					if (RangeMax > RangeMin) {
						float v = offset * (RangeMax - RangeMin) + RangeMin;
						if (begin) {
							delta = Value - v;
						} else {
							Value = v + delta;
						}
					}
				}
			}
		}
	}
}
