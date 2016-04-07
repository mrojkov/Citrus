using System;
using Lime;

namespace Tangerine.UI
{
	public static class Colors
	{
		public static readonly Color4 WhiteBackground = DesktopTheme.Colors.WhiteBackground;
		public static readonly Color4 GrayBackground = DesktopTheme.Colors.GrayBackground;
		public static readonly Color4 SelectedBackground = DesktopTheme.Colors.SelectedBackground;
		public static readonly Color4 DragCursor = Color4.FromFloats(0, 0, 0);
		public static readonly Color4 GridPropertyRowBackground = Color4.Lerp(0.5f, GrayBackground, WhiteBackground);
		public static readonly Color4 GridLines = Color4.FromFloats(0.8f, 0.8f, 0.8f);
		public static readonly Color4 GridSelection = Color4.FromFloats(0.5f, 0.5f, 0.5f, 0.5f);
		public static readonly Color4 OverviewVeil = Color4.FromFloats(0.5f, 0.5f, 0.5f, 0.3f);
		public static readonly ColorGradient Toolbar = new ColorGradient(new Color4(255, 255, 255), new Color4(230, 230, 230));
	}
}

