#if !ANDROID && !iOS
using System;
using System.Collections.Generic;
using System.Reflection;
using Yuzu;

namespace Lime
{
	public static class Theme
	{
		public static class Metrics
		{
			public static readonly int TextHeight = 16;
			public static readonly Vector2 CheckBoxSize = new Vector2(16, 16);
			public static readonly Vector2 DefaultButtonSize = new Vector2(75, 22);
			public static readonly Vector2 DefaultToolbarButtonSize = new Vector2(20, 20);
			public static readonly Vector2 DefaultEditBoxSize = new Vector2(75, 22);
			public static readonly Vector2 MaxTabSize = new Vector2(250, 24);
			public static readonly Vector2 MinTabSize = new Vector2(25, 24);
			public static readonly Vector2 CloseButtonSize = new Vector2(16, 16);
			public static readonly Thickness ControlsPadding = new Thickness(2);
		}

		// to set all colors to the value of default one, making them more noticeable
		// in case their value is unset (it'll be (0 0 0 0) otherwise
		public class DefaultColors
		{
			public static Color4 DefaultColor { get; set; } = new Color4(255, 0, 255, 255);
			public DefaultColors()
			{
				foreach (var p in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
					if (p.PropertyType == typeof(Color4)) {
						p.SetValue(this, DefaultColor);
					}
				}
			}
		}

		[YuzuDontGenerateDeserializer]
		public class ColorTheme : DefaultColors
		{
			[YuzuOptional]
			public Color4 BlackText { get; set; }
			[YuzuOptional]
			public Color4 GrayText { get; set; }
			[YuzuOptional]
			public Color4 WhiteBackground { get; set; }
			[YuzuOptional]
			public Color4 GrayBackground { get; set; }
			[YuzuOptional]
			public Color4 SelectedBackground { get; set; }
			[YuzuOptional]
			public Color4 HoveredBackground { get; set; }
			[YuzuOptional]
			public Color4 ControlBorder { get; set; }
			[YuzuOptional]
			public ColorGradient ButtonDefault { get; set; }
			[YuzuOptional]
			public ColorGradient ButtonHover { get; set; }
			[YuzuOptional]
			public ColorGradient ButtonPress { get; set; }
			[YuzuOptional]
			public ColorGradient ButtonDisable { get; set; }
			[YuzuOptional]
			public ColorGradient ExpandButtonDefault { get; set; }
			[YuzuOptional]
			public ColorGradient ExpandButtonHover { get; set; }
			[YuzuOptional]
			public ColorGradient ExpandButtonPress { get; set; }
			[YuzuOptional]
			public ColorGradient ExpandButtonDisable { get; set; }
			[YuzuOptional]
			public ColorGradient PathBarButtonNormal { get; set; }
			[YuzuOptional]
			public ColorGradient PathBarButtonHover { get; set; }
			[YuzuOptional]
			public ColorGradient PathBarButtonPress { get; set; }
			[YuzuOptional]
			public Color4 PathBarButtonOutlineNormal { get; set; }
			[YuzuOptional]
			public Color4 PathBarButtonOutlineHover { get; set; }
			[YuzuOptional]
			public Color4 PathBarButtonOutlinePress { get; set; }
			[YuzuOptional]
			public Color4 DirectoryPickerBackground { get; set; }
			[YuzuOptional]
			public Color4 DirectoryPickerOutline { get; set; }
			[YuzuOptional]
			public Color4 DirectoryPickerItemHoveredBackground { get; set; }
			[YuzuOptional]
			public Color4 TabNormal { get; set; }
			[YuzuOptional]
			public Color4 TabActive { get; set; }
			[YuzuOptional]
			public Color4 SeparatorColor { get; set; }
			[YuzuOptional]
			public Color4 SeparatorHighlightColor { get; set; }
			[YuzuOptional]
			public Color4 SeparatorDragColor { get; set; }
			[YuzuOptional]
			public Color4 KeyboardFocusBorder { get; set; }
			[YuzuOptional]
			public Color4 TextSelection { get; set; }
			[YuzuOptional]
			public Color4 CloseButtonNormal { get; set; }
			[YuzuOptional]
			public Color4 CloseButtonHovered { get; set; }
			[YuzuOptional]
			public Color4 CloseButtonPressed { get; set; }
			[YuzuOptional]
			public Color4 CloseButtonFocusBorderNormal { get; set; }
			[YuzuOptional]
			public Color4 CloseButtonFocusBorderHovered { get; set; }
			[YuzuOptional]
			public Color4 ScrollbarBackground { get; set; }
			[YuzuOptional]
			public Color4 ScrollbarThumb { get; set; }
			[YuzuOptional]
			public Color4 TextCaret { get; set; }
			[YuzuOptional]
			public Color4 ZebraColor1 { get; set; }
			[YuzuOptional]
			public Color4 ZebraColor2 { get; set; }
			[YuzuOptional]
			public Color4 SelectedBorder { get; set; }
			[YuzuOptional]
			public Color4 RedText { get; set; }
			[YuzuOptional]
			public Color4 WarningBackground { get; set; }

			public static ColorTheme CreateDarkTheme()
			{
				var grayBackground = new Color4(45, 45, 48);
				var blackText = new Color4(204, 204, 204);
				return new ColorTheme {
					BlackText = blackText,
					RedText = new Color4(255, 100, 100),
					GrayText = blackText.Darken(0.35f),
					WhiteBackground = Color4.Black.Lighten(0.25f),
					GrayBackground = new Color4(45, 45, 48),
					SelectedBackground = new Color4(86, 86, 86),
					SelectedBorder = new Color4(153, 209, 255),
					HoveredBackground = new Color4(86, 86, 86).Lighten(0.25f),
					ControlBorder = new Color4(39, 39, 39),
					ButtonDefault = new ColorGradient(new Color4(107, 107, 107), new Color4(107, 107, 107)),
					ButtonHover = new ColorGradient(new Color4(133, 133, 133), new Color4(133, 133, 133)),
					ButtonPress = new ColorGradient(new Color4(141, 167, 204), new Color4(141, 167, 204)),
					ButtonDisable = new ColorGradient(new Color4(64, 64, 64), new Color4(64, 64, 64)),
					ExpandButtonDefault = new ColorGradient(new Color4(107, 107, 107), new Color4(107, 107, 107)),
					ExpandButtonHover = new ColorGradient(new Color4(133, 133, 133), new Color4(133, 133, 133)),
					ExpandButtonPress = new ColorGradient(new Color4(141, 167, 204), new Color4(141, 167, 204)),
					ExpandButtonDisable = new ColorGradient(new Color4(64, 64, 64), new Color4(64, 64, 64)),
					PathBarButtonNormal = new ColorGradient(new Color4(63, 63, 63)),
					PathBarButtonHover = new ColorGradient(new Color4(89, 89, 89)),
					PathBarButtonPress = new ColorGradient(new Color4(107, 107, 107)),
					PathBarButtonOutlineNormal = Color4.Transparent,
					PathBarButtonOutlineHover = new Color4(107, 107, 109),
					PathBarButtonOutlinePress = new Color4(143, 179, 215),
					DirectoryPickerBackground = new Color4(82, 82, 82),
					DirectoryPickerOutline = new Color4(39, 39, 39),
					DirectoryPickerItemHoveredBackground = new Color4(127, 127, 127),
					TabNormal = grayBackground.Lighten(0.2f),
					TabActive = grayBackground.Lighten(0.05f),
					SeparatorColor = grayBackground.Lighten(0.3f),
					SeparatorHighlightColor = grayBackground.Lighten(0.5f),
					SeparatorDragColor = grayBackground.Lighten(0.7f),
					KeyboardFocusBorder = new Color4(100, 150, 200),
					TextSelection = new Color4(133, 133, 133),
					CloseButtonNormal = grayBackground.Lighten(0.6f),
					CloseButtonHovered = grayBackground.Lighten(0.8f),
					CloseButtonPressed = grayBackground.Lighten(1),
					CloseButtonFocusBorderNormal = Color4.Transparent,
					CloseButtonFocusBorderHovered = Color4.Red.Lighten(0.6f),
					ScrollbarBackground = new Color4(51, 51, 51),
					ScrollbarThumb = new Color4(107, 107, 107),
					TextCaret = new Color4(204, 204, 204),
					ZebraColor1 = grayBackground.Lighten(0.2f),
					ZebraColor2 = grayBackground.Lighten(0.3f),
					WarningBackground = new Color4(255, 194, 26),
				};

			}

			public static ColorTheme CreateLightTheme()
			{
				var grayBackground = new Color4(240, 240, 240);
				return new ColorTheme {
					BlackText = Color4.Black,
					RedText = Color4.Red,
					GrayText = Color4.Black.Lighten(0.35f),
					WhiteBackground = Color4.White,
					GrayBackground = grayBackground,
					SelectedBackground = new Color4(205, 232, 255),
					SelectedBorder = new Color4(153, 209, 255),
					HoveredBackground = new Color4(229, 243, 255),
					ControlBorder = new Color4(172, 172, 172).Lighten(0.3f),
					ButtonDefault = new ColorGradient(new Color4(239, 239, 239), new Color4(229, 229, 229)),
					ButtonHover = new ColorGradient(new Color4(235, 244, 252), new Color4(222, 238, 252)),
					ButtonPress = new ColorGradient(new Color4(215, 234, 252), new Color4(199, 226, 252)),
					ButtonDisable = new ColorGradient(new Color4(244, 244, 244), new Color4(244, 244, 244)),
					ExpandButtonDefault = new ColorGradient(new Color4(107, 107, 107), new Color4(107, 107, 107)),
					ExpandButtonHover = new ColorGradient(new Color4(133, 133, 133), new Color4(133, 133, 133)),
					ExpandButtonPress = new ColorGradient(new Color4(141, 167, 204), new Color4(141, 167, 204)),
					ExpandButtonDisable = new ColorGradient(new Color4(64, 64, 64), new Color4(64, 64, 64)),
					PathBarButtonNormal = new ColorGradient(new Color4(255, 255, 255)),
					PathBarButtonHover = new ColorGradient(new Color4(219, 236, 249)),
					PathBarButtonPress = new ColorGradient(new Color4(159, 200, 233)),
					PathBarButtonOutlineNormal = Color4.Transparent,
					PathBarButtonOutlineHover = new Color4(115, 193, 255),
					PathBarButtonOutlinePress = new Color4(0, 86, 155),
					DirectoryPickerBackground = new Color4(240, 240, 240),
					DirectoryPickerOutline = new Color4(133, 133, 133),
					DirectoryPickerItemHoveredBackground = new Color4(158, 200, 233),
					TabNormal = grayBackground.Darken(0.2f),
					TabActive = grayBackground.Darken(0.05f),
					SeparatorColor = grayBackground.Darken(0.4f),
					SeparatorHighlightColor = grayBackground.Darken(0.6f),
					SeparatorDragColor = grayBackground.Darken(0.7f),
					KeyboardFocusBorder = new Color4(0, 120, 215),
					TextSelection = new Color4(200, 230, 255),
					CloseButtonNormal = grayBackground.Darken(0.6f),
					CloseButtonHovered = grayBackground.Darken(0.8f),
					CloseButtonPressed = grayBackground.Darken(1),
					CloseButtonFocusBorderNormal = Color4.Transparent,
					CloseButtonFocusBorderHovered = Color4.Red.Lighten(0.6f),
					ScrollbarBackground = new Color4(210, 210, 210),
					ScrollbarThumb = new Color4(120, 120, 120),
					TextCaret = Color4.Black,
					ZebraColor1 = Color4.White,
					ZebraColor2 = Color4.White.Darken(0.1f),
					WarningBackground = new Color4(255, 194, 26),
				};
			}

			public ColorTheme Clone()
			{
				return (ColorTheme)this.MemberwiseClone();
			}
		}

		public static ColorTheme Colors = ColorTheme.CreateLightTheme();

		public class KeyboardFocusBorderPresenter : IPresenter
		{
			private float thickness = 1.0f;

			public KeyboardFocusBorderPresenter(float thikness = 1.0f)
			{
				this.thickness = thikness;
			}

			public IPresenter Clone() => (IPresenter)MemberwiseClone();

			public Lime.RenderObject GetRenderObject(Node node)
			{
				var widget = (Widget)node;
				if (Widget.Focused == node && !widget.IsMouseOverThisOrDescendant()) {
					var ro = RenderObjectPool<RenderObject>.Acquire();
					ro.CaptureRenderState(widget);
					ro.Size = widget.Size;
					ro.Thickness = thickness;
					ro.Color = Colors.KeyboardFocusBorder;
					return ro;
				}
				return null;
			}

			public bool PartialHitTest(Node node, ref HitTestArgs args) => false;

			private class RenderObject : WidgetRenderObject
			{
				public Vector2 Size;
				public float Thickness;
				public Color4 Color;

				public override void Render()
				{
					PrepareRenderState();
					Renderer.DrawRectOutline(Vector2.Zero, Size, Color, Thickness);
				}
			}
		}

		internal class MouseHoverBorderPresenter : IPresenter
		{
			public IPresenter Clone() => (IPresenter)MemberwiseClone();

			public Lime.RenderObject GetRenderObject(Node node)
			{
				var widget = (Widget)node;
				if (widget.IsMouseOverThisOrDescendant()) {
					var ro = RenderObjectPool<RenderObject>.Acquire();
					ro.CaptureRenderState(widget);
					ro.Size = widget.Size;
					ro.Color = Colors.KeyboardFocusBorder;
					return ro;
				}
				return null;
			}

			public bool PartialHitTest(Node node, ref HitTestArgs args) => false;

			private class RenderObject : WidgetRenderObject
			{
				public Vector2 Size;
				public Color4 Color;

				public override void Render()
				{
					PrepareRenderState();
					Renderer.DrawRectOutline(Vector2.Zero, Size, Color, 1);
				}
			}
		}

		public static IEnumerator<object> MouseHoverInvalidationTask(Widget widget)
		{
			var isHovered = widget.IsMouseOverThisOrDescendant();
			while (true) {
				var isMouseOver = widget.IsMouseOverThisOrDescendant();
				if (isMouseOver != isHovered) {
					isHovered = isMouseOver;
					Window.Current.Invalidate();
				}
				yield return null;
			}
		}
	}
}
#endif
