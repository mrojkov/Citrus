using System;
using Lime;

namespace Tangerine.UI
{
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
		public static readonly ColorGradient Background = new ColorGradient(Color4.White, Color4.White.Darken(0.2f));
		public static readonly Color4 Border = DesktopTheme.Colors.ControlBorder;
	}

	public static class SceneViewColors
	{
		public static readonly Color4 SelectedWidgetBorder = Color4.Green;
		public static readonly Color4 LockedWidgetBorder = Color4.FromFloats(0, 1, 1);
		public static readonly Color4 ExposedItemInactiveBorder = Color4.Gray;
		public static readonly Color4 ExposedItemActiveBorder = Color4.White;
		public static readonly Color4 ExposedItemSelectedBorder = SelectedWidgetBorder;
		public static readonly Color4 Label = Color4.Green;
		public static readonly Color4 MouseSelection = Color4.Yellow;
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
		public static readonly Color4 Veil = Color4.White.Darken(0.3f).Transparentify(0.3f);
		public static readonly Color4 Border = Color4.White.Darken(0.4f);
	}
	public static class TimelineRollColors
	{
		public static readonly Color4 Lines = TimelineGridColors.Lines;
	}

	public static class DockingColors
	{
		public static readonly Color4 DragRectagleOutline = Color4.FromFloats(0.2f, 0.2f, 1f);
		public static readonly Color4 PanelBorder = Color4.White.Darken(0.75f);
	}

	public static class InspectorColors
	{
		public static readonly Color4 BorderAroundKeyframeColorbox = DesktopTheme.Colors.ControlBorder;
		public static readonly Color4 CategoryLabelBackground = Color4.White.Darken(0.2f);
	}
}
