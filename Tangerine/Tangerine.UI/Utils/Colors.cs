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
		public static readonly ColorGradient Toolbar = new ColorGradient(Color4.White, Color4.White.Darken(0.2f));
		public static class Timeline
		{
			public static class Grid
			{
				public static readonly Color4 PropertyRowBackground = GrayBackground.Lighten(0.5f);
				public static readonly Color4 Lines = Color4.White.Darken(0.25f);
				public static readonly Color4 Selection = Color4.Gray.Transparentify(0.5f);
				public static readonly Color4 Cursor = Color4.Red.Lighten(0.4f);
			}
			public static class Ruler
			{
				public static readonly Color4 Notchings = Grid.Lines;
				public static readonly Color4 JumpMarker = Color4.Yellow;
				public static readonly Color4 PlayMarker = Color4.Green;
				public static readonly Color4 StopMarker = Color4.Red;
				public static readonly Color4 UnknownMarker = Color4.Black;
				public static readonly Color4 Cursor = Grid.Cursor;
			}
			public static class Overview
			{
				public static readonly Color4 Veil = Color4.White.Darken(0.3f).Transparentify(0.3f);
				public static readonly Color4 Border = Color4.White.Darken(0.4f);
			}
			public static class Roll
			{
				public static readonly Color4 Lines = Grid.Lines;
			}
		}
		public static class Docking
		{
			public static readonly Color4 DragRectagleOutline = Color4.FromFloats(0.2f, 0.2f, 1f);
			public static readonly Color4 PanelBorder = Color4.White.Darken(0.75f);
		}
		public static class Inspector
		{
			public static readonly Color4 BorderAroundKeyframeColorbox = Color4.Black;
			public static readonly Color4 CategoryLabelBackground = Color4.White.Darken(0.2f);
		}
	}
}

