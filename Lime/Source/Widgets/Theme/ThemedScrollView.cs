#if !ANDROID && !iOS
using System;

namespace Lime
{
	public class ThemedScrollView : Frame
	{
		public override bool IsNotDecorated() => false;

		public ScrollView Behaviour { get; set; }
		public ScrollDirection Direction;

		public float ScrollPosition
		{
			get { return Behaviour.ScrollPosition; }
			set { Behaviour.ScrollPosition = value; }
		}

		public float MinScrollPosition
		{
			get { return Behaviour.MinScrollPosition; }
		}

		public float MaxScrollPosition
		{
			get { return Behaviour.MaxScrollPosition; }
		}

		public Widget Content
		{
			get { return Behaviour.Content; }
		}

		public ThemedScrollView(ScrollDirection scrollDirection = ScrollDirection.Vertical)
		{
			var slider = new Widget();
			slider.Size = new Vector2(10, 5);
			Direction = scrollDirection;
			if (scrollDirection == ScrollDirection.Vertical) { // Small icons
				slider.CompoundPresenter.Add(new DelegatePresenter<Widget>(_ => {
					PrepareRendererState();
					Renderer.DrawRect(new Vector2(Width - slider.Width, 0), Size, Theme.Colors.ScrollbarBackground);
					slider.PrepareRendererState();
					Renderer.DrawRect(new Vector2(2, 0), new Vector2(slider.Width - 2, slider.Height), Theme.Colors.ScrollbarThumb);
				}));
			} else { // Linear
				slider.CompoundPresenter.Add(new DelegatePresenter<Widget>(_ => {
					PrepareRendererState();
					Renderer.DrawRect(new Vector2(0, Height - slider.Height), Size, Theme.Colors.ScrollbarBackground);
					slider.PrepareRendererState();
					Renderer.DrawRect(new Vector2(2, 0), new Vector2(slider.Width - 2, slider.Height), Theme.Colors.ScrollbarThumb);
				}));
			}
			var ae = new AnimationEngineDelegate();
			ae.OnRunAnimation = (_, marker, animationTimeCorrection) => {
				slider.Opacity = marker == "Show" ? 1 : 0;
				return true;
			};
			slider.DefaultAnimation.AnimationEngine = ae;
			Behaviour = new ScrollViewWithSlider(this, slider, scrollDirection) {
				ScrollBySlider = true
			};
		}
	}
}
#endif
