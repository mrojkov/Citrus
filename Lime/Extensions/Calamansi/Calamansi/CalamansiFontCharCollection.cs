using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;

namespace Calamansi
{
	public class CalamansiFontCharCollection : FontCharCollection
	{
		public CalamansiFontCharCollection(CalamansiConfig config, string assetDirectory)
		{
			var chars = new CalamansiCharCache(config.Height, null, Textures);
			chars.HPadding = chars.VPadding = config.Padding;
			ProcessOption(config, config.Main);
			foreach (var option in config.AlwaysInclude) {
				ProcessOption(config, option);
			}
			void ProcessOption(CalamansiConfig cfg, CalamansiConfig.CalamansiOption option)
			{
				var fontData = File.ReadAllBytes(AssetPath.Combine(assetDirectory, option.Font ?? cfg.Main.Font));
				var fontRenderer = new FontRenderer(fontData) { LcdSupported = false };
				chars.FontRenderer = fontRenderer;
				var exclude = cfg.AlwaysExclude.FirstOrDefault(o => o.Font == option.Font || option == config.Main && o.Font == null);
				foreach (var c in option.Charset) {
					if (exclude != null && exclude.Charset.Contains(c)) {
						continue;
					}
					var fontChar = chars.Get(c);
					if (cfg.SDF) {
						fontChar.ACWidths *= cfg.SDFScale;
						fontChar.Height *= cfg.SDFScale;
						fontChar.Width *= cfg.SDFScale;
					}
					((ICollection<FontChar>)this).Add(fontChar);
				}
				Calamansi.GenerateKerningPairs(this, fontRenderer, option.Charset);
			}

		}
	}
}
