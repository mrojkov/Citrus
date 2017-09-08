using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.UI
{
	public static class Cursors
	{

		private static Bitmap GetBitmapFromEmbededResource(string id)
		{
			var path = $"Tangerine.Resources.{id}.png";
			var png = new EmbeddedResource(path, "Tangerine").GetResourceStream();
			if (png == null) {
				throw new ArgumentException($"Icon '{path}' doesn't exist");
			}
			return new Bitmap(png);
		}

		private static MouseCursor CreateCursorFromEmbededResource(string name, IntVector2 hotSpot)
		{
			var icon = GetBitmapFromEmbededResource(name);
			return new MouseCursor(icon, hotSpot);
		}

		public static MouseCursor Rotate { get; } = CreateCursorFromEmbededResource("Cursors.Rotate", new IntVector2(8, 8));
		public static MouseCursor Pipette { get; } = CreateCursorFromEmbededResource("Icons.Tools.Pipette", new IntVector2(0, 23));
	}
}
