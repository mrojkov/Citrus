using Lime;
using System;

namespace Tangerine.UI
{
	public static class Cursors
	{
		private static Bitmap GetBitmapFromEmbededResource(string resourceId)
		{
            var png = new ThemedconResource(resourceId, "Tangerine").GetResourceStream();
			if (png == null) {
				throw new ArgumentException($"Icon '{resourceId}' doesn't exist");
			}
			return new Bitmap(png);
		}

		private static MouseCursor CreateCursorFromEmbededResource(string path, IntVector2 hotSpot)
		{
			var icon = GetBitmapFromEmbededResource(path);
			return new MouseCursor(icon, hotSpot);
		}

		public static MouseCursor Rotate { get; } = CreateCursorFromEmbededResource("Cursors.Rotate", new IntVector2(8, 8));
		public static MouseCursor Pipette { get; } = CreateCursorFromEmbededResource("Tools.Pipette", new IntVector2(0, 23));
		public static MouseCursor DragHandClosed { get; } = CreateCursorFromEmbededResource("Cursors.DragHandClosed", new IntVector2(11, 1));
		public static MouseCursor DragHandOpen { get; } = CreateCursorFromEmbededResource("Cursors.DragHandOpen", new IntVector2(11, 8));
		public static MouseCursor EnabledHelp { get; } = CreateCursorFromEmbededResource("Cursors.EnabledHelp", new IntVector2(0, 0));
		public static MouseCursor DisabledHelp { get; } = CreateCursorFromEmbededResource("Cursors.DisabledHelp", new IntVector2(0, 0));
	}
}
