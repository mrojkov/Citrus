using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class ScrollViewWithSlider : ScrollView
	{
		public float sliderMinLength;
		private Widget slider;
		private float sliderTimeToLive;

		public Widget Slider
		{
			get { return slider; }
			set { SetSlider(value); }
		}

		public ScrollViewWithSlider(Frame frame, ScrollDirection scrollDirection = ScrollDirection.Vertical, bool processChildrenFirst = false)
			: base(frame, scrollDirection, processChildrenFirst)
		{
			Content.LateTasks.Add(ShowSliderWhenScrollingTask());
			Content.LateTasks.Add(AdjustSliderTask());
		}

		public void ForceShowSlider(float time = 1)
		{
			sliderTimeToLive = time;
		}

		private IEnumerator<object> ShowSliderWhenScrollingTask()
		{
			yield return Task.WaitWhile(() => Slider == null);
			while (true) {
				if (IsScrolling() || IsDragging || IsScrollingByMouseWheel) {
					sliderTimeToLive = 1;
				}
				yield return null;
			}
		}

		private void SetSlider(Widget slider)
		{
			this.slider = slider;
			sliderMinLength = ProjectToScrollAxis(slider.Size);
			slider.RunAnimation("Def");
			// Bring the slider above the content widget
			slider.Unlink();
			slider.PushToNode(Frame);
		}

		private IEnumerator<object> AdjustSliderTask()
		{
			if (Slider == null) {
				yield break;
			}
			while (true) {
				ShowSliderIfNeeded();
				MoveSlider();
				ResizeSlider();
				yield return null;
			}
		}

		private void ShowSliderIfNeeded()
		{
			if (sliderTimeToLive > 0) {
				sliderTimeToLive -= Task.Delta;
				ChangeAnimation(Slider, "Show");
			} else if (Slider.CurrentAnimation != "Def") {
				ChangeAnimation(Slider, "Hide");
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
				sliderPos = p * ProjectToScrollAxis(Frame.Size - Slider.Size) / MaxScrollPosition;
			}
			if (ScrollDirection == ScrollDirection.Vertical) {
				Slider.Y = sliderPos;
			} else {
				Slider.X = sliderPos;
			}
		}

		private void ResizeSlider()
		{
			if (ScrollDirection == ScrollDirection.Vertical) {
				Slider.Height = CalcSliderLength();
			} else {
				Slider.Width = CalcSliderLength();
			}
		}

		private float CalcSliderLength()
		{
			float pagesCount = ContentLength / ProjectToScrollAxis(Frame.Size);
			if (pagesCount < float.Epsilon) {
				return 0;
			}
			float length = ProjectToScrollAxis(Frame.Size) / pagesCount;
			float t = 0;
			if (ScrollPosition > MaxScrollPosition) {
				t = (ScrollPosition - MaxScrollPosition) / BounceZoneThickness;
			} else if (ScrollPosition < MinScrollPosition) {
				t = (MinScrollPosition - ScrollPosition) / BounceZoneThickness;
			}
			length = t.Lerp(length, sliderMinLength);
			length = length.Clamp(sliderMinLength, ProjectToScrollAxis(Frame.Size));
			return length;
		}
	}
}
