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
		private static HashSet<char> kerningsCharacters = new HashSet<char>(
			"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!\"" +
			"#$%&ˆ‘’“”´˜`'„()–—*+-÷~,.…/:;<>=?@[]\\^_¯{}|€‚ƒ‹›•§«»©®™¨°¿" +
			"ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿšœžŸŒŽŠ¡¢£¤¥µ¶ " +
			"АБВГДЕЁЖЗИЙКЛМНОПРСТУФХШЩЧЦЪЬЫЭЮЯабвгдеёжзийклмнопрстуфхшщчцъьыэюя");

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

		public static void GenerateKerningPairs(CalamansiFontCharCollection fnt, FontRenderer renderer, IEnumerable<char> charset)
		{
			//var pixelSize = (uint) Math.Round(Math.Abs(CalcPixelSize(height)));
			//face.SetCharSize(0, height, pixelSize, pixelSize);

			var face = renderer.Face;
			var font = SharpFont.HarfBuzz.Font.FromFTFace(face);
			foreach (var lhs in charset) {
				if (!kerningsCharacters.Contains(lhs)) {
					continue;
				}
				var fontChar = fnt.Get('a', 0f);
				foreach (var rhs in kerningsCharacters) {
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
						// Impossible to recognize this case
						continue;
					}
					face.LoadGlyph(glyphInfos[1].codepoint, LoadFlags.Default, LoadTarget.Normal);
					face.Glyph.RenderGlyph(RenderMode.Normal);
					var bufferShift = glyphPositions[1].xAdvance >> 6;
					var simpleShift = (int) (face.Glyph.Metrics.HorizontalBearingX * 2 + face.Glyph.Metrics.Width * 2 -
					                         face.Glyph.Metrics.HorizontalAdvance);
					var kerningAmout = bufferShift - simpleShift;
					if (kerningAmout != 0) {
						fontChar.KerningPairs = fontChar.KerningPairs ?? new List<KerningPair>();
						fontChar.KerningPairs.Add(new KerningPair { Char = rhs, Kerning = kerningAmout });
					}
				}
			}
		}
	}
}
