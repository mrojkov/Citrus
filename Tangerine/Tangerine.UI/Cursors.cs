using Lime;
using System;

namespace Tangerine.UI
{
	public static class Cursors
	{
		private static Bitmap GetBitmapFromEmbededResource(string path)
		{
			var png = new EmbeddedResource(path, "Tangerine").GetResourceStream();
			if (png == null) {
				throw new ArgumentException($"Icon '{path}' doesn't exist");
			}
			return new Bitmap(png);
		}

		private static MouseCursor CreateCursorFromEmbededResource(string path, IntVector2 hotSpot)
		{
			var icon = GetBitmapFromEmbededResource(path);
			return new MouseCursor(icon, hotSpot);
		}

		public static MouseCursor Rotate { get; } = CreateCursorFromEmbededResource(Theme.Textures.CursorRotatePath, new IntVector2(8, 8));
		public static MouseCursor Pipette { get; } = CreateCursorFromEmbededResource(Theme.Textures.CursorPipettePath, new IntVector2(0, 23));
		public static MouseCursor DragHandClosed { get; } = CreateCursorFromEmbededResource(Theme.Textures.CursorDragHandClosedPath, new IntVector2(11, 1));
		public static MouseCursor DragHandOpen { get; } = CreateCursorFromEmbededResource(Theme.Textures.CursorDragHandOpenPath, new IntVector2(11, 8));
	}
}
