using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;

namespace Calamansi
{
	public class Calamansi
	{
		public static void UpdateMainCharset(CalamansiConfig config, string assetDirectory)
		{
			var dictPath = AssetPath.Combine(assetDirectory,
				AssetPath.Combine(Localization.DictionariesPath, $"Dictionary.{config.Localization ?? ""}.txt"));
			if (config.Localization == null || !File.Exists(dictPath)) {
				return;
			}
			var characters = new HashSet<char>();
			var dict = new LocalizationDictionary();
			using (var stream = File.Open(dictPath, FileMode.Open)) {
				dict.ReadFromStream(stream);
			}
			foreach (var (_, value) in dict) {
				if (value.Text == null) {
					continue;
				}
				foreach (var c in value.Text) {
					if (c != 10) {
						characters.Add(c);
					}
				}
			}
			config.Main.Charset = string.Join("", characters.OrderBy(c => c));
		}
	}
}
