using System;
using Lime;
using Yuzu;

namespace Tangerine.UI.SceneView
{
	public class UserPreferences : Component
	{
		[YuzuRequired]
		public bool ShowOverlays { get; set; }

		[YuzuRequired]
		public float DefaultBoneWidth { get; set; } = 5;

		[YuzuRequired]
		public bool EnableChessBackground { get; set; } = true;

		[YuzuRequired]
		public Color4 BackgroundColorA { get; set; } = ColorTheme.Current.SceneView.BackgroundColorA;

		[YuzuRequired]
		public Color4 BackgroundColorB { get; set; } = ColorTheme.Current.SceneView.BackgroundColorB;

		[YuzuRequired]
		public Color4 RootWidgetOverlayColor { get; set; } = ColorTheme.Current.SceneView.RootWidgetOverlayColor;
	}
}