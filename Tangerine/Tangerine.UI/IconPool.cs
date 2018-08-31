using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI
{
	public static class IconPool
	{
		private static readonly Dictionary<string, Icon> icons = new Dictionary<string, Icon>();

		public static Icon GetIcon(string id, string defaultId = null)
		{
			if (!icons.TryGetValue(id, out var icon)) {
				icons[id] = icon = CreateIcon(id, defaultId);
			}
			return icon;
		}

		public static ITexture GetTexture(string id, string defaultId = null) => GetIcon(id, defaultId).AsTexture;

		private static Icon CreateIcon(string id, string defaultId = null)
		{
			while (true) {
				var png = new ThemedIconResource(id, "Tangerine").GetResourceStream();
				if (png != null) {
					return new Icon(new Bitmap(png));
				}
				id = defaultId ?? throw new ArgumentException($"Icon '{id}' doesn't exist");
				defaultId = null;
			}
		}
	}
}

