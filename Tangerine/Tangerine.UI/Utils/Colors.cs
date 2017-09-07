using System;
using Lime;

namespace Tangerine.UI
{
	public class ColorTheme
	{
		public class ToolbarColors
		{
			public Color4 ButtonHighlightBorder;
			public Color4 ButtonHighlightBackground;
			public Color4 ButtonPressBorder;
			public Color4 ButtonPressBackground;
			public Color4 ButtonCheckedBorder;
			public Color4 ButtonCheckedBackground;
			public Color4 ButtonDisabledColor;
			public Color4 Background;
			public Color4 Border;
		}

		public class SceneViewColors
		{
			public Color4 Selection;
			public Color4 LockedWidgetBorder;
			public Color4 ExposedItemInactiveBorder;
			public Color4 ExposedItemActiveBorder;
			public Color4 ExposedItemSelectedBorder;
			public Color4 Label;
			public Color4 MouseSelection;
			public Color4 ContainerOuterSpace;
			public Color4 ContainerInnerSpace;
			public Color4 ContainerBorder;
			public Color4 PointObject;
			public Color4 Bone;
			public Color4 BoneOutline;
			public Color4 BoneEffectiveRadius;
			public Color4 BoneFadeoutZone;
			public Color4 BackgroundColorA;
			public Color4 BackgroundColorB;
			public Color4 RootWidgetOverlayColor;
			public Color4 DistortionMeshOutline;
		}

		public class TimelineGridColors
		{
			public Color4 PropertyRowBackground;
			public Color4 Lines;
			public Color4 LinesLight;
			public Color4 Selection;
			public Color4 Cursor;
			public Color4 RunningCursor;
			public Color4 WaveformColor;
			public Color4 WaveformBackground;
		}

		public class TimelineRulerColors
		{
			public Color4 Notchings;
			public Color4 JumpMarker;
			public Color4 PlayMarker;
			public Color4 StopMarker;
			public Color4 UnknownMarker;
			public Color4 Cursor;
			public Color4 RunningCursor;
		}

		public class TimelineOverviewColors
		{
			public Color4 Veil;
			public Color4 Border;
		}

		public class TimelineRollColors
		{
			public Color4 Lines;
			public Color4 GrayedLabel;
			public Color4 DragCursor;
		}

		public class DockingColors
		{
			public Color4 DragRectagleOutline;
			public Color4 PanelTitleBackground;
			public Color4 PanelTitleSeparator;
		}

		public class InspectorColors
		{
			public Color4 BorderAroundKeyframeColorbox;
			public Color4 CategoryLabelBackground;
			public Color4 StripeBackground1;
			public Color4 StripeBackground2;
		}

		public Theme.ColorTheme Basic;
		public ToolbarColors Toolbar;
		public SceneViewColors SceneView;
		public TimelineGridColors TimelineGrid;
		public TimelineRulerColors TimelineRuler;
		public TimelineOverviewColors TimelineOverview;
		public TimelineRollColors TimelineRoll;
		public DockingColors Docking;
		public InspectorColors Inspector;

		public static ColorTheme Current = CreateLightTheme();

		public static ColorTheme CreateDarkTheme()
		{
			var toolbuttonHighlightBorder = Theme.Colors.KeyboardFocusBorder.Lighten(0.2f);
			var toolbuttonHighlightBackground = Theme.Colors.KeyboardFocusBorder.Darken(0.3f);
			var toolbar = new ToolbarColors {
				ButtonHighlightBorder = toolbuttonHighlightBorder,
				ButtonHighlightBackground = toolbuttonHighlightBackground,
				ButtonPressBorder = toolbuttonHighlightBorder,
				ButtonPressBackground = toolbuttonHighlightBackground.Lighten(0.1f),
				ButtonCheckedBorder = toolbuttonHighlightBorder.Lighten(0.1f),
				ButtonCheckedBackground = toolbuttonHighlightBackground.Transparentify(0.5f),
				ButtonDisabledColor = Color4.Gray.Lighten(0.1f),
				Background = Theme.Colors.GrayBackground,
				Border = Theme.Colors.SeparatorColor
			};
			var sceneView = new SceneViewColors {
				Selection = Color4.Green,
				LockedWidgetBorder = Color4.FromFloats(0, 1, 1),
				ExposedItemInactiveBorder = Color4.Gray,
				ExposedItemActiveBorder = Color4.White,
				ExposedItemSelectedBorder = Color4.Green,
				Label = Color4.Green,
				MouseSelection = Color4.Yellow,
				ContainerOuterSpace = Color4.Gray,
				ContainerInnerSpace = Color4.White,
				ContainerBorder = Color4.Blue,
				PointObject = Color4.Gray.Lighten(0.3f),
				Bone = new Color4(136, 136, 136, 128),
				BoneOutline = new Color4(105, 105, 105),
				BoneEffectiveRadius = Color4.Yellow,
				BoneFadeoutZone = Color4.Red,
				BackgroundColorA = Color4.Gray,
				BackgroundColorB = Color4.Gray.Darken(0.15f),
				RootWidgetOverlayColor = Color4.White.Transparentify(0.8f),
				DistortionMeshOutline = new Color4(0, 255, 255),
			};
			var timelineGrid = new TimelineGridColors {
				PropertyRowBackground = Color4.White.Transparentify(0.95f),
				Lines = new Color4(45, 45, 48),
				LinesLight = new Color4(45, 45, 48).Lighten(0.065f),
				Selection = Color4.Gray.Transparentify(0.5f),
				Cursor = new Color4(163, 0, 0).Darken(0.15f),
				RunningCursor = new Color4(0, 163, 0).Darken(0.15f),
				WaveformColor = new Color4(170, 255, 140),
				WaveformBackground = new Color4(160, 160, 220, 60)
			};
			var timelineRuler = new TimelineRulerColors {
				Notchings = timelineGrid.Lines,
				JumpMarker = new Color4(209, 206, 0),
				PlayMarker = new Color4(0, 163, 0),
				StopMarker = new Color4(163, 0, 0),
				UnknownMarker = Color4.Black,
				Cursor = timelineGrid.Cursor,
				RunningCursor = timelineGrid.RunningCursor
			};
			var timelineOverview = new TimelineOverviewColors {
				Veil = Color4.White.Darken(0.2f).Transparentify(0.6f),
				Border = Color4.White.Darken(0.2f)
			};
			var timelineRoll = new TimelineRollColors {
				Lines = timelineGrid.Lines,
				GrayedLabel = Theme.Colors.BlackText.Darken(0.5f),
				DragCursor = new Color4(254, 170, 24)
			};
			var docking = new DockingColors {
				DragRectagleOutline = new Color4(0, 255, 255),
				PanelTitleBackground = Theme.Colors.GrayBackground.Lighten(0.1f),
				PanelTitleSeparator = Theme.Colors.GrayBackground.Lighten(0.15f)
			};
			var inspector = new InspectorColors {
				BorderAroundKeyframeColorbox = Theme.Colors.ControlBorder,
				CategoryLabelBackground = Color4.Black.Lighten(0.3f),
				StripeBackground1 = Theme.Colors.GrayBackground,
				StripeBackground2 = Theme.Colors.GrayBackground.Lighten(0.05f),
			};
			return new ColorTheme {
				Basic = Theme.Colors,
				Toolbar = toolbar,
				SceneView = sceneView,
				TimelineGrid = timelineGrid,
				TimelineRuler = timelineRuler,
				TimelineOverview = timelineOverview,
				TimelineRoll = timelineRoll,
				Docking = docking,
				Inspector = inspector
			};
		}

		public static ColorTheme CreateLightTheme()
		{
			var toolbuttonHighlightBorder = Theme.Colors.KeyboardFocusBorder.Darken(0.2f);
			var toolbuttonHighlightBackground = Theme.Colors.KeyboardFocusBorder.Lighten(0.3f);
			var toolbar = new ToolbarColors {
				ButtonHighlightBorder = toolbuttonHighlightBorder,
				ButtonHighlightBackground = toolbuttonHighlightBackground,
				ButtonPressBorder = toolbuttonHighlightBorder,
				ButtonPressBackground = toolbuttonHighlightBackground.Darken(0.1f),
				ButtonCheckedBorder = toolbuttonHighlightBorder.Darken(0.1f),
				ButtonCheckedBackground = toolbuttonHighlightBackground.Transparentify(0.5f),
				ButtonDisabledColor = Color4.Gray.Darken(0.1f),
				Background = Theme.Colors.GrayBackground,
				Border = Theme.Colors.SeparatorColor
			};
			var sceneView = new SceneViewColors {
				Selection = Color4.Green,
				LockedWidgetBorder = Color4.FromFloats(0, 1, 1),
				ExposedItemInactiveBorder = Color4.Gray,
				ExposedItemActiveBorder = Color4.White,
				ExposedItemSelectedBorder = Color4.Green,
				Label = Color4.Green,
				MouseSelection = Color4.Yellow,
				ContainerOuterSpace = Color4.Gray,
				ContainerInnerSpace = Color4.White,
				ContainerBorder = Color4.Blue,
				PointObject = Color4.Gray.Lighten(0.2f),
				Bone = new Color4(136, 136, 136, 128),
				BoneOutline = new Color4(105, 105, 105),
				BoneEffectiveRadius = Color4.Yellow,
				BoneFadeoutZone = Color4.Red,
				BackgroundColorA = new Color4(202, 202, 202),
				BackgroundColorB = new Color4(190, 190, 190),
				RootWidgetOverlayColor = new Color4(255, 255, 255, 85),
				DistortionMeshOutline = new Color4(0, 255, 255),
			};
			var timelineGrid = new TimelineGridColors {
				PropertyRowBackground = Color4.Black.Transparentify(0.95f),
				Lines = new Color4(160, 160, 160),
				LinesLight = new Color4(240, 240, 240),
				Selection = Color4.Gray.Transparentify(0.5f),
				Cursor = Color4.Red.Lighten(0.4f),
				RunningCursor = Color4.Green.Lighten(0.4f),
				WaveformColor = new Color4(140, 170, 255),
				WaveformBackground = new Color4(255, 200, 140, 60)
			};
			var timelineRuler = new TimelineRulerColors {
				Notchings = timelineGrid.Lines,
				JumpMarker = Color4.Yellow,
				PlayMarker = Color4.Green,
				StopMarker = Color4.Red,
				UnknownMarker = Color4.Black,
				Cursor = timelineGrid.Cursor,
				RunningCursor = timelineGrid.RunningCursor
			};
			var timelineOverview = new TimelineOverviewColors {
				Veil = Color4.White.Darken(0.2f).Transparentify(0.3f),
				Border = Color4.White.Darken(0.2f)
			};
			var timelineRoll = new TimelineRollColors {
				Lines = timelineGrid.Lines,
				GrayedLabel = Theme.Colors.BlackText.Lighten(0.5f),
				DragCursor = Color4.Black
			};
			var docking = new DockingColors {
				DragRectagleOutline = Color4.FromFloats(0.2f, 0.2f, 1f),
				PanelTitleBackground = Theme.Colors.GrayBackground.Darken(0.1f),
				PanelTitleSeparator = Theme.Colors.GrayBackground.Darken(0.15f)
			};
			var inspector = new InspectorColors {
				BorderAroundKeyframeColorbox = Theme.Colors.ControlBorder,
				CategoryLabelBackground = Color4.White.Darken(0.2f),
				StripeBackground1 = Theme.Colors.GrayBackground,
				StripeBackground2 = Theme.Colors.GrayBackground.Darken(0.05f),
			};
			return new ColorTheme {
				Basic = Theme.Colors,
				Toolbar = toolbar,
				SceneView = sceneView,
				TimelineGrid = timelineGrid,
				TimelineRuler = timelineRuler,
				TimelineOverview = timelineOverview,
				TimelineRoll = timelineRoll,
				Docking = docking,
				Inspector = inspector
			};
		}
	}
}
