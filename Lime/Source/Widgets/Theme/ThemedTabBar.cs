#if !ANDROID && !iOS
using System;

namespace Lime
{
	public class ThemedTabBar : TabBar
	{
		public override bool IsNotDecorated() => false;

		public ThemedTabBar()
		{
			Layout = new HBoxLayout();
		}
	}

	public class ThemedTab : Tab
	{
		public override bool IsNotDecorated() => false;

		public ThemedTab()
		{
			Padding = Theme.Metrics.ControlsPadding;
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
				OnRunAnimation = (animation, markerId) => {
					presenter.SetState(markerId);
					return true;
				}
			};
			var closeButton = new ThemedTabCloseButton { Id = "CloseButton" };
			AddNode(caption);
			AddNode(closeButton);
			LateTasks.Add(Theme.MouseHoverInvalidationTask(this));
		}

		class TabPresenter : CustomPresenter
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

			public override void Render(Node node)
			{
				var widget = node.AsWidget;
				widget.PrepareRendererState();
				var color = active || widget.IsMouseOverThisOrDescendant() ? Theme.Colors.TabActive : Theme.Colors.TabNormal;
				Renderer.DrawRect(Vector2.Zero, widget.Size - new Vector2(1, 0), color);
			}

			public override bool PartialHitTest(Node node, ref HitTestArgs args)
			{
				return node.PartialHitTest(ref args);
			}
		}
	}

	public class VectorShapeButtonPresenter : CustomPresenter
	{
		private readonly VectorShape Shape;

		public VectorShapeButtonPresenter(VectorShape shape)
		{
			Shape = shape;
		}

		Color4 color;

		public override void Render(Node node)
		{
			var widget = node.AsWidget;
			widget.PrepareRendererState();
			var transform = Matrix32.Scaling(widget.Size);
			Shape.Draw(transform, color);
		}

		public override bool PartialHitTest(Node node, ref HitTestArgs args)
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
	}

	public class ThemedTabCloseButton : Button
	{
		public override bool IsNotDecorated() => false;

		public ThemedTabCloseButton()
		{
			var presenter = new VectorShapeButtonPresenter(new VectorShape {
				new VectorShape.Line(0.3f, 0.3f, 0.7f, 0.7f, Color4.White, 0.075f * 1.5f),
				new VectorShape.Line(0.3f, 0.7f, 0.7f, 0.3f, Color4.White, 0.0751f * 1.5f),
			});
			LayoutCell = new LayoutCell(Alignment.Center, stretchX: 0);
			Presenter = presenter;
			MinMaxSize = Theme.Metrics.CloseButtonSize;
			DefaultAnimation.AnimationEngine = new AnimationEngineDelegate {
				OnRunAnimation = (animation, markerId) => {
					presenter.SetState(markerId);
					return true;
				}
			};
		}

	}
}

#endif