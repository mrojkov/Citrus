using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI
{
	public static class IconPool
	{
		private static readonly Dictionary<string, ITexture> icons = new Dictionary<string, ITexture>();

		public static ITexture GetTexture(string id, string defaultId = null)
		{
			ITexture icon;
			if (!icons.TryGetValue(id, out icon)) {
				icons[id] = icon = CreateTexture(id, defaultId);
			}
			return icon;
		}

		public static Image CreateIcon(string id, string defaultId = null)
		{
			var icon = new Image(GetTexture(id, defaultId));
			icon.MinMaxSize = (Vector2)icon.Texture.ImageSize;
			return icon;
		}

		private static ITexture CreateTexture(string id, string defaultId = null)
		{
			var path = Theme.Textures.NodeIconPath(id);
			var png = new EmbeddedResource(path, "Tangerine").GetResourceStream();
			if (png == null) {
				if (defaultId != null) {
					return CreateTexture(defaultId);
				}
				throw new ArgumentException($"Icon '{path}' doesn't exist");
			}
			using (var bmp = new Bitmap(png)) {
				var texture = new Texture2D();
				texture.LoadImage(bmp);
				return texture;
			}
		}
	}
}

