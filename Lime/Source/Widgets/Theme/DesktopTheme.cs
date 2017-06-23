#if !ANDROID && !iOS && !UNITY
using System;
using System.Collections.Generic;

namespace Lime
{
	public class DesktopTheme : Theme
	{
		public static class Metrics
		{
			public static readonly int TextHeight = 16;
			public static readonly Vector2 CheckBoxSize = new Vector2(16, 16);
			public static readonly Vector2 DefaultButtonSize = new Vector2(75, 22);
			public static readonly Vector2 DefaultEditBoxSize = new Vector2(75, 22);
			public static readonly Vector2 MaxTabSize = new Vector2(250, 24);
			public static readonly Vector2 MinTabSize = new Vector2(25, 24);
			public static readonly Vector2 CloseButtonSize = new Vector2(16, 16);
			public static readonly Thickness ControlsPadding = new Thickness(2);
		}

		public class ColorTheme
		{
			public Color4 BlackText;
			public Color4 GrayText;
			public Color4 WhiteBackground;
			public Color4 GrayBackground;
			public Color4 SelectedBackground;
			public Color4 ControlBorder;
			public ColorGradient ButtonDefault;
			public ColorGradient ButtonHover;
			public ColorGradient ButtonPress;
			public ColorGradient ButtonDisable;
			public Color4 TabNormal;
			public Color4 TabActive;
			public Color4 SeparatorColor;
			public Color4 KeyboardFocusBorder;
			public Color4 TextSelection;
			public Color4 CloseButtonNormal;
			public Color4 CloseButtonHovered;
			public Color4 CloseButtonPressed;
			public Color4 ScrollbarBackground;
			public Color4 ScrollbarThumb;
			public Color4 TextCaret;
		
			public static ColorTheme CreateDarkTheme()
			{
				var grayBackground = new Color4(45, 45, 48);
				var blackText = new Color4(204, 204, 204);
				return new ColorTheme {
					BlackText = blackText,
					GrayText = blackText.Darken(0.35f),
					WhiteBackground = Color4.Black.Lighten(0.25f),
					GrayBackground = new Color4(45, 45, 48),
					SelectedBackground = new Color4(86, 86, 86),
					ControlBorder = new Color4(39, 39, 39),
					ButtonDefault = new ColorGradient(new Color4(107, 107, 107), new Color4(107, 107, 107)),
					ButtonHover = new ColorGradient(new Color4(133, 133, 133), new Color4(133, 133, 133)),
					ButtonPress = new ColorGradient(new Color4(141, 167, 204), new Color4(141, 167, 204)),
					ButtonDisable = new ColorGradient(new Color4(64, 64, 64), new Color4(64, 64, 64)),
					TabNormal = grayBackground.Lighten(0.1f),
					TabActive = grayBackground.Lighten(0.05f),
					SeparatorColor = grayBackground.Lighten(0.3f),
					KeyboardFocusBorder = new Color4(100, 150, 200),
					TextSelection = new Color4(133, 133, 133),
					CloseButtonNormal = grayBackground.Lighten(0.6f),
					CloseButtonHovered = grayBackground.Lighten(0.8f),
					CloseButtonPressed = grayBackground.Lighten(1),
					ScrollbarBackground = new Color4(51, 51, 51),
					ScrollbarThumb = new Color4(107, 107, 107),
					TextCaret = new Color4(204, 204, 204)
				};
			}

			public static ColorTheme CreateLightTheme()
			{
				var grayBackground = new Color4(240, 240, 240);
				return new ColorTheme {
					BlackText = Color4.Black,
					GrayText = Color4.Black.Lighten(0.35f),
					WhiteBackground = Color4.White,
					GrayBackground = grayBackground,
					SelectedBackground = new Color4(140, 170, 255),
					ControlBorder = new Color4(172, 172, 172).Lighten(0.3f),
					ButtonDefault = new ColorGradient(new Color4(239, 239, 239), new Color4(229, 229, 229)),
					ButtonHover = new ColorGradient(new Color4(235, 244, 252), new Color4(222, 238, 252)),
					ButtonPress = new ColorGradient(new Color4(215, 234, 252), new Color4(199, 226, 252)),
					ButtonDisable = new ColorGradient(new Color4(244, 244, 244), new Color4(244, 244, 244)),
					TabNormal = grayBackground.Darken(0.1f),
					TabActive = grayBackground.Darken(0.05f),
					SeparatorColor = grayBackground.Darken(0.3f),
					KeyboardFocusBorder = new Color4(150, 200, 255),
					TextSelection = new Color4(200, 230, 255),
					CloseButtonNormal = grayBackground.Darken(0.6f),
					CloseButtonHovered = grayBackground.Darken(0.8f),
					CloseButtonPressed = grayBackground.Darken(1),
					ScrollbarBackground = new Color4(210, 210, 210),
					ScrollbarThumb = new Color4(120, 120, 120),
					TextCaret = Color4.Black
				};
			}
		}

		public static ColorTheme Colors = ColorTheme.CreateLightTheme();

		public DesktopTheme()
		{
			Decorators[typeof(SimpleText)] = DecorateSimpleText;
			Decorators[typeof(Button)] = DecorateButton;
			Decorators[typeof(TabCloseButton)] = DecorateTabCloseButton;
			Decorators[typeof(EditBox)] = DecorateEditBox;
			Decorators[typeof(NumericEditBox)] = DecorateEditBoxWithSpinner;
			Decorators[typeof(CheckBox)] = DecorateCheckBox;
			Decorators[typeof(WindowWidget)] = DecorateWindowWidget;
			Decorators[typeof(TextView)] = DecorateTextView;
			Decorators[typeof(DropDownList)] = DecorateDropDownList;
			Decorators[typeof(ComboBox)] = DecorateComboBox;
			Decorators[typeof(FileChooserButton)] = DecorateFileChooserButton;
			Decorators[typeof(HSplitter)] = DecorateSplitter;
			Decorators[typeof(VSplitter)] = DecorateSplitter;
			Decorators[typeof(Tab)] = DecorateTab;
			Decorators[typeof(TabBar)] = DecorateTabBar;
			Decorators[typeof(BorderedFrame)] = DecorateBorderedFrame;
			Decorators[typeof(Slider)] = DecorateSlider;
			Decorators[typeof(ScrollViewWidget)] = DecorateScrollViewWidget;
		}

		private void DecorateSplitter(Widget widget)
		{
			var splitter = (Splitter)widget;
			splitter.SeparatorColor = Colors.SeparatorColor;
			splitter.SeparatorWidth = 1;
			splitter.SeparatorActiveAreaWidth = 4;
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
			button.DefaultAnimation.AnimationEngine = new AnimationEngineDelegate {
				OnRunAnimation = (animation, markerId) => {
					presenter.SetState(markerId);
					return true;
				}
			};
			var caption = new SimpleText {
				Id = "TextPresenter",
				TextColor = Colors.BlackText,
				FontHeight = Metrics.TextHeight,
				HAlignment = HAlignment.Center,
				VAlignment = VAlignment.Center,
				OverflowMode = TextOverflowMode.Ellipsis
			};
			button.AddNode(caption);
			button.TabTravesable = new TabTraversable();
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
			text.VAlignment = VAlignment.Top;
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
			var eb = (CommonEditBox)widget;
			var tw = eb.TextWidget;
			DecorateSimpleText(tw);
			tw.AutoSizeConstraints = false;
			eb.MinSize = Metrics.DefaultEditBoxSize;
			eb.MaxHeight = eb.MinHeight;
			tw.Localizable = false;
			tw.TrimWhitespaces = false;
			tw.OverflowMode = TextOverflowMode.Ignore;
			tw.Padding = Metrics.ControlsPadding;

			var editorParams = new EditorParams {
				MaxLines = 1, Scroll = eb.Scroll,
				OffsetContextMenu = p => p + new Vector2(1f, tw.FontHeight + 1f),
				SelectAllOnFocus = true
			};
			eb.Editor = new Editor(tw, editorParams, eb);
			var vc = new VerticalLineCaret { Color = Colors.TextCaret };
			eb.Updated += delta =>
				vc.Width = eb.Editor.OverwriteMode && !eb.Editor.HasSelection() ?
					tw.Font.Chars.Get(eb.Editor.CurrentChar(), tw.FontHeight)?.Width ?? 5f : 0f;
			new CaretDisplay(
				tw, eb.Editor.CaretPos, new CaretParams { CaretPresenter = vc });
			new SelectionPresenter(
				tw, eb.Editor.SelectionStart, eb.Editor.SelectionEnd,
				new SelectionParams {
					Padding = Thickness.Zero,
					OutlineColor = Colors.TextSelection,
					Color = Colors.TextSelection
				});

			eb.TabTravesable = new TabTraversable();
			eb.CompoundPresenter.Add(new BorderedFramePresenter(Colors.WhiteBackground, Colors.ControlBorder));
			eb.CompoundPostPresenter.Add(new KeyboardFocusBorderPresenter());
		}

		private void DecorateEditBoxWithSpinner(Widget widget)
		{
			DecorateEditBox(widget);
			var eb = (NumericEditBox)widget;
			eb.TextWidget.Padding = new Thickness(SpinButtonPresenter.ButtonWidth + 2, 2);
			eb.CompoundPostPresenter.Add(new SpinButtonPresenter(true));
			eb.CompoundPostPresenter.Add(new SpinButtonPresenter(false));
			eb.Awoken += n => {
				var e = (NumericEditBox)n;
				e.Tasks.Add(HandleSpinButtonTask(e, true));
				e.Tasks.Add(HandleSpinButtonTask(e, false));
			};
		}

		private IEnumerator<object> HandleSpinButtonTask(NumericEditBox eb, bool leftToRight)
		{
			while (true) {
				if (eb.Input.WasMousePressed()) {
					if (
						leftToRight && eb.Input.LocalMousePosition.X > eb.Width - SpinButtonPresenter.ButtonWidth ||
						!leftToRight && eb.Input.LocalMousePosition.X < SpinButtonPresenter.ButtonWidth
					) {
						eb.RaiseBeginSpin();
						eb.Input.CaptureMouse();
						eb.Input.ConsumeKey(Key.Mouse0);
						var prevMousePos = Application.DesktopMousePosition;
						var dragged = false;
						var disp = Window.Current.Display;
						// TODO: Remove focus revoke and block editor input while dragging.
						eb.SetFocus();
						eb.RevokeFocus();
						while (eb.Input.IsMousePressed()) {
							dragged |= Application.DesktopMousePosition != prevMousePos;
							var wrapped = false;
							if (Application.DesktopMousePosition.X > disp.Position.X + disp.Size.X - 5) {
								prevMousePos.X = disp.Position.X + 5;
								wrapped = true;
							}
							if (Application.DesktopMousePosition.X < disp.Position.X + 5) {
								prevMousePos.X = disp.Position.X + disp.Size.X - 5;
								wrapped = true;
							}
							if (wrapped) {
								Application.DesktopMousePosition = new Vector2(prevMousePos.X, Application.DesktopMousePosition.Y);
							}
							var delta = (Application.DesktopMousePosition.X - prevMousePos.X).Round() * eb.Step;
							if (eb.Input.IsKeyPressed(Key.Shift)) {
								delta *= 10f;
							} else if (eb.Input.IsKeyPressed(Key.Control)) {
								delta *= 0.1f;
							}
							eb.Value += delta;
							prevMousePos = Application.DesktopMousePosition;
							yield return null;
						}
						if (!dragged) {
							var delta = (leftToRight ? 1 : -1) * eb.Step;
							if (eb.Input.IsKeyPressed(Key.Shift)) {
								delta *= 10f;
							} else if (eb.Input.IsKeyPressed(Key.Control)) {
								delta *= 0.1f;
							}
							eb.Value += delta;
						}
						eb.Input.ConsumeKey(Key.Mouse0);
						eb.Input.ReleaseMouse();
						eb.RaiseEndSpin();
					}
				}
				yield return null;
			}
		}

		class SpinButtonPresenter : CustomPresenter
		{
			public const float ButtonWidth = 10;

			static Color4 color = Color4.Lerp(0.25f, Colors.ControlBorder, Colors.BlackText);

			private readonly VectorShape buttonShape = new VectorShape {
				new VectorShape.TriangleFan(new float[] { 0, 0, 1, 0, 1, 1, 0, 1 }, Colors.WhiteBackground),
				new VectorShape.TriangleFan(new float[] { 0.3f, 0.3f, 0.7f, 0.5f, 0.3f, 0.7f }, color)
			};

			private bool leftToRight;

			public SpinButtonPresenter(bool leftToRight)
			{
				this.leftToRight = leftToRight;
			}

			public override bool PartialHitTest(Node node, ref HitTestArgs args)
			{
				return base.PartialHitTest(node, ref args);
			}

			public override void Render(Node node)
			{
				var widget = node.AsWidget;
				widget.PrepareRendererState();
				Matrix32 transform;
				if (leftToRight) {
					transform = Matrix32.Scaling(ButtonWidth, widget.Height - 2) * Matrix32.Translation(widget.Width - ButtonWidth - 1, 1);
				} else {
					transform = Matrix32.Scaling(-ButtonWidth, widget.Height - 2) * Matrix32.Translation(ButtonWidth + 1, 1);
				}
				buttonShape.Draw(transform);
			}
		}

		private void DecorateCheckBox(Widget widget)
		{
			var cb = (CheckBox)widget;
			cb.Layout = new StackLayout();
			cb.AddNode(new Button {
				Id = "Button",
				Presenter = new CheckBoxPresenter(cb),
				LayoutCell = new LayoutCell(Alignment.Center),
				MinMaxSize = Metrics.CheckBoxSize,
				TabTravesable = null
			});
			cb.TabTravesable = new TabTraversable();
			cb.CompoundPostPresenter.Add(new KeyboardFocusBorderPresenter());
		}

		private void DecorateDropDownList(Widget widget)
		{
			var dropDownList = (DropDownList)widget;
			dropDownList.MinSize = Metrics.DefaultButtonSize;
			dropDownList.MaxHeight = Metrics.DefaultButtonSize.Y;
			dropDownList.TabTravesable = new TabTraversable();
			var text = new SimpleText {
				Id = "TextWidget",
				VAlignment = VAlignment.Center,
			};
			text.CompoundPresenter.Add(new DropDownListPresenter());
			dropDownList.PostPresenter = new KeyboardFocusBorderPresenter();
			text.Padding = Metrics.ControlsPadding;
			dropDownList.AddNode(text);
			ExpandToContainer(text);
		}

		private void DecorateComboBox(Widget widget)
		{
			var comboBox = (ComboBox)widget;
			comboBox.MinSize = Metrics.DefaultButtonSize;
			comboBox.MaxHeight = Metrics.DefaultButtonSize.Y;
			comboBox.CompoundPresenter.Add(new DropDownListPresenter());
			var editBox = new EditBox { Id = "TextWidget" };
			comboBox.AddNode(editBox);
			ExpandToContainer(editBox);
			editBox.Width -= DropDownListPresenter.IconWidth;
		}

		private void DecorateTabBar(Widget widget)
		{
			var tabBar = (TabBar)widget;
			tabBar.Layout = new HBoxLayout();
		}

		private void DecorateTab(Widget widget)
		{
			var tab = (Tab)widget;
			tab.Padding = Metrics.ControlsPadding;
			tab.MinSize = Metrics.MinTabSize;
			tab.MaxSize = Metrics.MaxTabSize;
			tab.Size = tab.MinSize;
			tab.Layout = new HBoxLayout();
			var caption = new SimpleText {
				Id = "TextPresenter",
				AutoSizeConstraints = false,
				FontHeight = Metrics.TextHeight,
				HAlignment = HAlignment.Center,
				VAlignment = VAlignment.Center,
				OverflowMode = TextOverflowMode.Ellipsis,
				LayoutCell = new LayoutCell(Alignment.Center)
			};
			var presenter = new TabPresenter(caption);
			tab.Presenter = presenter;
			tab.DefaultAnimation.AnimationEngine = new AnimationEngineDelegate {
				OnRunAnimation = (animation, markerId) => {
					presenter.SetState(markerId);
					return true;
				}
			};
			var closeButton = new TabCloseButton { Id = "CloseButton" };
			tab.AddNode(caption);
			tab.AddNode(closeButton);
		}

		private void DecorateTabCloseButton(Widget widget)
		{
			var button = (Button)widget;
			var presenter = new TabCloseButtonPresenter();
			button.LayoutCell = new LayoutCell(Alignment.Center, stretchX: 0);
			button.Presenter = presenter;
			button.MinMaxSize = Metrics.CloseButtonSize;
			button.DefaultAnimation.AnimationEngine = new AnimationEngineDelegate {
				OnRunAnimation = (animation, markerId) => {
					presenter.SetState(markerId);
					return true;
				}
			};
		}

		private void DecorateTextView(Widget widget)
		{
			DecorateScrollViewWidget(widget);
			var tw = (TextView)widget;
			tw.Behaviour.Content.Padding = new Thickness(4);
			tw.Behaviour.Content.Layout = new VBoxLayout();
			tw.Behaviour.Frame.CompoundPresenter.Add(new BorderedFramePresenter(DesktopTheme.Colors.WhiteBackground, DesktopTheme.Colors.ControlBorder));
		}

		private void DecorateBorderedFrame(Widget widget)
		{
			var bf = (BorderedFrame)widget;
			bf.CompoundPresenter.Add(new BorderedFramePresenter(Colors.GrayBackground, Colors.ControlBorder));
		}

		private void DecorateSlider(Widget widget)
		{
			var slider = (Slider)widget;
			slider.Options = Slider.SliderOptions.ClickOnRail;
			var rail = new Spline { Id = "Rail" };
			rail.AddNode(new SplinePoint { Position = new Vector2(0, 0.5f) });
			rail.AddNode(new SplinePoint { Position = new Vector2(1, 0.5f) });
			slider.AddNode(rail);
			ExpandToContainer(rail);
			var thumb = new Widget {
				Id = "Thumb",
				Size = new Vector2(8, 16),
				Pivot = Vector2.Half,
			};
			slider.AddNode(thumb);
			slider.MinSize = new Vector2(30, 16);
			thumb.CompoundPresenter.Add(new SliderThumbPresenter());
			slider.CompoundPresenter.Add(new SliderPresenter());
		}

		private void DecorateScrollViewWidget(Widget widget)
		{
			var sv = (ScrollViewWidget)widget;
			var slider = new Widget();
			slider.Size = new Vector2(10, 5);
			slider.CompoundPresenter.Add(new DelegatePresenter<Widget>(_ => {
				sv.PrepareRendererState();
				Renderer.DrawRect(new Vector2(sv.Width - slider.Width, 0), sv.Size, Colors.ScrollbarBackground);
				slider.PrepareRendererState();
				Renderer.DrawRect(new Vector2(2, 0), new Vector2(slider.Width - 2, slider.Height), Colors.ScrollbarThumb);
			}));
			var ae = new AnimationEngineDelegate();
			ae.OnRunAnimation = (_, marker) => {
				slider.Opacity = marker == "Show" ? 1 : 0;
				return true;
			};
			slider.DefaultAnimation.AnimationEngine = ae;
			sv.Behaviour = new ScrollViewWithSlider(sv, slider, ScrollDirection.Vertical) {
				ScrollBySlider = true
			};
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
				if (Widget.Focused == node) {
					var widget = node.AsWidget;
					widget.PrepareRendererState();
					Renderer.DrawRectOutline(Vector2.Zero, widget.Size, Colors.KeyboardFocusBorder, 1);
				}
			}
		}

		class DropDownListPresenter : CustomPresenter
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

		interface IButtonPresenter
		{
			void SetState(string state);
		}

		class ButtonPresenter : CustomPresenter, IButtonPresenter
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

		class SliderThumbPresenter : CustomPresenter
		{
			public override void Render(Node node)
			{
				var widget = node.AsWidget;
				widget.PrepareRendererState();
				var p = new Vector2(0, 2);
				Renderer.DrawVerticalGradientRect(-p, p + widget.Size, Colors.ButtonDefault);
				Renderer.DrawRectOutline(-p, p + widget.Size, Colors.ControlBorder);
			}
		}

		class SliderPresenter : CustomPresenter
		{
			public override void Render(Node node)
			{
				var widget = node.AsWidget;
				widget.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, widget.Size, Colors.WhiteBackground);
				Renderer.DrawRectOutline(Vector2.Zero, widget.Size, Colors.ControlBorder);
			}
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
				label.Color = active ? Colors.BlackText : Colors.GrayText;
			}

			public override void Render(Node node)
			{
				var widget = node.AsWidget;
				widget.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, widget.Size - new Vector2(1, 0), active ? Colors.TabActive : Colors.TabNormal);
			}

			public override bool PartialHitTest(Node node, ref HitTestArgs args)
			{
				return node.PartialHitTest(ref args);
			}
		}

		class CheckBoxPresenter : CustomPresenter
		{
			readonly CheckBox checkBox;

			private VectorShape icon = new VectorShape {
				new VectorShape.Line(0.2f, 0.5f, 0.4f, 0.8f, Colors.BlackText, 0.1f),
				new VectorShape.Line(0.4f, 0.8f, 0.75f, 0.25f, Colors.BlackText, 0.1f),
			};

			public CheckBoxPresenter(CheckBox checkBox)
			{
				this.checkBox = checkBox;
			}

			public override void Render(Node node)
			{
				var widget = node.AsWidget;
				widget.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, widget.Size, Colors.WhiteBackground);
				Renderer.DrawRectOutline(Vector2.Zero, widget.Size, Colors.ControlBorder);
				if (checkBox.Checked) {
					var transform = Matrix32.Scaling(Metrics.CheckBoxSize);
					icon.Draw(transform);
				}
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

		public class TabCloseButton : Button
		{
		}

		class TabCloseButtonPresenter : CustomPresenter, IButtonPresenter
		{
			private VectorShape icon = new VectorShape {
				new VectorShape.Line(0.3f, 0.3f, 0.7f, 0.7f, Color4.White, 0.075f * 1.5f),
				new VectorShape.Line(0.3f, 0.7f, 0.7f, 0.3f, Color4.White, 0.0751f * 1.5f),
			};

			Color4 color;

			public override void Render(Node node)
			{
				var widget = node.AsWidget;
				widget.PrepareRendererState();
				var transform = Matrix32.Scaling(Metrics.CloseButtonSize);
				icon.Draw(transform, color);
			}

			public override bool PartialHitTest(Node node, ref HitTestArgs args)
			{
				return node.PartialHitTest(ref args);
			}

			public void SetState(string state)
			{
				CommonWindow.Current.Invalidate();
				if (state == "Normal") {
					color = Colors.CloseButtonNormal;
				} else if (state == "Focus") {
					color = Colors.CloseButtonHovered;
				} else {
					color = Colors.CloseButtonPressed;
				}
			}
		}
	}
}
#endif
