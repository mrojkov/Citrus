using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using SharpFont;
using SharpFont.HarfBuzz;

namespace Lime
{
	public static class FontGenerator
	{
		/// <summary>
		/// Extracts characters from specified dictionaries for each element of CharSets
		/// and generates Tangerine Font.
		/// </summary>
		/// <param name="configPath"> Path to configuration file relative to <paramref name="assetDirectory"/>. </param>
		/// <param name="assetDirectory"> Path to asset directory. </param>
		public static void UpdateCharSetsAndGenerateFont(string configPath, string assetDirectory)
		{
			var config = InternalPersistence.Instance.ReadObjectFromFile<TftConfig>(AssetPath.Combine(assetDirectory, configPath));
			UpdateCharsets(config, assetDirectory);
			InternalPersistence.Instance.WriteObjectToFile(AssetPath.Combine(assetDirectory, configPath), config, Persistence.Format.Json);
			GenerateFont(config, assetDirectory, Path.ChangeExtension(configPath, null));
		}

		/// <summary>
		/// Generates Tangerine Font.
		/// </summary>
		/// <param name="configPath"> Path to configuration file relative to <paramref name="assetDirectory"/>. </param>
		/// <param name="assetDirectory"> Path to asset directory. </param>
		public static void GenerateFont(string configPath, string assetDirectory)
		{
			var config = InternalPersistence.Instance.ReadObjectFromFile<TftConfig>(AssetPath.Combine(assetDirectory, configPath));
			GenerateFont(config, assetDirectory, Path.ChangeExtension(configPath, null));
		}

		/// <summary>
		/// Generates Tangerine Font.
		/// </summary>
		/// <param name="config"> Tangerine Font Config. </param>
		/// <param name="assetDirectory"> Path to asset directory. </param>
		/// <param name="outputPath"> Path for Tangerine Font and it's textures </param>
		public static void GenerateFont(TftConfig config, string assetDirectory, string outputPath)
		{
			var fontCharCollection = new FontCharCollection();
			var chars = new CharCache(config.Height, null, fontCharCollection.Textures) {
				Padding = config.Padding,
				MinTextureSize = config.TextureSize,
				MaxTextureSize = config.TextureSize,
			};
			var missingCharacters = new List<char>();
			foreach (var charSet in config.CharSets) {
				var fontPath = AssetPath.Combine(assetDirectory, charSet.Font);
				if (!File.Exists(fontPath)) {
					Console.WriteLine($"Missing font: {fontPath}\n Please ensure font existence!!!");
					return;
				}
				var fontData = File.ReadAllBytes(fontPath);
				chars.FontRenderer = new FontRenderer(fontData) { LcdSupported = false };
				missingCharacters.Clear();
				foreach (var c in charSet.Chars) {
					if (config.ExcludeChars.Any(character => character == c)) {
						continue;
					}
					var fontChar = chars.Get(c);
					if (fontChar == FontChar.Null) {
						missingCharacters.Add(c);
						continue;
					}
					if (config.IsSdf) {
						fontChar.ACWidths *= config.SdfScale;
						fontChar.Height *= config.SdfScale;
						fontChar.Width *= config.SdfScale;
						fontChar.Padding *= config.SdfScale;
						fontChar.VerticalOffset *= config.SdfScale;
						if (fontChar.KerningPairs != null) {
							for (int i = 0; i < fontChar.KerningPairs.Count; i++) {
								var pair = fontChar.KerningPairs[i];
								pair.Kerning *= config.SdfScale;
								fontChar.KerningPairs[i] = pair;
							}
						}
					}
					fontCharCollection.Add(fontChar);
				}
				if (missingCharacters.Count > 0) {
					Console.WriteLine($"Characters: {string.Join("", missingCharacters)} -- are missing in font {charSet.Font}");
				}
				GenerateKerningPairs(fontCharCollection, chars.FontRenderer.Face, config);
			}
			if (config.IsSdf) {
				foreach (var texture in fontCharCollection.Textures) {
					SdfConverter.ConvertToSdf(texture.GetPixels(), texture.ImageSize.Width, texture.ImageSize.Height, config.Padding / 2);
				}
			}
			using (var font = new Font(fontCharCollection)) {
				SaveAsTft(font, config, assetDirectory, outputPath);
			}
		}

		/// <summary>
		/// Generates kerning pairs using HarfBuzz.
		/// </summary>
		private static void GenerateKerningPairs(FontCharCollection fontChars, Face face, TftConfig config)
		{
			var font = SharpFont.HarfBuzz.Font.FromFTFace(face);
			foreach (var fontChar in fontChars) {
				var lhs = fontChar.Char;
				var kerningCharset = FontRenderer.KerningPairCharsets.FirstOrDefault(c => c.Contains(lhs));
				if (kerningCharset == null) {
					continue;
				}
				config.CustomKerningPairs.TryGetValue(lhs, out var customKernings);
				var leftGlyphId = face.GetCharIndex(lhs);
				foreach (var rhs in kerningCharset) {
					if (customKernings != null && TryGetKerning(rhs, out var kerningPair)) {
						fontChar.AddOrReplaceKerningPair(kerningPair.Char, kerningPair.Kerning);
						continue;
					}
					if (!fontChars.Contains(rhs) || fontChar.ContainsKerningFor(rhs)) {
						continue;
					}
					var rightGlyphId = face.GetCharIndex(rhs);
					// 0 is undefined character
					if (rightGlyphId == 0) {
						continue;
					}
					var kerningAmount = font.GetHorizontalKerning(leftGlyphId, rightGlyphId) / 64f;
					kerningAmount = config.IsSdf ? kerningAmount * config.SdfScale : kerningAmount;
					if (kerningAmount != 0) {
						fontChar.KerningPairs = fontChar.KerningPairs ?? new List<KerningPair>();
						fontChar.KerningPairs.Add(new KerningPair { Char = rhs, Kerning = kerningAmount });
					}
				}
				bool TryGetKerning(char c, out KerningPair kerningPair)
				{
					foreach (var customKerning in customKernings) {
						if (customKerning.Char == c) {
							kerningPair = customKerning;
							return true;
						}
					}
					kerningPair = default;
					return false;
				}
			}
		}

		private static bool ContainsKerningFor(this FontChar fontChar, char @char)
		{
			if (fontChar.KerningPairs != null) {
				foreach (var pair in fontChar.KerningPairs) {
					if (pair.Char == @char) {
						return true;
					}
				}
			}
			return false;
		}

		private static void AddOrReplaceKerningPair(this FontChar fontChar, char @char, float kerning)
		{
			var pairs = fontChar.KerningPairs = fontChar.KerningPairs ?? new List<KerningPair>();
			for (int i = 0; i < pairs.Count; i++) {
				var pair = pairs[i];
				if (pair.Char == @char) {
					pair.Kerning = kerning;
					pairs[i] = pair;
					return;
				}
			}
			pairs.Add(new KerningPair { Char = @char, Kerning = kerning });
		}

		public static void UpdateCharsets(TftConfig config, string assetDirectory)
		{
			foreach (var charSet in config.CharSets) {
				UpdateCharset(charSet, assetDirectory, sortByFrequency:true);
			}
		}

		public static void UpdateCharset(TftConfig.CharSet charSet, string assetDirectory, bool sortByFrequency = false)
		{
			if (string.IsNullOrEmpty(charSet.ExtractFromDictionaries)) {
				return;
			}
			var characters = new HashSet<char>();
			var frequency = new Dictionary<char, int>();
			var dict = new LocalizationDictionary();
			foreach (var localization in charSet.ExtractFromDictionaries.Split(',')) {
				// cause EN is default dictionary
				var loc = localization == "EN" ? string.Empty : localization;
				var dictPath = AssetPath.Combine(assetDirectory, Localization.DictionariesPath,
					$"Dictionary.{loc}.txt".Replace("..", "."));
				if (!File.Exists(dictPath)) {
					Console.WriteLine($"Dictionary of {localization} localization is missing!: {dictPath}");
					continue;
				}
				using (var stream = File.Open(dictPath, FileMode.Open)) {
					dict.ReadFromStream(stream);
				}
				ExtractCharacters(dict, characters, frequency);
			}
			charSet.Chars = string.Join("", sortByFrequency ?
				characters.OrderByDescending(c => frequency[c]) : characters.OrderBy(c => c));
		}

		private static void ExtractCharacters(LocalizationDictionary dictionary, HashSet<char> chars,
			Dictionary<char, int> frequency)
		{
			foreach (var (_, value) in dictionary) {
				if (value.Text == null) {
					continue;
				}
				foreach (var c in value.Text) {
					if (c != '\n' && !char.IsSurrogate(c)) {
						chars.Add(c);
						frequency[c] = frequency.TryGetValue(c, out var v) ? v + 1 : 1;
					}
				}
			}
		}

		public static void SaveAsTft(Font font, TftConfig config, string assetDirectory, string path)
		{
			var basePath = Path.ChangeExtension(path, null);
			var absolutePath = AssetPath.Combine(assetDirectory, path);
			foreach (var file in Directory.EnumerateFiles(Path.GetDirectoryName(absolutePath), $"{Path.GetFileName(path)}??.png")) {
				File.Delete(file);
			}
			for (int i = 0; i < font.Textures.Count; i++) {
				var texture = font.Textures[i];
				var pixels = texture.GetPixels();
				var w = texture.ImageSize.Width;
				var h = texture.ImageSize.Height;
				if (config.IsSdf) {
					var sourceBitmap = new Bitmap(pixels, w, h);
					var bitmap = sourceBitmap.Rescale((int)(w * config.SdfScale), (int)(h * config.SdfScale));
					sourceBitmap.Dispose();
					bitmap.SaveTo(AssetPath.Combine(assetDirectory, basePath + (i > 0 ? $"{i:00}.png" : ".png")));
					bitmap.Dispose();
				} else {
					using (var bm = new Bitmap(pixels, w, h)) {
						bm.SaveTo(AssetPath.Combine(assetDirectory, basePath + (i > 0 ? $"{i:00}.png" : ".png")));
					}
				}
				font.Textures[i] = new SerializableTexture(basePath + (i > 0 ? $"{i:00}" : ""));
			}
			InternalPersistence.Instance.WriteObjectToFile(Path.ChangeExtension(absolutePath, "tft"), font, Persistence.Format.Json);
		}
	}
}
