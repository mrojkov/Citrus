using Lime;
using Yuzu;

namespace Tangerine.UI
{
	public class ColorTheme
	{
		public class ToolbarColors : Theme.DefaultColors
		{
			[YuzuOptional]
			public Color4 ButtonHighlightBorder { get; set; }
			[YuzuOptional]
			public Color4 ButtonHighlightBackground { get; set; }
			[YuzuOptional]
			public Color4 ButtonPressBorder { get; set; }
			[YuzuOptional]
			public Color4 ButtonPressBackground { get; set; }
			[YuzuOptional]
			public Color4 ButtonCheckedBorder { get; set; }
			[YuzuOptional]
			public Color4 ButtonCheckedBackground { get; set; }
			[YuzuOptional]
			public Color4 ButtonDisabledColor { get; set; }
			[YuzuOptional]
			public Color4 Background { get; set; }
			[YuzuOptional]
			public Color4 ButtonSelected { get; set; }
			[YuzuOptional]
			public Color4 PanelPlacementHighlightBackground { get; set; }
			[YuzuOptional]
			public Color4 PanelPlacementHighlightBorder { get; set; }
			[YuzuOptional]
			public Color4 Separator { get; set; }
		}

		public class SceneViewColors : Theme.DefaultColors
		{
			[YuzuOptional]
			public Color4 Selection { get; set; }
			[YuzuOptional]
			public Color4 SelectedWidgetPivotOutline { get; set; }
			[YuzuOptional]
			public Color4 LockedWidgetBorder { get; set; }
			[YuzuOptional]
			public Color4 ExposedItemInactiveBorder { get; set; }
			[YuzuOptional]
			public Color4 ExposedItemActiveBorder { get; set; }
			[YuzuOptional]
			public Color4 ExposedItemSelectedBorder { get; set; }
			[YuzuOptional]
			public Color4 Label { get; set; }
			[YuzuOptional]
			public Color4 MouseSelection { get; set; }
			[YuzuOptional]
			public Color4 ContainerOuterSpace { get; set; }
			[YuzuOptional]
			public Color4 ContainerInnerSpace { get; set; }
			[YuzuOptional]
			public Color4 ContainerBorder { get; set; }
			[YuzuOptional]
			public Color4 PointObject { get; set; }
			[YuzuOptional]
			public Color4 Bone { get; set; }
			[YuzuOptional]
			public Color4 BoneOutline { get; set; }
			[YuzuOptional]
			public Color4 BoneEffectiveRadius { get; set; }
			[YuzuOptional]
			public Color4 BoneFadeoutZone { get; set; }
			[YuzuOptional]
			public Color4 BackgroundColorA { get; set; }
			[YuzuOptional]
			public Color4 BackgroundColorB { get; set; }
			[YuzuOptional]
			public Color4 RootWidgetOverlayColor { get; set; }
			[YuzuOptional]
			public Color4 DistortionMeshOutline { get; set; }
			[YuzuOptional]
			public Color4 Ruler { get; set; }
			[YuzuOptional]
			public Color4 RulerEditable { get; set; }
			[YuzuOptional]
			public Color4 RulerEditableActiveDraging { get; set; }
			[YuzuOptional]
			public Color4 SelectedWidget { get; set; }
			[YuzuOptional]
			public Color4 RulerBackground { get; set; }
			[YuzuOptional]
			public Color4 RulerTextColor { get; set; }
			[YuzuOptional]
			public Color4 ResolutionPreviewOuterSpace { get; set; }
			[YuzuOptional]
			public Color4 ResolutionPreviewText { get; set; }
			[YuzuOptional]
			public Color4 EmitterCustomShape { get; set; }
			[YuzuOptional]
			public Color4 EmitterCustomShapeLine { get; set; }
			[YuzuOptional]
			public Color4 SplineOutline { get; set; }
			[YuzuOptional]
			public Color4 FrameProgressionBeginColor { get; set; }
			[YuzuOptional]
			public Color4 FrameProgressionEndColor { get; set; }
		}

		public class TimelineGridColors : Theme.DefaultColors
		{
			[YuzuOptional]
			public Color4 PropertyRowBackground { get; set; }
			[YuzuOptional]
			public Color4 Lines { get; set; }
			[YuzuOptional]
			public Color4 LinesLight { get; set; }
			[YuzuOptional]
			public Color4 Selection { get; set; }
			[YuzuOptional]
			public Color4 Backlight { get; set; }
			[YuzuOptional]
			public Color4 Cursor { get; set; }
			[YuzuOptional]
			public Color4 RunningCursor { get; set; }
			[YuzuOptional]
			public Color4 WaveformColor { get; set; }
			[YuzuOptional]
			public Color4 WaveformBackground { get; set; }
			[YuzuOptional]
			public Color4 AnimatedRangeBackground { get; set; }
			[YuzuOptional]
			public Color4 SelectedRowBackground { get; set; }
			[YuzuOptional]
			public Color4 SelectionBorder { get; set; }
			[YuzuOptional]
			public Color4 AnimationClip { get; set; }
			[YuzuOptional]
			public Color4 AnimationClipBorder { get; set; }
		}

		public class TimelineCurveEditorColors : Theme.DefaultColors
		{
			[YuzuOptional]
			public Color4[] Curves { get; set; }
			[YuzuOptional]
			public Color4 Selection { get; set; }
		}

		public class TimelineRulerColors : Theme.DefaultColors
		{
			[YuzuOptional]
			public Color4 Notchings { get; set; }
			[YuzuOptional]
			public Color4 JumpMarker { get; set; }
			[YuzuOptional]
			public Color4 PlayMarker { get; set; }
			[YuzuOptional]
			public Color4 StopMarker { get; set; }
			[YuzuOptional]
			public Color4 UnknownMarker { get; set; }
			[YuzuOptional]
			public Color4 Cursor { get; set; }
			[YuzuOptional]
			public Color4 RunningCursor { get; set; }
			[YuzuOptional]
			public Color4 MarkerBorder { get; set; }
		}

		public class TimelineOverviewColors : Theme.DefaultColors
		{
			[YuzuOptional]
			public Color4 Veil { get; set; }
			[YuzuOptional]
			public Color4 Border { get; set; }
		}

		public class TimelineRollColors : Theme.DefaultColors
		{
			[YuzuOptional]
			public Color4 Lines { get; set; }
			[YuzuOptional]
			public Color4 GrayedLabel { get; set; }
			[YuzuOptional]
			public Color4 DragCursor { get; set; }
			[YuzuOptional]
			public Color4 DragTarget { get; set; }
			[YuzuOptional]
			public Color4 BlueMark { get; set; }
			[YuzuOptional]
			public Color4 RedMark { get; set; }
			[YuzuOptional]
			public Color4 GreenMark { get; set; }
			[YuzuOptional]
			public Color4 OrangeMark { get; set; }
			[YuzuOptional]
			public Color4 YellowMark { get; set; }
			[YuzuOptional]
			public Color4 VioletMark { get; set; }
			[YuzuOptional]
			public Color4 GrayMark { get; set; }
		}

		public class DockingColors : Theme.DefaultColors
		{
			[YuzuOptional]
			public Color4 DragRectagleOutline { get; set; }
			[YuzuOptional]
			public Color4 PanelTitleBackground { get; set; }
			[YuzuOptional]
			public Color4 PanelTitleSeparator { get; set; }
			[YuzuOptional]
			public Color4 TabBarBackground { get; set; }
			[YuzuOptional]
			public Color4 TabHighlighted { get; set; }
			[YuzuOptional]
			public Color4 TabOutline { get; set; }
		}

		public class InspectorColors : Theme.DefaultColors
		{
			[YuzuOptional]
			public Color4 BorderAroundKeyframeColorbox { get; set; }
			[YuzuOptional]
			public Color4 CategoryLabelBackground { get; set; }
			[YuzuOptional]
			public Color4 GroupHeaderLabelBackground { get; set; }
			[YuzuOptional]
			public Color4 ComponentHeaderLabelBackground { get; set; }
			[YuzuOptional]
			public Color4 StripeBackground1 { get; set; }
			[YuzuOptional]
			public Color4 StripeBackground2 { get; set; }
		}

		public class KeyboardColors : Theme.DefaultColors
		{
			[YuzuOptional]
			public Color4 BlackText { get; set; }
			[YuzuOptional]
			public Color4 GrayText { get; set; }
			[YuzuOptional]
			public Color4 GenericKeyBackground { get; set; }
			[YuzuOptional]
			public Color4 PanelKeyBackground { get; set; }
			[YuzuOptional]
			public Color4 ButtonBackground { get; set; }
			[YuzuOptional]
			public Color4 ModifierBackground { get; set; }
			[YuzuOptional]
			public Color4 SelectedBorder { get; set; }
			[YuzuOptional]
			public Color4 Border { get; set; }
		}

		public class HierarchyColors : Theme.DefaultColors
		{
			[YuzuOptional]
			public Color4 SelectionColor { get; set; }
			[YuzuOptional]
			public Color4 GrayedSelectionColor { get; set; }
			[YuzuOptional]
			public Color4 MatchColor { get; set; }
			[YuzuOptional]
			public Color4 JointColor { get; set; }
		}

		[YuzuOptional]
		public bool IsDark { get; set; }
		[YuzuOptional]
		public ToolbarColors Toolbar { get; set; }
		[YuzuOptional]
		public SceneViewColors SceneView { get; set; }
		[YuzuOptional]
		public TimelineGridColors TimelineGrid { get; set; }
		[YuzuOptional]
		public TimelineCurveEditorColors TimelineCurveEditor { get; set; }
		[YuzuOptional]
		public TimelineRulerColors TimelineRuler { get; set; }
		[YuzuOptional]
		public TimelineOverviewColors TimelineOverview { get; set; }
		[YuzuOptional]
		public TimelineRollColors TimelineRoll { get; set; }
		[YuzuOptional]
		public DockingColors Docking { get; set; }
		[YuzuOptional]
		public InspectorColors Inspector { get; set; }
		[YuzuOptional]
		public KeyboardColors Keyboard { get; set; }
		[YuzuOptional]
		public HierarchyColors Hierarchy { get; set; }

		public ColorTheme Clone()
		{
			var clone = (ColorTheme)this.MemberwiseClone();
			return clone;
		}

		public enum ColorThemeKind
		{
			Light,
			Dark,
			Custom,
		}

		public static ColorTheme Current = CreateLightTheme();

		public static ColorTheme CreateDarkTheme()
		{
			var basic = Theme.ColorTheme.CreateDarkTheme();
			var toolbuttonHighlightBorder = basic.KeyboardFocusBorder.Lighten(0.2f);
			var toolbuttonHighlightBackground = basic.KeyboardFocusBorder.Darken(0.3f);
			var toolbar = new ToolbarColors {
				ButtonHighlightBorder = toolbuttonHighlightBorder,
				ButtonHighlightBackground = toolbuttonHighlightBackground,
				ButtonPressBorder = toolbuttonHighlightBorder,
				ButtonPressBackground = toolbuttonHighlightBackground.Lighten(0.1f),
				ButtonCheckedBorder = toolbuttonHighlightBorder.Lighten(0.1f),
				ButtonCheckedBackground = toolbuttonHighlightBackground.Transparentify(0.5f),
				ButtonDisabledColor = Color4.Gray.Lighten(0.1f),
				Background = basic.GrayBackground,
				ButtonSelected = toolbuttonHighlightBorder,
				PanelPlacementHighlightBackground = toolbuttonHighlightBackground.Transparentify(0.7f),
				PanelPlacementHighlightBorder = toolbuttonHighlightBorder,
				Separator = Color4.Gray.Lighten(0.3f),
			};
			var sceneView = new SceneViewColors {
				Selection = Color4.Green,
				SelectedWidgetPivotOutline = Color4.Red,
				LockedWidgetBorder = Color4.FromFloats(0, 1, 1),
				ExposedItemInactiveBorder = Color4.Gray,
				ExposedItemActiveBorder = Color4.White,
				ExposedItemSelectedBorder = Color4.Green,
				Label = Color4.Green,
				MouseSelection = Color4.Yellow,
				ContainerOuterSpace = Color4.Gray,
				ContainerInnerSpace = Color4.White,
				ContainerBorder = Color4.Blue,
				PointObject = Color4.Red.Lighten(0.3f),
				SplineOutline = Color4.White,
				Bone = new Color4(136, 136, 136, 128),
				BoneOutline = new Color4(105, 105, 105),
				BoneEffectiveRadius = Color4.Yellow,
				BoneFadeoutZone = Color4.Red,
				BackgroundColorA = Color4.Gray,
				BackgroundColorB = Color4.Gray.Darken(0.15f),
				RootWidgetOverlayColor = Color4.White.Transparentify(0.8f),
				DistortionMeshOutline = new Color4(0, 255, 255),
				RulerEditable = new Color4(255, 0, 255),
				RulerEditableActiveDraging = Color4.White,
				Ruler = new Color4(0, 255, 255),
				SelectedWidget = new Color4(0, 255, 255),
				RulerBackground = Color4.DarkGray,
				RulerTextColor = Color4.Gray.Lighten(0.4f),
				ResolutionPreviewOuterSpace = new Color4(7, 7, 7, 220),
				ResolutionPreviewText = new Color4(204, 204, 204),
				EmitterCustomShape = Color4.Gray.Darken(0.3f).Transparentify(0.5f),
				EmitterCustomShapeLine = Color4.White.Darken(0.05f),
			};
			var timelineGrid = new TimelineGridColors {
				PropertyRowBackground = Color4.White.Transparentify(0.95f),
				Lines = new Color4(45, 45, 48),
				LinesLight = new Color4(45, 45, 48).Lighten(0.065f),
				Selection = new Color4(140, 140, 140, 128),
				Backlight = Color4.Gray.Transparentify(0.25f),
				Cursor = new Color4(163, 0, 0).Darken(0.15f),
				RunningCursor = new Color4(0, 163, 0).Darken(0.15f),
				WaveformColor = new Color4(170, 255, 140),
				WaveformBackground = new Color4(160, 160, 220, 60),
				AnimatedRangeBackground = new Color4(85, 85, 85),
				SelectedRowBackground = new Color4(100, 100, 100),
				SelectionBorder = new Color4(0, 255, 255),
				AnimationClip = new Color4(50, 50, 255, 80),
				AnimationClipBorder = new Color4(50, 50, 255),
			};
			var timelineCurveEditor = new TimelineCurveEditorColors {
				Curves = new[] { Color4.Red, Color4.Green, Color4.Blue, Color4.Yellow },
				Selection = Color4.Green,
			};
			var timelineRuler = new TimelineRulerColors {
				Notchings = timelineGrid.Lines,
				JumpMarker = new Color4(209, 206, 0),
				PlayMarker = new Color4(0, 163, 0),
				StopMarker = new Color4(163, 0, 0),
				UnknownMarker = Color4.Black,
				Cursor = timelineGrid.Cursor,
				RunningCursor = timelineGrid.RunningCursor,
				MarkerBorder = Color4.White,
			};
			var timelineOverview = new TimelineOverviewColors {
				Veil = Color4.White.Darken(0.2f).Transparentify(0.6f),
				Border = Color4.White.Darken(0.2f),
			};
			var timelineRoll = new TimelineRollColors {
				Lines = timelineGrid.Lines,
				GrayedLabel = basic.BlackText.Darken(0.5f),
				DragCursor = new Color4(254, 170, 24),
				DragTarget = new Color4(254, 170, 24, 20),
				BlueMark = new Color4(0, 255, 255).Darken(0.15f),
				RedMark = Color4.Red.Darken(0.15f),
				GreenMark = Color4.Green.Darken(0.15f),
				OrangeMark = Color4.Orange.Darken(0.15f),
				YellowMark = Color4.Yellow.Darken(0.15f),
				VioletMark = new Color4(238, 130, 238).Darken(0.15f),
				GrayMark = Color4.Gray.Darken(0.15f),
			};
			var docking = new DockingColors {
				DragRectagleOutline = new Color4(0, 255, 255),
				PanelTitleBackground = basic.GrayBackground.Lighten(0.1f),
				PanelTitleSeparator = basic.GrayBackground.Lighten(0.15f),
				TabBarBackground = basic.GrayBackground,
				TabHighlighted = basic.TabNormal,
				TabOutline = basic.TabNormal,

			};
			var inspector = new InspectorColors {
				BorderAroundKeyframeColorbox = basic.ControlBorder,
				CategoryLabelBackground = Color4.Black.Lighten(0.3f),
				GroupHeaderLabelBackground = Color4.Black.Lighten(0.275f),
				ComponentHeaderLabelBackground = Color4.Black.Lighten(0.375f),
				StripeBackground1 = basic.GrayBackground,
				StripeBackground2 = basic.GrayBackground.Lighten(0.05f),
			};
			var keyboard = new KeyboardColors {
				BlackText = Color4.White,
				GrayText = Color4.White.Darken(0.2f),
				GenericKeyBackground = new Color4(115, 75, 195),
				PanelKeyBackground = new Color4(30, 180, 140),
				ButtonBackground = new Color4(112, 112, 112),
				ModifierBackground = new Color4(80, 80, 80),
				SelectedBorder = new Color4(100, 255, 255),
				Border = new Color4(32, 32, 32),
			};
			var hierarchy = new HierarchyColors {
				SelectionColor = toolbar.ButtonHighlightBackground,
				GrayedSelectionColor = toolbar.ButtonHighlightBackground.Darken(0.3f),
				MatchColor = Color4.Yellow.Darken(0.5f).Transparentify(0.6f),
				JointColor = Color4.Gray,
			};
			return new ColorTheme {
				IsDark = true,
				Toolbar = toolbar,
				SceneView = sceneView,
				TimelineGrid = timelineGrid,
				TimelineCurveEditor = timelineCurveEditor,
				TimelineRuler = timelineRuler,
				TimelineOverview = timelineOverview,
				TimelineRoll = timelineRoll,
				Docking = docking,
				Inspector = inspector,
				Keyboard = keyboard,
				Hierarchy = hierarchy,
			};
		}

		public static ColorTheme CreateLightTheme()
		{
			var basic = Theme.ColorTheme.CreateLightTheme();
			var toolbuttonHighlightBorder = basic.KeyboardFocusBorder.Darken(0.2f);
			var toolbuttonHighlightBackground = basic.KeyboardFocusBorder.Lighten(0.3f);
			var toolbar = new ToolbarColors {
				ButtonHighlightBorder = toolbuttonHighlightBorder,
				ButtonHighlightBackground = toolbuttonHighlightBackground,
				ButtonPressBorder = toolbuttonHighlightBorder,
				ButtonPressBackground = toolbuttonHighlightBackground.Darken(0.1f),
				ButtonCheckedBorder = toolbuttonHighlightBorder.Darken(0.1f),
				ButtonCheckedBackground = toolbuttonHighlightBackground.Transparentify(0.5f),
				ButtonDisabledColor = Color4.Gray.Darken(0.1f),
				Background = basic.GrayBackground,
				ButtonSelected = toolbuttonHighlightBorder,
				PanelPlacementHighlightBackground = toolbuttonHighlightBackground.Transparentify(0.7f),
				PanelPlacementHighlightBorder = toolbuttonHighlightBorder,
				Separator = Color4.Gray.Lighten(0.3f),
			};
			var sceneView = new SceneViewColors {
				Selection = Color4.Green,
				SelectedWidgetPivotOutline = Color4.Red,
				LockedWidgetBorder = Color4.FromFloats(0, 1, 1),
				ExposedItemInactiveBorder = Color4.Gray,
				ExposedItemActiveBorder = Color4.White,
				ExposedItemSelectedBorder = Color4.Green,
				Label = Color4.Green,
				MouseSelection = Color4.Yellow,
				ContainerOuterSpace = Color4.Gray,
				ContainerInnerSpace = Color4.White,
				ContainerBorder = Color4.Blue,
				PointObject = Color4.Red.Lighten(0.2f),
				SplineOutline = Color4.White,
				Bone = new Color4(136, 136, 136, 128),
				BoneOutline = new Color4(105, 105, 105),
				BoneEffectiveRadius = Color4.Yellow,
				BoneFadeoutZone = Color4.Red,
				BackgroundColorA = new Color4(202, 202, 202),
				BackgroundColorB = new Color4(190, 190, 190),
				RootWidgetOverlayColor = new Color4(255, 255, 255, 85),
				DistortionMeshOutline = new Color4(0, 255, 255),
				RulerEditable = new Color4(255, 0, 255),
				RulerEditableActiveDraging = Color4.White,
				Ruler = new Color4(0, 255, 255),
				SelectedWidget = new Color4(0, 255, 255),
				RulerBackground = new Color4(168, 166, 168),
				RulerTextColor = Color4.Black,
				ResolutionPreviewOuterSpace = new Color4(7, 7, 7, 220),
				ResolutionPreviewText = new Color4(204, 204, 204),
				EmitterCustomShape = Color4.Gray.Darken(0.2f).Transparentify(0.5f),
				EmitterCustomShapeLine = Color4.White,
				FrameProgressionBeginColor = new Color4(0, 0, 255, 100),
				FrameProgressionEndColor = new Color4(255, 0, 0, 100),
			};
			var timelineGrid = new TimelineGridColors {
				PropertyRowBackground = Color4.Black.Transparentify(0.95f),
				Lines = new Color4(160, 160, 160),
				LinesLight = new Color4(210, 210, 210),
				Selection = Color4.Gray.Transparentify(0.7f),
				Backlight = Color4.Gray.Transparentify(0.75f),
				Cursor = Color4.Red.Lighten(0.4f),
				RunningCursor = Color4.Green.Lighten(0.4f),
				WaveformColor = new Color4(140, 170, 255),
				WaveformBackground = new Color4(255, 200, 140, 60),
				AnimatedRangeBackground = new Color4(240, 240, 240),
				SelectedRowBackground = new Color4(225, 225, 225),
				SelectionBorder = new Color4(255, 0, 128),
				AnimationClip = new Color4(50, 50, 255, 80),
				AnimationClipBorder = new Color4(50, 50, 255),
			};
			var timelineCurveEditor = new TimelineCurveEditorColors {
				Curves = new[] { Color4.Red, Color4.Green, Color4.Blue, Color4.Yellow },
				Selection = Color4.Green,
			};
			var timelineRuler = new TimelineRulerColors {
				Notchings = timelineGrid.Lines,
				JumpMarker = Color4.Yellow,
				PlayMarker = Color4.Green,
				StopMarker = Color4.Red,
				UnknownMarker = Color4.Black,
				Cursor = timelineGrid.Cursor,
				RunningCursor = timelineGrid.RunningCursor,
				MarkerBorder = Color4.Black,
			};
			var timelineOverview = new TimelineOverviewColors {
				Veil = Color4.White.Darken(0.2f).Transparentify(0.3f),
				Border = Color4.White.Darken(0.2f),
			};
			var timelineRoll = new TimelineRollColors {
				Lines = timelineGrid.Lines,
				GrayedLabel = basic.BlackText.Lighten(0.5f),
				DragCursor = Color4.Black,
				DragTarget = new Color4(0, 0, 0, 20),
				BlueMark = new Color4(0, 255, 255),
				RedMark = Color4.Red,
				GreenMark = Color4.Green,
				OrangeMark = Color4.Orange,
				YellowMark = Color4.Yellow,
				VioletMark = new Color4(238, 130, 238),
				GrayMark = Color4.Gray,
			};
			var docking = new DockingColors {
				DragRectagleOutline = Color4.FromFloats(0.2f, 0.2f, 1f),
				PanelTitleBackground = basic.GrayBackground.Darken(0.1f),
				PanelTitleSeparator = basic.GrayBackground.Darken(0.15f),
				TabBarBackground = basic.GrayBackground,
				TabHighlighted = Color4.White.Darken(0.1f),
				TabOutline = basic.GrayBackground.Darken(0.5f),
			};
			var inspector = new InspectorColors {
				BorderAroundKeyframeColorbox = basic.ControlBorder,
				CategoryLabelBackground = Color4.White.Darken(0.2f),
				GroupHeaderLabelBackground = Color4.White.Darken(0.175f),
				ComponentHeaderLabelBackground = Color4.White.Darken(0.275f),
				StripeBackground1 = basic.GrayBackground,
				StripeBackground2 = basic.GrayBackground.Darken(0.05f),
			};
			var keyboard = new KeyboardColors {
				BlackText = Color4.Black,
				GrayText = Color4.Black.Lighten(0.2f),
				GenericKeyBackground = new Color4(180, 255, 255),
				PanelKeyBackground = new Color4(255, 255, 128),
				ButtonBackground = new Color4(220, 220, 255),
				ModifierBackground = new Color4(160, 160, 190),
				SelectedBorder = new Color4(255, 0, 0),
				Border = new Color4(32, 32, 32),
			};
			var hierarchy = new HierarchyColors {
				SelectionColor = toolbar.ButtonHighlightBackground,
				GrayedSelectionColor = toolbar.ButtonHighlightBackground.Darken(0.3f),
				MatchColor = Color4.Yellow.Transparentify(0.6f),
				JointColor = Color4.Gray,
			};
			return new ColorTheme {
				IsDark = false,
				Toolbar = toolbar,
				SceneView = sceneView,
				TimelineGrid = timelineGrid,
				TimelineCurveEditor = timelineCurveEditor,
				TimelineRuler = timelineRuler,
				TimelineOverview = timelineOverview,
				TimelineRoll = timelineRoll,
				Docking = docking,
				Inspector = inspector,
				Keyboard = keyboard,
				Hierarchy = hierarchy,
			};
		}
	}
}
