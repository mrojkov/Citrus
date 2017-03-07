using System;
using Lime;

namespace Tangerine.UI
{
	/* Light theme
	public static class Colors
	{
		public static readonly Color4 WhiteBackground = DesktopTheme.Colors.WhiteBackground;
		public static readonly Color4 GrayBackground = DesktopTheme.Colors.GrayBackground;
		public static readonly Color4 SelectedBackground = DesktopTheme.Colors.SelectedBackground;
		public static readonly Color4 DragCursor = Color4.Black;
	}

	public static class ToolbarColors
	{
		public static readonly Color4 ButtonHighlightBorder = DesktopTheme.Colors.KeyboardFocusBorder.Darken(0.2f);
		public static readonly Color4 ButtonHighlightBackground = DesktopTheme.Colors.KeyboardFocusBorder.Lighten(0.3f);
		public static readonly Color4 ButtonPressBorder = ButtonHighlightBorder;
		public static readonly Color4 ButtonPressBackground = ButtonHighlightBackground.Darken(0.1f);
		public static readonly Color4 ButtonCheckedBorder = ButtonPressBorder.Darken(0.1f);
		public static readonly Color4 ButtonCheckedBackground = ButtonPressBackground.Transparentify(0.5f);
		public static readonly Color4 ButtonDisabledColor = Color4.Gray.Darken(0.1f);
		public static readonly Color4 Background = DesktopTheme.Colors.GrayBackground;
		public static readonly Color4 Border = DesktopTheme.Colors.SeparatorColor;
	}

	public static class SceneViewColors
	{
		public static readonly Color4 Selection = Color4.Green;
		public static readonly Color4 LockedWidgetBorder = Color4.FromFloats(0, 1, 1);
		public static readonly Color4 ExposedItemInactiveBorder = Color4.Gray;
		public static readonly Color4 ExposedItemActiveBorder = Color4.White;
		public static readonly Color4 ExposedItemSelectedBorder = Selection;
		public static readonly Color4 Label = Color4.Green;
		public static readonly Color4 MouseSelection = Color4.Yellow;
		public static readonly Color4 ContainerOuterSpace = Color4.Gray;
		public static readonly Color4 ContainerInnerSpace = Color4.White;
		public static readonly Color4 ContainerBorder = Color4.Blue;
		public static readonly Color4 PointObject = Color4.Blue;
	}

	public static class TimelineGridColors
	{
		public static readonly Color4 PropertyRowBackground = Colors.GrayBackground.Lighten(0.5f);
		public static readonly Color4 Lines = Color4.White.Darken(0.25f);
		public static readonly Color4 Selection = Color4.Gray.Transparentify(0.5f);
		public static readonly Color4 Cursor = Color4.Red.Lighten(0.4f);
		public static readonly Color4 RunningCursor = Color4.Green.Lighten(0.4f);
	}

	public static class TimelineRulerColors
	{
		public static readonly Color4 Notchings = TimelineGridColors.Lines;
		public static readonly Color4 JumpMarker = Color4.Yellow;
		public static readonly Color4 PlayMarker = Color4.Green;
		public static readonly Color4 StopMarker = Color4.Red;
		public static readonly Color4 UnknownMarker = Color4.Black;
		public static readonly Color4 Cursor = TimelineGridColors.Cursor;
		public static readonly Color4 RunningCursor = TimelineGridColors.RunningCursor;
	}

	public static class TimelineOverviewColors
	{
		public static readonly Color4 Veil = Color4.White.Darken(0.2f).Transparentify(0.3f);
		public static readonly Color4 Border = Color4.White.Darken(0.2f);
	}

	public static class TimelineRollColors
	{
		public static readonly Color4 Lines = TimelineGridColors.Lines;
		public static readonly Color4 GrayedLabel = DesktopTheme.Colors.BlackText.Lighten(0.5f);
	}

	public static class DockingColors
	{
		public static readonly Color4 DragRectagleOutline = Color4.FromFloats(0.2f, 0.2f, 1f);
		public static readonly Color4 PanelTitleBackground = DesktopTheme.Colors.GrayBackground.Darken(0.1f);
		public static readonly Color4 PanelTitleSeparator = DesktopTheme.Colors.GrayBackground.Darken(0.15f);
	}

	public static class InspectorColors
	{
		public static readonly Color4 BorderAroundKeyframeColorbox = DesktopTheme.Colors.ControlBorder;
		public static readonly Color4 CategoryLabelBackground = Color4.White.Darken(0.13f);
	}
	*/

	public static class Colors
	{
		public static readonly Color4 WhiteBackground = DesktopTheme.Colors.WhiteBackground;
		public static readonly Color4 GrayBackground = DesktopTheme.Colors.GrayBackground;
		public static readonly Color4 SelectedBackground = DesktopTheme.Colors.SelectedBackground;
		public static readonly Color4 DragCursor = new Color4(254, 170, 24);
	}

	public static class ToolbarColors
	{
		public static readonly Color4 ButtonHighlightBorder = DesktopTheme.Colors.KeyboardFocusBorder.Lighten(0.2f);
		public static readonly Color4 ButtonHighlightBackground = DesktopTheme.Colors.KeyboardFocusBorder.Darken(0.3f);
		public static readonly Color4 ButtonPressBorder = ButtonHighlightBorder;
		public static readonly Color4 ButtonPressBackground = ButtonHighlightBackground.Lighten(0.1f);
		public static readonly Color4 ButtonCheckedBorder = ButtonPressBorder.Lighten(0.1f);
		public static readonly Color4 ButtonCheckedBackground = ButtonPressBackground.Transparentify(0.5f);
		public static readonly Color4 ButtonDisabledColor = Color4.Gray.Lighten(0.1f);
		public static readonly Color4 Background = DesktopTheme.Colors.GrayBackground;
		public static readonly Color4 Border = DesktopTheme.Colors.SeparatorColor;
	}

	public static class SceneViewColors
	{
		public static readonly Color4 Selection = Color4.Green;
		public static readonly Color4 LockedWidgetBorder = Color4.FromFloats(0, 1, 1);
		public static readonly Color4 ExposedItemInactiveBorder = Color4.Gray;
		public static readonly Color4 ExposedItemActiveBorder = Color4.White;
		public static readonly Color4 ExposedItemSelectedBorder = Selection;
		public static readonly Color4 Label = Color4.Green;
		public static readonly Color4 MouseSelection = Color4.Yellow;
		public static readonly Color4 ContainerOuterSpace = Color4.Gray;
		public static readonly Color4 ContainerInnerSpace = Color4.White;
		public static readonly Color4 ContainerBorder = Color4.Blue;
		public static readonly Color4 PointObject = Color4.Blue;
	}

	public static class TimelineGridColors
	{
		public static readonly Color4 PropertyRowBackground = Colors.GrayBackground.Darken(0.5f);
		public static readonly Color4 Lines = new Color4(45, 45, 48);
		public static readonly Color4 Selection = Color4.Gray.Transparentify(0.5f);
		public static readonly Color4 Cursor = new Color4(163, 0, 0).Darken(0.15f);
		public static readonly Color4 RunningCursor = new Color4(0, 163, 0).Darken(0.15f);
	}

	public static class TimelineRulerColors
	{
		public static readonly Color4 Notchings = TimelineGridColors.Lines;
		public static readonly Color4 JumpMarker = new Color4(209, 206, 0);
		public static readonly Color4 PlayMarker = new Color4(0, 163, 0);
		public static readonly Color4 StopMarker = new Color4(163, 0, 0);
		public static readonly Color4 UnknownMarker = Color4.Black;
		public static readonly Color4 Cursor = TimelineGridColors.Cursor;
		public static readonly Color4 RunningCursor = TimelineGridColors.RunningCursor;
	}

	public static class TimelineOverviewColors
	{
		public static readonly Color4 Veil = Color4.White.Darken(0.2f).Transparentify(0.3f);
		public static readonly Color4 Border = Color4.White.Darken(0.2f);
	}

	public static class TimelineRollColors
	{
		public static readonly Color4 Lines = TimelineGridColors.Lines;
		public static readonly Color4 GrayedLabel = DesktopTheme.Colors.BlackText.Darken(0.5f);
	}

	public static class DockingColors
	{
		public static readonly Color4 DragRectagleOutline = new Color4(51, 51, 255);
		public static readonly Color4 PanelTitleBackground = DesktopTheme.Colors.GrayBackground.Lighten(0.1f);
		public static readonly Color4 PanelTitleSeparator = DesktopTheme.Colors.GrayBackground.Lighten(0.15f);
	}

	public static class InspectorColors
	{
		public static readonly Color4 BorderAroundKeyframeColorbox = DesktopTheme.Colors.ControlBorder;
		public static readonly Color4 CategoryLabelBackground = Color4.Black.Lighten(0.13f);
	}
}
