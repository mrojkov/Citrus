using System.Collections.Generic;
using System.IO;
using Lime;

namespace Calamansi
{
	class CalamansiFontCharCollection : FontCharCollection
	{
		public CalamansiFontCharCollection(CalamansiConfig config, string assetDirectory)
		{
			var chars = new CalamansiCharCache(config.Height, null, Textures);
			foreach (var option in config.Options) {
				var fontData = File.ReadAllBytes(AssetPath.Combine(assetDirectory, option.Font));
				var fontRenderer = new FontRenderer(fontData) { LcdSupported = false };
				chars.FontRenderer = fontRenderer;
				chars.HPadding = chars.VPadding = config.Padding;
				foreach (var c in option.Charset) {
					var fontChar = chars.Get(c);
					if (config.SDF) {
						fontChar.ACWidths *= config.SDFScale;
						fontChar.Height *= config.SDFScale;
						fontChar.Width *= config.SDFScale;
					}
					((ICollection<FontChar>) this).Add(fontChar);
				}
			}
		}
	}
}
