#if !ANDROID && !iOS
using System;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
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
			slider.CompoundPresenter.Add(new SliderPresenter(this));
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

		public class SliderPresenter : IPresenter
		{
			private ThemedScrollView scrollView;

			public SliderPresenter(ThemedScrollView scrollView)
			{
				this.scrollView = scrollView;
			}

			public IPresenter Clone() => (IPresenter)MemberwiseClone();

			public Lime.RenderObject GetRenderObject(Node node)
			{
				var slider = (Widget)node;
				var ro = RenderObjectPool<RenderObject>.Acquire();
				ro.ScrollViewTransform = scrollView.LocalToWorldTransform;
				ro.ScrollViewBlending = scrollView.GlobalBlending;
				ro.ScrollViewShader = scrollView.GlobalShader;
				ro.ScrollViewSize = scrollView.Size;
				ro.SliderTransform = slider.LocalToWorldTransform;
				ro.SliderBlending = slider.GlobalBlending;
				ro.SliderShader = slider.GlobalShader;
				ro.SliderSize = slider.Size;
				ro.BackgroundColor = Theme.Colors.ScrollbarBackground;
				ro.ThumbColor = Theme.Colors.ScrollbarThumb;
				ro.Direction = scrollView.Direction;
				return ro;
			}

			public bool PartialHitTest(Node node, ref HitTestArgs args) => false;

			private class RenderObject : Lime.RenderObject
			{
				public Matrix32 SliderTransform;
				public Blending SliderBlending;
				public ShaderId SliderShader;
				public Vector2 SliderSize;
				public Matrix32 ScrollViewTransform;
				public Blending ScrollViewBlending;
				public ShaderId ScrollViewShader;
				public Vector2 ScrollViewSize;
				public Color4 BackgroundColor;
				public Color4 ThumbColor;
				public ScrollDirection Direction;

				public override void Render()
				{
					Renderer.Transform1 = ScrollViewTransform;
					Renderer.Blending = ScrollViewBlending;
					Renderer.Shader = ScrollViewShader;
					if (Direction == ScrollDirection.Vertical) {
						Renderer.DrawRect(new Vector2(ScrollViewSize.X - SliderSize.X, 0), ScrollViewSize, BackgroundColor);
					} else {
						Renderer.DrawRect(new Vector2(0, ScrollViewSize.Y - SliderSize.Y), ScrollViewSize, BackgroundColor);
					}
					Renderer.Transform1 = SliderTransform;
					Renderer.Blending = SliderBlending;
					Renderer.Shader = SliderShader;
					Renderer.DrawRect(new Vector2(2, 0), new Vector2(SliderSize.X - 2, SliderSize.Y), ThumbColor);
				}
			}
		}
	}
}
#endif
