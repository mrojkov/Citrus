#if !ANDROID && !iOS && !UNITY
using System;
using System.Collections.Generic;

namespace Lime
{
	public class DesktopTheme : Theme
	{
		public static class Metrics
		{
			public static readonly int TextHeight = 18;
			public static readonly Vector2 DefaultButtonSize = new Vector2(75, 23);
			public static readonly Thickness ControlsPadding = new Thickness(2);
		}

		public static class Colors
		{
			public static readonly Color4 BlackText = new Color4(0, 0, 0);
			public static readonly Color4 WhiteBackground = new Color4(255, 255, 255);
			public static readonly Color4 GrayBackground = new Color4(240, 240, 240);
			public static readonly Color4 SelectedBackground = new Color4(140, 170, 255);
			public static readonly Color4 ControlBorder = new Color4(172, 172, 172);
			public static readonly ColorGradient ButtonDefault = new ColorGradient(new Color4(239, 239, 239), new Color4(229, 229, 229));
			public static readonly ColorGradient ButtonHover = new ColorGradient(new Color4(235, 244, 252), new Color4(222, 238, 252));
			public static readonly ColorGradient ButtonPress = new ColorGradient(new Color4(215, 234, 252), new Color4(199, 226, 252));
			public static readonly ColorGradient ButtonDisable = new ColorGradient(new Color4(244, 244, 244), new Color4(244, 244, 244));
			public static readonly Color4 SeparatorColor = new Color4(255, 255, 255);
			public static readonly Color4 KeyboardFocusBorder = new Color4(150, 200, 255);
		}

		public DesktopTheme()
		{
			Decorators[typeof(SimpleText)] = DecorateSimpleText;
			Decorators[typeof(Button)] = DecorateButton;
			Decorators[typeof(EditBox)] = DecorateEditBox;
			Decorators[typeof(WindowWidget)] = DecorateWindowWidget;
			Decorators[typeof(TextView)] = DecorateTextView;
			Decorators[typeof(ComboBox)] = DecorateComboBox;
			Decorators[typeof(FileChooserButton)] = DecorateFileChooserButton;
			Decorators[typeof(HSplitter)] = DecorateSplitter;
			Decorators[typeof(VSplitter)] = DecorateSplitter;
		}

		private void DecorateSplitter(Widget widget)
		{
			var splitter = (Splitter)widget;
			splitter.SeparatorColor = Colors.SeparatorColor;
			splitter.SeparatorWidth = 2;
			splitter.SeparatorActiveAreaWidth = 10;
		}

		private void DecorateButton(Widget widget)
		{
			var button = (Button)widget;
			var presenter = new ButtonPresenter();
			button.Nodes.Clear();
			button.MinMaxSize = Metrics.DefaultButtonSize;
			button.Size = button.MinSize;
			button.Padding = Metrics.ControlsPadding;
			button.Presenter = presenter;
			button.PostPresenter = new KeyboardFocusBorderPresenter();
			button.DefaultAnimation.AnimationEngine = new ButtonAnimationEngine(presenter);
			var caption = new SimpleText {
				Id = "TextPresenter",
				TextColor = Colors.BlackText,
				FontHeight = Metrics.TextHeight,
				HAlignment = HAlignment.Center,
				VAlignment = VAlignment.Center,
				OverflowMode = TextOverflowMode.Ellipsis
			};
			button.AddNode(caption);
			button.Focusable = new Focusable();
			ExpandToContainer(caption);
		}

		private void DecorateFileChooserButton(Widget widget)
		{
			var fc = (FileChooserButton)widget;
			fc.Layout = new HBoxLayout();
			var label = new SimpleText {
				Id = "Label",
				AutoSizeConstraints = false,
				MinMaxHeight = Metrics.DefaultButtonSize.Y,
				Padding = Metrics.ControlsPadding,
				LayoutCell = new LayoutCell { StretchX = float.MaxValue }
			};
			var button = new Button {
				Id = "Button",
				Text = "...",
				MinMaxWidth = 20
			};
			fc.PostPresenter = new BorderedFramePresenter(Colors.GrayBackground, Colors.ControlBorder);
			fc.AddNode(label);
			fc.AddNode(button);
		}
		
		private void DecorateSimpleText(Widget widget)
		{
			var text = (SimpleText)widget;
			text.AutoSizeConstraints = true;
			text.Localizable = true;
			text.TextColor = Color4.White;
			text.Color = Colors.BlackText;
			text.Font = new SerializableFont();
			text.FontHeight = Metrics.TextHeight;
			text.HAlignment = HAlignment.Left;
			text.VAlignment = VAlignment.Center;
			text.OverflowMode = TextOverflowMode.Ellipsis;
			text.TrimWhitespaces = true;	
			text.Size = text.MinSize;
		}
				
		private void DecorateWindowWidget(Widget widget)
		{
			widget.CompoundPresenter.Push(new WindowWidgetPresenter());
		}
		
		private void DecorateEditBox(Widget widget)
		{
			DecorateSimpleText(widget);
			var eb = (EditBox)widget;
			eb.AutoSizeConstraints = false;
			eb.MinSize = Metrics.DefaultButtonSize;
			eb.MaxHeight = eb.MinHeight;
			eb.HAlignment = HAlignment.Left;
			eb.Localizable = false;	
			eb.TrimWhitespaces = false;
			eb.VAlignment = VAlignment.Center;
			eb.Padding = Metrics.ControlsPadding;
			var editorParams = new EditorParams { MaxLength = 100, MaxLines = 1, RevokeFocusOnEnter = true };
			new CaretDisplay(
				eb, eb.Caret,
				new CaretParams { CaretPresenter = new VerticalLineCaret() });
			new Editor(eb, eb.Caret, editorParams);
			eb.Focusable = new Focusable();
			eb.CompoundPresenter.Add(new BorderedFramePresenter(Colors.WhiteBackground, Colors.ControlBorder));
			eb.CompoundPostPresenter.Add(new KeyboardFocusBorderPresenter());
		}

		private void DecorateComboBox(Widget widget)
		{
			var comboBox = (ComboBox)widget;
			comboBox.MinSize = Metrics.DefaultButtonSize;
			comboBox.MaxHeight = Metrics.DefaultButtonSize.Y;
			comboBox.Focusable = new Focusable();
			var text = new SimpleText {
				Id = "Label",
				VAlignment = VAlignment.Center,
			};
			text.CompoundPresenter.Add(new ComboBoxPresenter());
			comboBox.PostPresenter = new KeyboardFocusBorderPresenter();
			text.Padding = Metrics.ControlsPadding;
			text.Padding.Right = ComboBoxPresenter.IconWidth;
			comboBox.AddNode(text);
			ExpandToContainer(text);
		}

		private void DecorateTextView(Widget widget)
		{
			var tv = (TextView)widget;
			tv.CompoundPresenter.Push(new BorderedFramePresenter(Colors.WhiteBackground, Colors.ControlBorder));
		}

		private static void ExpandToContainer(Widget widget)
		{
			widget.Anchors = Anchors.None;
			widget.Size = widget.ParentWidget.Size;
			widget.Anchors = Anchors.LeftRightTopBottom;
		}

		class BorderedFramePresenter : CustomPresenter
		{
			private readonly Color4 innerColor;
			private readonly Color4 borderColor;

			public BorderedFramePresenter(Color4 innerColor, Color4 borderColor)
			{
				this.innerColor = innerColor;
				this.borderColor = borderColor;
			}

			public override void Render(Node node)
			{
				var widget = node.AsWidget;
				widget.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, widget.Size, innerColor);
				Renderer.DrawRectOutline(Vector2.Zero, widget.Size, borderColor);
			}
		}

		class KeyboardFocusBorderPresenter : CustomPresenter
		{
			public override void Render(Node node)
			{
				if (node == KeyboardFocus.Instance.Focused) {
					var widget = node.AsWidget;
					widget.PrepareRendererState();
					Renderer.DrawRectOutline(Vector2.Zero, widget.Size, Colors.KeyboardFocusBorder, 2);
				}
			}
		}

		class ComboBoxPresenter : CustomPresenter
		{
			public const float IconWidth = 20;
			private VectorShape icon = new VectorShape {
				new VectorShape.Line(0, 0.1f, 0, 0.9f, Colors.ControlBorder, 0.05f),
				new VectorShape.Line(0.5f, 0.8f, 0.7f, 0.65f, Colors.BlackText, 0.07f),
				new VectorShape.Line(0.5f, 0.8f, 0.3f, 0.65f, Colors.BlackText, 0.07f),
				new VectorShape.Line(0.5f, 0.2f, 0.7f, 0.35f, Colors.BlackText, 0.07f),
				new VectorShape.Line(0.5f, 0.2f, 0.3f, 0.35f, Colors.BlackText, 0.07f),
			};

			public override void Render(Node node)
			{
				var widget = node.AsWidget;
				widget.PrepareRendererState();
				Renderer.DrawVerticalGradientRect(Vector2.Zero, widget.Size, Colors.ButtonDefault);
				Renderer.DrawRectOutline(Vector2.Zero, widget.Size, Colors.ControlBorder);
				var transform = Matrix32.Scaling(IconWidth, widget.Height) * Matrix32.Translation(widget.Width - IconWidth, 0);
				icon.Draw(transform);
			}
		}

		class ButtonAnimationEngine : Lime.AnimationEngine
		{
			private readonly ButtonPresenter presenter;

			public ButtonAnimationEngine(ButtonPresenter presenter)
			{
				this.presenter = presenter;
			}

			public override bool TryRunAnimation(Animation animation, string markerId)
			{
				presenter.SetState(markerId);
				return true;
			}
		}

		class ButtonPresenter : CustomPresenter
		{
			private ColorGradient innerGradient;

			public void SetState(string state)
			{
				CommonWindow.Current.Invalidate();
				switch (state) {
					case "Press":
						innerGradient = Colors.ButtonPress;
						break;
					case "Focus":
						innerGradient = Colors.ButtonHover;
						break;
					case "Disable":
						innerGradient = Colors.ButtonDisable;
						break;
					default:
						innerGradient = Colors.ButtonDefault;
						break;
				}
			}

			public override void Render(Node node)
			{
				var widget = node.AsWidget;
				widget.PrepareRendererState();
				Renderer.DrawVerticalGradientRect(Vector2.Zero, widget.Size, innerGradient);
				Renderer.DrawRectOutline(Vector2.Zero, widget.Size, Colors.ControlBorder);
			}

			public override bool PartialHitTest(Node node, ref HitTestArgs args)
			{
				return node.PartialHitTest(ref args);
			}
		}

		class WindowWidgetPresenter : CustomPresenter
		{
			public override void Render(Node node)
			{
				var widget = node.AsWidget;
				widget.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, widget.Size, Colors.GrayBackground);
			}
		}
	}
}
#endif
