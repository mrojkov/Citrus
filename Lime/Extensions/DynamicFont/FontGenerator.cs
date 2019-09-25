using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
				VPadding = config.Padding,
				HPadding = config.Padding,
				MinTextureSize = config.TextureSize,
				MaxTextureSize = config.TextureSize,
			};
			var missingCharacters = new List<char>();
			foreach (var charSet in config.CharSets) {
				var fontData = File.ReadAllBytes(AssetPath.Combine(assetDirectory, charSet.Font));
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
					}
					fontCharCollection.Add(fontChar);
				}
				if (missingCharacters.Count > 0) {
					Console.WriteLine($"Characters: {string.Join("", missingCharacters)} -- are missing in font {charSet.Font}");
				}
				GenerateKerningPairs(fontCharCollection, chars.FontRenderer.Face, config, charSet.Chars);
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
		private static void GenerateKerningPairs(FontCharCollection fontChars, Face face,
			TftConfig config, IEnumerable<char> chars)
		{
			var font = SharpFont.HarfBuzz.Font.FromFTFace(face);
			var height = (int)(config.IsSdf ? config.Height * config.SdfScale : config.Height);
			var pixelSize = (uint)Math.Round(Math.Abs(CalcPixelSize(height, face)));
			face.SetCharSize(0, height, pixelSize, pixelSize);
			foreach (var lhs in chars) {
				var kerningCharset = FontRenderer.KerningPairCharsets.FirstOrDefault(c => c.Contains(lhs));
				if (kerningCharset == null) {
					continue;
				}
				var fontChar = fontChars.Get(lhs, 0f);
				config.CustomKerningPairs.TryGetValue(lhs, out var customKernings);
				foreach (var rhs in kerningCharset) {
					if (customKernings != null && TryGetKerning(rhs, out var kerningPair)) {
						fontChar.KerningPairs = fontChar.KerningPairs ?? new List<KerningPair>();
						fontChar.KerningPairs.Add(kerningPair);
						continue;
					}
					var buf = new SharpFont.HarfBuzz.Buffer {
						Direction = Direction.LeftToRight,
						Script = Script.Latin
					};
					var bufferText = string.Concat(lhs, rhs);
					buf.AddText(bufferText);
					font.Shape(buf);
					var glyphInfos = buf.GlyphInfo();
					var glyphPositions = buf.GlyphPositions();
					if (glyphInfos.Length != 2) {
						continue;
					}
					face.LoadGlyph(glyphInfos[1].codepoint, LoadFlags.Default, LoadTarget.Normal);
					face.Glyph.RenderGlyph(RenderMode.Normal);
					var bufferShift = glyphPositions[1].xAdvance >> 6;
					var simpleShift = (int)(face.Glyph.Metrics.HorizontalBearingX * 2 + face.Glyph.Metrics.Width * 2 -
											face.Glyph.Metrics.HorizontalAdvance);
					var kerningAmount = bufferShift - simpleShift;
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

		private static float CalcPixelSize(int height, Face face)
		{
			var designHeight = (float)(face.BBox.Top - face.BBox.Bottom);
			var scale = height / designHeight;
			var pixelSize = scale * face.UnitsPerEM;
			return pixelSize;
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
				using (var stream = File.Open(dictPath, FileMode.Open)) {
					dict.ReadFromStream(stream);
				}
				ExtractCharacters(dict, characters, frequency);
			}
			charSet.Chars = string.Join("", sortByFrequency ?
				characters.OrderBy(c => frequency[c]) : characters.OrderBy(c => c));
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
