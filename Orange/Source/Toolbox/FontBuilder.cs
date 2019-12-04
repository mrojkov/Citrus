#if false
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime;
using SharpFont;

namespace Orange
{
	public class FontBuilder
	{
		public readonly string FontPath;
		public int TextureSize { get; } = 1024;
		public int Height { get; private set; }
		public readonly string Characters;

		public FontBuilder(string fontPath, int height, string characters = null)
		{
			FontPath = fontPath;
			Height = height;
			Characters = characters ??
				"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!\"" +
				"#$%&ˆ‘’“”´˜`'„()–—*+-÷~,.…/:;<>=?@[]\\^_¯{}|€‚ƒ‹›•§«»©®™¨°¿" +
				"ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿšœžŸŒŽŠ¡¢£¤¥µ¶ " +
				"АБВГДЕЁЖЗИЙКЛМНОПРСТУФХШЩЧЦЪЬЫЭЮЯабвгдеёжзийклмнопрстуфхшщчцъьыэюя";
		}

		public Font Build()
		{
			var font = new Font();
			using (var library = new Library()) {
				var fontData = new EmbeddedResource("Orange.Source.UI.NewUI.Fonts." + FontPath, "Orange").GetResourceBytes();
				var face = library.NewMemoryFace(fontData, 0);
				var pixelSize = (uint)CalcPixelSize(face, Height).Round();
				face.SetPixelSizes(pixelSize, pixelSize);
				RenderFont(font, face);
			}
			return font;
		}

		private static float CalcPixelSize(Face face, int height)
		{
			// See http://www.freetype.org/freetype2/docs/tutorial/step2.html
			// Chapter: Scaling Distances to Device Space
			// BBox suits better than Height (baseline-to-baseline distance), because it can enclose all the glyphs in the font face.
			var designHeight = (float)face.BBox.Top - (float)face.BBox.Bottom;
			var scale = height / designHeight;
			var pixelSize = scale * face.UnitsPerEM;
			return pixelSize;
		}

		private void RenderFont(Font font, Face face)
		{
			var pixels = new Color4[TextureSize * TextureSize];
			int x = 0;
			int y = 0;
			foreach (var @char in Characters) {
				uint glyphIndex = face.GetCharIndex(@char);
				face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
				if (face.Glyph.Metrics.Width == 0) {
					continue;
				}
				face.Glyph.RenderGlyph(RenderMode.Normal);
				var bitmap = face.Glyph.Bitmap;
				if (x + bitmap.Width + 1 >= TextureSize) {
					x = 0;
					y += Height + 1;
				}
				if (y + Height + 1 >= TextureSize) {
					AddFontTexture(font, pixels);
					pixels = new Color4[TextureSize * TextureSize];
					x = y = 0;
				}
				var verticalOffset = Height - face.Glyph.BitmapTop + face.Size.Metrics.Descender.Round();
				CopyBitmap(bitmap, pixels, x, y + verticalOffset);
				var uv0 = new Vector2(x, y) / TextureSize;
				var uv1 = new Vector2(x + bitmap.Width, y + Height) / TextureSize;
				var bearingX = (float)face.Glyph.Metrics.HorizontalBearingX;
				var fontChar = new FontChar() {
					Char = @char,
					Width = bitmap.Width,
					Height = Height,
					TextureIndex = font.Textures.Count,
					ACWidths = new Vector2(
						bearingX.Round(),
						((float)face.Glyph.Metrics.HorizontalAdvance - (float)face.Glyph.Metrics.Width - bearingX).Round()
					),
					UV0 = uv0,
					UV1 = uv1,
				};
				x += bitmap.Width + 1;
				// Iterate through kerning pairs
				foreach (var prevChar in Characters) {
					uint prevGlyphIndex = face.GetCharIndex(prevChar);
					var kerning = (float)face.GetKerning(prevGlyphIndex, glyphIndex, KerningMode.Default).X;
					// Round kerning to prevent blurring
					kerning = kerning.Round();
					if (kerning != 0) {
						if (fontChar.KerningPairs == null) {
							fontChar.KerningPairs = new List<KerningPair>();
						}
						fontChar.KerningPairs.Add(new KerningPair() { Char = prevChar, Kerning = kerning });
					}
				}
				font.CharSource.Add(fontChar);
			}
			AddFontTexture(font, pixels);
			// Add the whitespace character
			font.CharSource.Add(new FontChar() {
				Char = ' ',
				Width = ((float)Height / 5).Round(),
				Height = Height,
			});
		}

		private void AddFontTexture(Font font, Color4[] pixels)
		{
			var texture = new Texture2D();
			texture.LoadImage(pixels, TextureSize, TextureSize, generateMips: false);
			font.Textures.Add(texture);
		}

		private void CopyBitmap(FTBitmap bitmap, Color4[] pixels, int x, int y)
		{
			if (bitmap.PixelMode != PixelMode.Gray) {
				throw new System.Exception("Invalid pixel mode: " + bitmap.PixelMode);
			}
			if (bitmap.Pitch != bitmap.Width) {
				throw new System.Exception("Bitmap pitch doesn't match its width");
			}
			var data = bitmap.BufferData;
			var t = 0;
			for (int i = 0; i < bitmap.Rows; i++) {
				int w = (y + i) * TextureSize + x;
				for (int j = 0; j < bitmap.Width; j++) {
					pixels[w++] = new Color4(255, 255, 255, data[t++]);
				}
			}
		}
	}

}
#endif
