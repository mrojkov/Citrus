#if !ANDROID && !iOS
using System;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	public class ThemedButton : Button
	{
		public override bool IsNotDecorated() => false;

		public ThemedButton(string caption) : this()
		{
			Text = caption;
		}

		public ThemedButton()
		{
			var presenter = new ButtonPresenter();
			Nodes.Clear();
			MinMaxSize = Theme.Metrics.DefaultButtonSize;
			Size = MinSize;
			Padding = Theme.Metrics.ControlsPadding;
			Presenter = presenter;
			PostPresenter = new Theme.KeyboardFocusBorderPresenter(2.0f);
			DefaultAnimation.AnimationEngine = new AnimationEngineDelegate {
				OnRunAnimation = (animation, markerId, animationTimeCorrection) => {
					presenter.SetState(markerId);
					return true;
				}
			};
			var caption = new SimpleText {
				Id = "TextPresenter",
				TextColor = Theme.Colors.BlackText,
				FontHeight = Theme.Metrics.TextHeight,
				HAlignment = HAlignment.Center,
				VAlignment = VAlignment.Center,
				OverflowMode = TextOverflowMode.Ellipsis
			};
			AddNode(caption);
			TabTravesable = new TabTraversable();
			caption.ExpandToContainerWithAnchors();
		}

		public interface IButtonPresenter
		{
			void SetState(string state);
		}

		public class ButtonPresenter : IPresenter, IButtonPresenter
		{
			private ColorGradient innerGradient;

			public ButtonPresenter()
			{
				innerGradient = Theme.Colors.ButtonDefault;
			}

			public virtual void SetState(string state)
			{
				CommonWindow.Current.Invalidate();
				switch (state) {
				case "Press":
					innerGradient = Theme.Colors.ButtonPress;
					break;
				case "Focus":
					innerGradient = Theme.Colors.ButtonHover;
					break;
				case "Disable":
					innerGradient = Theme.Colors.ButtonDisable;
					break;
				default:
					innerGradient = Theme.Colors.ButtonDefault;
					break;
				}
			}

			public IPresenter Clone() => (IPresenter)MemberwiseClone();

			public bool PartialHitTest(Node node, ref HitTestArgs args)
			{
				return node.PartialHitTest(ref args);
			}

			public Lime.RenderObject GetRenderObject(Node node)
			{
				var widget = (Widget)node;
				var ro = RenderObjectPool<RenderObject>.Acquire();
				ro.CaptureRenderState(widget);
				ro.Size = widget.Size;
				ro.Gradient = innerGradient;
				ro.BorderColor = Theme.Colors.ControlBorder;
				return ro;
			}

			private class RenderObject : WidgetRenderObject
			{
				public Vector2 Size;
				public ColorGradient Gradient;
				public Color4 BorderColor;

				public override void Render()
				{
					PrepareRenderState();
					Renderer.DrawVerticalGradientRect(Vector2.Zero, Size, Gradient);
					Renderer.DrawRectOutline(Vector2.Zero, Size, BorderColor);
				}
			}
		}
	}
}
#endif
