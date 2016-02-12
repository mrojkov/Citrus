using System;
using System.Collections.Generic;

namespace Lime
{
	public class DesktopTheme : Theme
	{
		public class ThemeConstants
		{
			public readonly int TextHeight = 18;
			public readonly Color4 TextColor = new Color4(0, 0, 0);
			public readonly Color4 TextViewBackground = new Color4(255, 255, 255);
			public readonly Color4 DialogBackground = new Color4(240, 240, 240);
			public readonly Color4 ButtonInactiveBorder = new Color4(172, 172, 172);
			public readonly Color4 ButtonActiveBorder = new Color4(51, 153, 255);
			public readonly ColorGradient ButtonDefault = new ColorGradient(new Color4(239, 239, 239), new Color4(229, 229, 229));
			public readonly ColorGradient ButtonHover = new ColorGradient(new Color4(235, 244, 252), new Color4(222, 238, 252));
			public readonly ColorGradient ButtonPress = new ColorGradient(new Color4(215, 234, 252), new Color4(199, 226, 252));
			public readonly ColorGradient ButtonDisable = new ColorGradient(new Color4(244, 244, 244), new Color4(244, 244, 244));
			public readonly Vector2 DefaultButtonSize = new Vector2(75, 23);
			public readonly Thickness ControlsPadding = new Thickness(2);
		}

		public static ThemeConstants Constants = new ThemeConstants();

		public DesktopTheme()
		{
			Decorators[typeof(SimpleText)] = DecorateSimpleText;
			Decorators[typeof(Button)] = DecorateButton;
			Decorators[typeof(EditBox)] = DecorateEditBox;
			Decorators[typeof(WindowWidget)] = DecorateWindowWidget;
			Decorators[typeof(TextView)] = DecorateTextView;
			Decorators[typeof(ComboBox)] = DecorateComboBox;
			Decorators[typeof(FileChooserButton)] = DecorateFileChooserButton;
		}

		private void DecorateButton(Widget widget)
		{
			var button = (Button)widget;
			button.Nodes.Clear();
			button.MinMaxSize = Constants.DefaultButtonSize;
			button.Size = button.MinSize;
			button.Padding = Constants.ControlsPadding;
			button.Presenter = new ButtonPresenter(button);
			button.DefaultAnimation.AnimationEngine = new ButtonAnimationEngine(button);
			var caption = new SimpleText {
				Id = "TextPresenter",
				TextColor = Constants.TextColor,
				FontHeight = Constants.TextHeight,
				HAlignment = HAlignment.Center,
				VAlignment = VAlignment.Center,
				OverflowMode = TextOverflowMode.Ellipsis
			};
			button.AddNode(caption);
			ExpandToContainer(caption);
		}

		private void DecorateFileChooserButton(Widget widget)
		{
			var fc = (FileChooserButton)widget;
			fc.Layout = new HBoxLayout();
			var label = new SimpleText {
				Id = "Label",
				AutoSizeConstraints = false,
				MinMaxHeight = Constants.DefaultButtonSize.Y,
				Padding = Constants.ControlsPadding,
				LayoutCell = new LayoutCell { StretchX = float.MaxValue }
			};
			var button = new Button {
				Id = "Button",
				Text = "...",
				MinMaxWidth = 20
			};
			fc.Presenter = new BorderedFramePresenter(fc, Constants.DialogBackground, Constants.ButtonInactiveBorder);
			fc.AddNode(label);
			fc.AddNode(button);
		}
		
		private void DecorateSimpleText(Widget widget)
		{
			var text = (SimpleText)widget;
			text.AutoSizeConstraints = true;
			text.Localizable = true;
			text.TextColor = Color4.White;
			text.Color = Constants.TextColor;
			text.Font = new SerializableFont();
			text.FontHeight = Constants.TextHeight;
			text.HAlignment = HAlignment.Left;
			text.VAlignment = VAlignment.Center;
			text.OverflowMode = TextOverflowMode.Ellipsis;
			text.TrimWhitespaces = true;	
			text.Size = text.MinSize;
		}
				
		private void DecorateWindowWidget(Widget widget)
		{
			widget.Presenter = new WindowWidgetPresenter(widget);
		}
		
		private void DecorateEditBox(Widget widget)
		{
			DecorateSimpleText(widget);
			var eb = (EditBox)widget;
			eb.AutoSizeConstraints = false;
			eb.Caret.IsVisible = true;
			eb.MinSize = Constants.DefaultButtonSize;
			eb.MaxHeight = eb.MinHeight;
			eb.HAlignment = HAlignment.Left;
			eb.Localizable = false;	
			eb.TrimWhitespaces = false;
			eb.VAlignment = VAlignment.Center;
			eb.Padding = Constants.ControlsPadding;
			var editorParams = new EditorParams { MaxLength = 100, MaxLines = 1 };
			new CaretDisplay(
				eb, eb.Caret,
				new CaretParams { CaretWidget = new VerticalLineCaret(eb, thickness: 1.0f) });
			new Editor(eb, eb.Caret, editorParams);
			eb.Presenter = new BorderedFramePresenter(eb, Constants.TextViewBackground, Constants.ButtonInactiveBorder);
		}

		private void DecorateComboBox(Widget widget)
		{
			var comboBox = (ComboBox)widget;
			comboBox.MinSize = Constants.DefaultButtonSize;
			comboBox.MaxHeight = Constants.DefaultButtonSize.Y;
			var text = new SimpleText {
				Id = "Label",
				VAlignment = VAlignment.Center,
			};
			text.Presenter = new ComboBoxPresenter(text);
			comboBox.AddNode(text);
			ExpandToContainer(text);
		}

		private void DecorateTextView(Widget widget)
		{
			var tv = (TextView)widget;
			tv.Presenter = new BorderedFramePresenter(tv, Constants.TextViewBackground, Constants.ButtonInactiveBorder);
		}

		class BorderedFramePresenter : IPresenter
		{
			private IPresenter oldPresenter;
			private Widget widget;
			private Color4 innerColor;
			private Color4 borderColor;

			public BorderedFramePresenter(Widget widget, Color4 innerColor, Color4 borderColor)
			{
				this.widget = widget;
				this.innerColor = innerColor;
				this.borderColor = borderColor;
				oldPresenter = widget.Presenter;
			}

			public void Render()
			{
				widget.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, widget.Size, innerColor);
				Renderer.DrawRectOutline(Vector2.Zero, widget.Size, borderColor);
				if (oldPresenter != null) {
					oldPresenter.Render();
				}
			}

			public IPresenter Clone(Node newNode)
			{
				return new BorderedFramePresenter((Widget)newNode, innerColor, borderColor);
			}
		}

		class ComboBoxPresenter : IPresenter
		{
			private const float IconWidth = 20;
			private IPresenter oldPresenter;
			private Widget widget;
			private VectorShape icon = new VectorShape {
				new VectorShape.Line(0, 0.1f, 0, 0.9f, Constants.ButtonInactiveBorder, 0.05f),
				new VectorShape.Line(0.5f, 0.8f, 0.7f, 0.65f, Constants.TextColor, 0.07f),
				new VectorShape.Line(0.5f, 0.8f, 0.3f, 0.65f, Constants.TextColor, 0.07f),
				new VectorShape.Line(0.5f, 0.2f, 0.7f, 0.35f, Constants.TextColor, 0.07f),
				new VectorShape.Line(0.5f, 0.2f, 0.3f, 0.35f, Constants.TextColor, 0.07f),
			};

			public ComboBoxPresenter(Widget widget)
			{
				this.widget = widget;
				oldPresenter = widget.Presenter;
				widget.Padding = Constants.ControlsPadding;
				widget.Padding.Right = IconWidth;
			}

			public void Render()
			{
				widget.PrepareRendererState();
				Renderer.DrawVerticalGradientRect(Vector2.Zero, widget.Size, Constants.ButtonDefault);
				Renderer.DrawRectOutline(Vector2.Zero, widget.Size, Constants.ButtonInactiveBorder);
				if (oldPresenter != null) {
					oldPresenter.Render();
				}
				var transform = Matrix32.Scaling(IconWidth, widget.Height) * Matrix32.Translation(widget.Width - IconWidth, 0);
				icon.Draw(transform);
			}

			public IPresenter Clone(Node newNode)
			{
				return new ComboBoxPresenter((Widget)newNode);
			}
		}

		class ButtonAnimationEngine : Lime.AnimationEngine
		{
			private Button button;

			public ButtonAnimationEngine(Button button)
			{
				this.button = button;
			}

			public override bool TryRunAnimation(Animation animation, string markerId)
			{
				(button.Presenter as ButtonPresenter).SetState(markerId);
				return true;
			}
		}

		class ButtonPresenter : IPresenter
		{
			private IPresenter oldPresenter;
			private Widget widget;
			private ColorGradient innerGradient;

			public ButtonPresenter(Widget widget)
			{
				this.widget = widget;
				oldPresenter = widget.Presenter;
			}

			public void SetState(string state)
			{
				Window.Current.Invalidate();
				switch (state) {
					case "Press":
						innerGradient = Constants.ButtonPress;
						break;
					case "Focus":
						innerGradient = Constants.ButtonHover;
						break;
					case "Disable":
						innerGradient = Constants.ButtonDisable;
						break;
					default:
						innerGradient = Constants.ButtonDefault;
						break;
				}
			}

			public void Render()
			{
				widget.PrepareRendererState();
				Renderer.DrawVerticalGradientRect(Vector2.Zero, widget.Size, innerGradient);
				Renderer.DrawRectOutline(Vector2.Zero, widget.Size, Constants.ButtonInactiveBorder);
				if (oldPresenter != null) {
					oldPresenter.Render();
				}
			}

			public IPresenter Clone(Node newNode)
			{
				return new ButtonPresenter((Widget)newNode);
			}
		}

		class WindowWidgetPresenter : IPresenter
		{
			private IPresenter oldPresenter;
			private Widget widget;

			public WindowWidgetPresenter(Widget widget)
			{
				this.widget = widget;
				oldPresenter = widget.Presenter;
			}

			public void Render()
			{
				widget.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, widget.Size, Constants.DialogBackground);
				if (oldPresenter != null) {
					oldPresenter.Render();
				}
			}

			public IPresenter Clone(Node newNode)
			{
				return new WindowWidgetPresenter((Widget)newNode);
			}
		}

		private static void ExpandToContainer(Widget widget)
		{
			widget.Anchors = Anchors.None;
			widget.Size = widget.ParentWidget.Size;
			widget.Anchors = Anchors.LeftRightTopBottom;
		}
	}
}