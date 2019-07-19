using System;
using System.Collections.Generic;
using SharpFont;
using SharpFont.TrueType;

namespace Lime
{
	public class FontRenderer : IDisposable
	{
		public class Glyph
		{
			public byte[] Pixels;
			public bool RgbIntensity;
			public Vector2 ACWidths;
			public int VerticalOffset;
			public int Pitch;
			public int Width;
			public int Height;
			public List<KerningPair> KerningPairs;
		}

		public string KerningCharacters =
			"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!\"" +
			"#$%&ˆ‘’“”´˜`'„()–—*+-÷~,.…/:;<>=?@[]\\^_¯{}|€‚ƒ‹›•§«»©®™¨°¿" +
			"ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿšœžŸŒŽŠ¡¢£¤¥µ¶ " +
			"АБВГДЕЁЖЗИЙКЛМНОПРСТУФХШЩЧЦЪЬЫЭЮЯабвгдеёжзийклмнопрстуфхшщчцъьыэюя";

		private Face face;
		private Library library;
		private int lastHeight;
		public bool LcdSupported { get; set; } = true;
		/// <summary>
		/// Workaround. DynamicFont incorrectly applies fontHeight when rasterizing the font,
		/// so the visual font height for the same fontHeight will be different for different ttf files.
		/// This function returns corrected font height for the specific current ttf file.
		/// The result should be selected manually.
		/// </summary>
		private Func<int, int> fontHeightResolver;

		public FontRenderer(byte[] fontData)
		{
			library = new Library();
			face = library.NewMemoryFace(fontData, 0);
#if SUBPIXEL_TEXT
			LcdSupported = true;
			try {
				// can not use any other filtration to achive a windows like look, 
				// becouse filtering creates wrong spacing between chars, and no visible way to correct it
				library.SetLcdFilter(LcdFilter.None);
			} catch (FreeTypeException) {
				LcdSupported = false;
			}
#else
			LcdSupported = false;
#endif
		}

		private static float CalcPixelSize(Face face, int height)
		{
			// See http://www.freetype.org/freetype2/docs/tutorial/step2.html
			// Chapter: Scaling Distances to Device Space
			// BBox suits better than Height (baseline-to-baseline distance), because it can enclose all the glyphs in the font face.
			var scale = height / (float)face.Height;
			var pixelSize = scale * face.UnitsPerEM;
			return pixelSize;
		}

		/// <summary>
		/// Renders a glyph with a given height, measured as a distance between two text lines in the device pixels.</param>
		/// </summary>
		public Glyph Render(char @char, int height)
		{
			if (lastHeight != height) {
				lastHeight = height;
				var pixelSize = (uint) Math.Abs(
					CalcPixelSize(face, height).Round()
				);
				face.SetPixelSizes(pixelSize, pixelSize);
			}

			var glyphIndex = face.GetCharIndex(@char);
			if (glyphIndex == 0) {
				return null;
			}

			face.LoadGlyph(glyphIndex, LoadFlags.Default, LcdSupported ? LoadTarget.Lcd : LoadTarget.Normal);
			face.Glyph.RenderGlyph(LcdSupported ? RenderMode.Lcd : RenderMode.Normal);
			FTBitmap bitmap = face.Glyph.Bitmap;

			var verticalOffset = height - face.Glyph.BitmapTop + face.Size.Metrics.Descender.Round();
			var bearingX = (float) face.Glyph.Metrics.HorizontalBearingX;
			bool rgbIntensity = bitmap.PixelMode == PixelMode.Lcd || bitmap.PixelMode == PixelMode.VerticalLcd;
			var glyph = new Glyph {
				Pixels = bitmap.BufferData,
				RgbIntensity = rgbIntensity,
				Pitch = bitmap.Pitch,
				Width = rgbIntensity ? bitmap.Width / 3 : bitmap.Width,
				Height = bitmap.Rows,
				VerticalOffset = verticalOffset,
				ACWidths = new Vector2(
					bearingX,
					(float) face.Glyph.Metrics.HorizontalAdvance - (float) face.Glyph.Metrics.Width - bearingX
				),
			};
			// Iterate through kerning pairs
			foreach (var nextChar in KerningCharacters) {
				var nextGlyphIndex = face.GetCharIndex(nextChar);
				var kerning = (float)face.GetKerning(glyphIndex, nextGlyphIndex, KerningMode.Default).X;
				if (kerning != 0) {
					if (glyph.KerningPairs == null) {
						glyph.KerningPairs = new List<KerningPair>();
					}
					glyph.KerningPairs.Add(new KerningPair { Char = nextChar, Kerning = kerning });
				}
			}
			return glyph;
		}

		public bool ContainsGlyph(char code)
		{
			return face.GetCharIndex(code) != 0;
		}

		public void Dispose()
		{
			if (library != null) {
				library.Dispose();
			}
			// Do not dispose the face, because library.Dispose() made it on its own.
			GC.SuppressFinalize(this);
		}
	}
}
