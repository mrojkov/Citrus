using System;
using Lime;
using Yuzu;

namespace Tangerine.UI.SceneView
{
	public class UserPreferences : Component
	{
		[YuzuRequired]
		public float DefaultBoneWidth { get; set; }

		[YuzuRequired]
		public bool EnableChessBackground { get; set; }

		[YuzuRequired]
		public Color4 BackgroundColorA { get; set; }

		[YuzuRequired]
		public Color4 BackgroundColorB { get; set; }

		[YuzuRequired]
		public Color4 RootWidgetOverlayColor { get; set; }

		[YuzuRequired]
		public Color4 AnimationPreviewBackground { get; set; }

		[YuzuRequired]
		public bool DrawFrameBorder { get; set; }

		public UserPreferences()
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
		}
	}
}