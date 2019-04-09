using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class ScrollViewWithSlider : ScrollView
	{
		private float sliderMinLength;
		private Widget slider;
		private float sliderTimeToLive;

		public bool SliderIsDragging;

		public ScrollViewWithSlider(Frame frame, Widget slider = null, ScrollDirection scrollDirection = ScrollDirection.Vertical, bool processChildrenFirst = false)
			: base(frame, scrollDirection, processChildrenFirst)
		{
			this.slider = slider;
			frame.Layout = new Layout(ScrollDirection, Content, slider);
			if (slider != null) {
				sliderMinLength = ProjectToScrollAxis(slider.Size);
				slider.RunAnimation("Def");
				// Bring the slider above the content widget
				slider.Unlink();
				slider.PushToNode(Frame);
			}
			Content.LateTasks.Add(ShowSliderWhenScrollingTask());
			Content.LateTasks.Add(AdjustSliderTask());
			Content.LateTasks.Add(DragSliderTask());

		}

		IEnumerator<object> DragSliderTask()
		{
			while (true) {
				SliderIsDragging = false;
				if (CanScroll && ScrollBySlider && slider != null) {
					slider.HitTestTarget = true;
					var input = slider.Input;
					if (input.WasMousePressed()) {
						SliderIsDragging = true;
						var iniMousePos = input.MousePosition;
						var iniScrollPos = ScrollPosition;
						while (input.IsMousePressed()) {
							var projectedFrameSize = ProjectToScrollAxis(Frame.Size);
							if (projectedFrameSize > Mathf.ZeroTolerance) {
								var d = ProjectToScrollAxis(input.MousePosition - iniMousePos) * ContentLength / projectedFrameSize;
								ScrollPosition = Mathf.Clamp(((iniScrollPos + d).Round()), MinScrollPosition, MaxScrollPosition);
							}
							yield return null;
						}
					}
				}
				yield return null;
			}
		}

		public void ForceShowSlider(float time = 1)
		{
			sliderTimeToLive = time;
		}

		private IEnumerator<object> ShowSliderWhenScrollingTask()
		{
			yield return Task.WaitWhile(() => slider == null);
			while (true) {
				if (ScrollBySlider) {
					sliderTimeToLive = ContentLength > ProjectToScrollAxis(Frame.Size) ? 0.1f : 0;
				} else if (IsScrolling() || IsDragging) {
					sliderTimeToLive = 1;
				}
				yield return null;
			}
		}

		private IEnumerator<object> AdjustSliderTask()
		{
			if (slider == null) {
				yield break;
			}
			while (true) {
				ShowSliderIfNeeded();
				ResizeSlider();
				MoveSlider();
				yield return null;
			}
		}

		private void ShowSliderIfNeeded()
		{
			if (sliderTimeToLive > 0) {
				sliderTimeToLive -= Task.Current.Delta;
				ChangeAnimation(slider, "Show");
			} else if (slider.CurrentAnimation != "Def") {
				ChangeAnimation(slider, "Hide");
			}
		}

		private static void ChangeAnimation(Widget widget, string animation)
		{
			if (widget.CurrentAnimation != animation) {
				widget.RunAnimation(animation);
			}
		}

		private void MoveSlider()
		{
			float sliderPos = 0.0f;
			if (MaxScrollPosition > float.Epsilon) {
				float p = ScrollPosition.Clamp(MinScrollPosition, MaxScrollPosition);
				sliderPos = p * ProjectToScrollAxis(Frame.Size - slider.Size) / MaxScrollPosition;
			}
			if (ScrollDirection == ScrollDirection.Vertical) {
				slider.Y = sliderPos;
			} else {
				slider.X = sliderPos;
			}
		}

		private void ResizeSlider()
		{
			if (ScrollDirection == ScrollDirection.Vertical) {
				slider.Height = CalcSliderLength();
			} else {
				slider.Width = CalcSliderLength();
			}
		}

		private float CalcSliderLength()
		{
			float pagesCount = ContentLength / ProjectToScrollAxis(Frame.Size);
			if (pagesCount <= float.Epsilon || float.IsNaN(pagesCount)) {
				return 0;
			}
			float length = ProjectToScrollAxis(Frame.Size) / pagesCount;
			float t = 0;
			if (BounceZoneThickness != 0) {
				if (ScrollPosition > MaxScrollPosition) {
					t = (ScrollPosition - MaxScrollPosition) / BounceZoneThickness;
				} else if (ScrollPosition < MinScrollPosition) {
					t = (MinScrollPosition - ScrollPosition) / BounceZoneThickness;
				}
			}
			length = t.Lerp(length, sliderMinLength);
			length = length.Clamp(sliderMinLength, ProjectToScrollAxis(Frame.Size));
			return length;
		}

		[YuzuDontGenerateDeserializer]
		new class Layout : ScrollView.Layout
		{
			readonly Widget slider;

			public Layout(ScrollDirection direction, Widget content, Widget slider) : base(direction, content)
			{
				this.slider = slider;
			}

			public override void ArrangeChildren()
			{
				base.ArrangeChildren();
				if (slider != null) {
					if (direction == ScrollDirection.Vertical) {
						slider.X = Owner.Width - slider.Width;
					} else {
						slider.Y = Owner.Height - slider.Height;
					}
				}
			}
		}
	}
}
