using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;
using SharpFont;
using SharpFont.HarfBuzz;
using Buffer = SharpFont.HarfBuzz.Buffer;

namespace Calamansi
{
	public class Calamansi
	{
		private static List<HashSet<char>> kerningsCharsets = new List<HashSet<char>> {
			new HashSet<char>("0123456789"),
			new HashSet<char>("ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅĀÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝ"),
			new HashSet<char>("abcdefghijklmnopqrstuvwxyzàáâãäåāæçèéêëìíîïðñòóôõöøùúûüýþÿšœž,.¿!?¡"),
			new HashSet<char>("АБВГДЕЁЖЗИЙКЛМНОПРСТУФХШЩЧЦЪЬЫЭЮЯ"),
			new HashSet<char>("абвгдеёжзийклмнопрстуфхшщчцъьыэюя,.!?"),
		};

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

		private static float CalcPixelSize(int height, Face face)
		{
			var designHeight = (float)(face.BBox.Top - face.BBox.Bottom);
			var scale = height / designHeight;
			var pixelSize = scale * face.UnitsPerEM;
			return pixelSize;
		}

		public static void GenerateKerningPairs(CalamansiFontCharCollection fnt, FontRenderer renderer,
			CalamansiConfig config, IEnumerable<char> charset)
		{
			var face = renderer.Face;
			var font = SharpFont.HarfBuzz.Font.FromFTFace(face);
			var height = (int)(config.SDF ? config.Height * config.SDFScale : config.Height);
			var pixelSize = (uint)Math.Round(Math.Abs(CalcPixelSize(height, face)));
			face.SetCharSize(0, height, pixelSize, pixelSize);
			foreach (var lhs in charset) {
				var kerningCharset = kerningsCharsets.FirstOrDefault(c => c.Contains(lhs));
				if (kerningCharset == null) {
					continue;
				}
				var fontChar = fnt.Get(lhs, 0f);
				foreach (var rhs in kerningCharset) {
					var buf = new Buffer {
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
					var simpleShift = (int) (face.Glyph.Metrics.HorizontalBearingX * 2 + face.Glyph.Metrics.Width * 2 -
											face.Glyph.Metrics.HorizontalAdvance);
					var kerningAmount = bufferShift - simpleShift;
					if (kerningAmount != 0) {
						fontChar.KerningPairs = fontChar.KerningPairs ?? new List<KerningPair>();
						fontChar.KerningPairs.Add(new KerningPair { Char = rhs, Kerning = kerningAmount });
					}
				}
			}
		}
	}
}
