#if !ANDROID && !iOS
using System;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	public class ThemedTabBar : TabBar
	{
		public override bool IsNotDecorated() => false;

		public ThemedTabBar()
		{
			Layout = new HBoxLayout();
		}
	}

	[YuzuDontGenerateDeserializer]
	public class ThemedTab : Tab
	{
		public override bool IsNotDecorated() => false;

		public ThemedTab()
		{
			Padding = Theme.Metrics.ControlsPadding + new Thickness(right: 5.0f);
			MinSize = Theme.Metrics.MinTabSize;
			MaxSize = Theme.Metrics.MaxTabSize;
			Size = MinSize;
			Layout = new HBoxLayout();
			var caption = new SimpleText {
				Id = "TextPresenter",
				ForceUncutText = false,
				FontHeight = Theme.Metrics.TextHeight,
				HAlignment = HAlignment.Center,
				VAlignment = VAlignment.Center,
				OverflowMode = TextOverflowMode.Ellipsis,
				LayoutCell = new LayoutCell(Alignment.Center)
			};
			var presenter = new TabPresenter(caption);
			Presenter = presenter;
			DefaultAnimation.AnimationEngine = new AnimationEngineDelegate {
				OnRunAnimation = (animation, markerId, animationTimeCorrection) => {
					presenter.SetState(markerId);
					return true;
				}
			};
			var closeButton = new ThemedTabCloseButton { Id = "CloseButton" };
			AddNode(caption);
			AddNode(closeButton);
			LateTasks.Add(Theme.MouseHoverInvalidationTask(this));
		}

		class TabPresenter : IPresenter
		{
			private SimpleText label;
			private bool active;

			public TabPresenter(SimpleText label) { this.label = label; }

			public void SetState(string state)
			{
				CommonWindow.Current.Invalidate();
				active = state == "Active";
				label.Color = active ? Theme.Colors.BlackText : Theme.Colors.GrayText;
			}

			public bool PartialHitTest(Node node, ref HitTestArgs args)
			{
				return node.PartialHitTest(ref args);
			}

			public Lime.RenderObject GetRenderObject(Node node)
			{
				var widget = (Widget)node;
				var ro = RenderObjectPool<RenderObject>.Acquire();
				ro.CaptureRenderState(widget);
				ro.Color = active || widget.IsMouseOverThisOrDescendant() ? Theme.Colors.TabActive : Theme.Colors.TabNormal;
				ro.Size = widget.Size;
				return ro;
			}

			private class RenderObject : WidgetRenderObject
			{
				public Color4 Color;
				public Vector2 Size;

				public override void Render()
				{
					PrepareRenderState();
					Renderer.DrawRect(Vector2.Zero, Size - new Vector2(1, 0), Color);
				}
			}
		}
	}

	public class VectorShapeButtonPresenter : IPresenter
	{
		private readonly VectorShape Shape;

		public VectorShapeButtonPresenter(VectorShape shape)
		{
			Shape = shape;
		}

		Color4 color;

		public bool PartialHitTest(Node node, ref HitTestArgs args)
		{
			return node.PartialHitTest(ref args);
		}

		public void SetState(string state)
		{
			CommonWindow.Current.Invalidate();
			if (state == "Normal") {
				color = Theme.Colors.CloseButtonNormal;
			} else if (state == "Focus") {
				color = Theme.Colors.CloseButtonHovered;
			} else {
				color = Theme.Colors.CloseButtonPressed;
			}
		}

		public Lime.RenderObject GetRenderObject(Node node)
		{
			var widget = (Widget)node;
			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.CaptureRenderState(widget);
			ro.Size = widget.Size;
			ro.Color = color;
			ro.Shape = Shape;
			return ro;
		}

		private class RenderObject : WidgetRenderObject
		{
			public Vector2 Size;
			public Color4 Color;
			public VectorShape Shape;

			public override void Render()
			{
				var transform = Matrix32.Scaling(Size);
				PrepareRenderState();
				Shape.Draw(transform, Color);
			}

			protected override void OnRelease()
			{
				Shape = null;
			}
		}
	}

	[YuzuDontGenerateDeserializer]
	public class ThemedTabCloseButton : Button
	{
		private WidgetFlatFillPresenter fill;

		public override bool IsNotDecorated() => false;

		public ThemedTabCloseButton()
		{
			var presenter = new VectorShapeButtonPresenter(new VectorShape {
				new VectorShape.Line(0.3f, 0.3f, 0.7f, 0.7f, Color4.White, 0.075f * 1.5f),
				new VectorShape.Line(0.3f, 0.7f, 0.7f, 0.3f, Color4.White, 0.0751f * 1.5f),
			});
			fill = new WidgetFlatFillPresenter(Theme.Colors.CloseButtonFocusBorderNormal);
			LayoutCell = new LayoutCell(Alignment.Center, stretchX: 0);
			MinMaxSize = Theme.Metrics.CloseButtonSize;
			DefaultAnimation.AnimationEngine = new AnimationEngineDelegate {
				OnRunAnimation = (animation, markerId, animationTimeCorrection) => {
					presenter.SetState(markerId);
					return true;
				}
			};
			CompoundPresenter.Add(presenter);
			CompoundPresenter.Add(fill);
		}

		public override void OnUpdate(float delta)
		{
			base.OnUpdate(delta);
			if (IsMouseOver()) {
				fill.Color = Theme.Colors.CloseButtonFocusBorderHovered;
			} else {
				fill.Color = Theme.Colors.CloseButtonFocusBorderNormal;
			}
		}
	}
}

#endif
