using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Yuzu;

namespace Tangerine.UI.SceneView
{
	public class SceneUserPreferences : Component
	{
		[YuzuOptional]
		public float DefaultBoneWidth { get; set; }

		[YuzuOptional]
		public bool EnableChessBackground { get; set; }

		[YuzuOptional]
		public Color4 BackgroundColorA { get; set; }

		[YuzuOptional]
		public Color4 BackgroundColorB { get; set; }

		[YuzuOptional]
		public Color4 RootWidgetOverlayColor { get; set; }

		[YuzuOptional]
		public Color4 AnimationPreviewBackground { get; set; }

		[YuzuOptional]
		public bool DrawFrameBorder { get; set; }

		[YuzuOptional]
		public bool SnapWidgetBorderToRuler { get; set; }

		[YuzuOptional]
		public bool SnapWidgetPivotToRuler { get; set; }

		[YuzuOptional]
		public bool SnapRulerLinesToWidgets { get; set; }

		[YuzuOptional]
		public VisualHintsRegistry VisualHintsRegistry { get; set; }

		public SceneUserPreferences()
		{
			ResetToDefaults();
		}

		public void ResetToDefaults()
		{
			DefaultBoneWidth = 5;
			EnableChessBackground = true;
			BackgroundColorA = ColorTheme.Current.SceneView.BackgroundColorA;
			BackgroundColorB = ColorTheme.Current.SceneView.BackgroundColorB;
			RootWidgetOverlayColor = ColorTheme.Current.SceneView.RootWidgetOverlayColor;
			AnimationPreviewBackground = Color4.Black.Transparentify(0.6f);
			DrawFrameBorder = false;
			SnapRulerLinesToWidgets = false;
			SnapWidgetBorderToRuler = false;
			SnapWidgetPivotToRuler = false;
			VisualHintsRegistry = new VisualHintsRegistry();
		}

		public static SceneUserPreferences Instance => Core.UserPreferences.Instance.Get<SceneUserPreferences>();
	}
}
