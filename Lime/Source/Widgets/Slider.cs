using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class Slider : GUIWidget
	{
		[ProtoMember (1)]
		public float RangeMin { get; set; }

		[ProtoMember (2)]
		public float RangeMax { get; set; }

		[ProtoMember (3)]
		public float Value { get; set; }

		public Slider ()
		{
			RangeMin = 0;
			RangeMax = 100;
			Value = 0;
		}

		protected override void Reset ()
		{
			var thumb = Find<Widget> ("SliderThumb", false);
			if (thumb != null) {
				thumb.PlayAnimation ("Normal");
			}
		}

		public override void UpdateGUI ()
		{
			var thumb = Find<Widget> ("SliderThumb", false);
			if (thumb != null) {
				if (thumb.HitTest (Input.MousePosition)) {
					if (GUIWidget.FocusedWidget == null) {
						thumb.PlayAnimation ("Focus");
						GUIWidget.FocusedWidget = this;
					}
				} else {
					if (GUIWidget.FocusedWidget == this && !Input.GetKey (Key.Mouse0)) {
						thumb.PlayAnimation ("Normal");
						GUIWidget.FocusedWidget = null;
					}
				}
				if (GUIWidget.FocusedWidget == this) {
					if (Input.GetKey (Key.Mouse0)) {
						Input.ConsumeKeyEvent (Key.Mouse0, true);
						SliderFollowByMouse ();
					}
				}
				Marker m1, m2;
				if (GUIWidget.FocusedWidget == this) {
					m1 = Markers.Get ("FocusLow");
					m2 = Markers.Get ("FocusHigh");
				} else {
					m1 = Markers.Get ("NormalLow");
					m2 = Markers.Get ("NormalHigh");
				}
				if (m1 != null && m2 != null) {
					if (RangeMax > RangeMin) {
						float value = Utils.Clamp (Value, RangeMin, RangeMax);
						float t1 = Animator.FramesToMsecs (m1.Frame);
						float t2 = Animator.FramesToMsecs (m2.Frame);
						AnimationTime = (int)(t1 + (Value - RangeMin) / (RangeMax - RangeMin) * (t2 - t1));
					}
				}
			}
		}

		void SliderFollowByMouse()
		{
			Spline rail = Find<Spline>("Rail", false);
			if (rail != null) {
				float railLength = rail.CalcLength ();
				if (railLength > 0) {
					Matrix32 transform = rail.WorldMatrix.CalcInversed ();
					Vector2 p = transform.TransformVector (Input.MousePosition);
					float offset = rail.CalcOffset (p) / railLength;
					if (RangeMax > RangeMin) {
						Value = offset * (RangeMax - RangeMin) + RangeMin;
					}
				}
			}
		}
	}
}
