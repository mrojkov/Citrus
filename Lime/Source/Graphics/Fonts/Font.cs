using System;
using System.Collections;
using System.Collections.Generic;

using Yuzu;

namespace Lime
{
	public interface IFont : IDisposable
	{
		string About { get; }
		IFontCharSource Chars { get; }
		void ClearCache();
		bool RoundCoordinates { get; }
	}

	public interface IFontCharSource : IDisposable
	{
		FontChar Get(char code, float heightHint);
		bool Contains(char code);
	}

	public class Font : IFont
	{
		[YuzuMember]
		public string About { get; set; }
		[YuzuMember]
		// It is better to move it to FontCharCollection, but leave it here for compatibility reasons.
		public List<ITexture> Textures { get { return CharCollection.Textures; } }
		[YuzuMember]
		public FontCharCollection CharCollection { get; private set; }
		public IFontCharSource Chars { get { return CharCollection; } }
		[YuzuMember]
		public bool RoundCoordinates { get; set; } = false;

		public Font()
		{
			CharCollection = new FontCharCollection();
		}

		public void Dispose()
		{
			foreach (var texture in Textures) {
				texture.Dispose();
			}
		}

		public void ClearCache() { }
	}

	public static class FontExtensions
	{
		public static Vector2 MeasureTextLine(this IFont font, string text, float fontHeight, float letterSpacing)
		{
			return MeasureTextLine(font, text, fontHeight, 0, text.Length, letterSpacing);
		}

		public static Vector2 MeasureTextLine(this IFont font, string text, float fontHeight, int start, int length, float letterSpacing, bool skipFirstLetter = true)
		{
			FontChar prevChar = null;
			var size = new Vector2(0, fontHeight);
			float width = 0;
			// float scale = fontHeight / font.CharHeight;
			for (int i = 0; i < length; i++) {
				char ch = text[i + start];
				if (ch == '\n') {
					size.Y += fontHeight;
					width = 0;
					prevChar = null;
					continue;
				}
				var fontChar = font.Chars.Get(ch, fontHeight);
				if (fontChar == FontChar.Null) {
					continue;
				}
				float scale = fontChar.Height != 0.0f ? fontHeight / fontChar.Height : 0.0f;
				width += scale * (fontChar.ACWidths.X + fontChar.Kerning(prevChar));
				width += scale * (fontChar.Width + fontChar.ACWidths.Y);
				if (!skipFirstLetter || i != 0) {
					width += scale * letterSpacing;
				}
				size.X = width;
				prevChar = fontChar;
			}
			return size;
		}
	}

	public class FontCharCollection : IFontCharSource, ICollection<FontChar>
	{
		private readonly CharMap charMap = new CharMap();
		private readonly List<FontChar> charList = new List<FontChar>();

		public readonly List<ITexture> Textures = new List<ITexture>();

		int ICollection<FontChar>.Count
		{
			get
			{
				return charList.Count;
			}
		}

		bool ICollection<FontChar>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public bool Contains(char code)
		{
			return charMap.Contains(code);
		}

		public FontChar Get(char code, float heightHint)
		{
			var c = charMap[code];
			if (c != null) {
				if (c.Texture == null) {
					c.Texture = Textures[c.TextureIndex];
				}
				return c;
			}
			return CharMap.TranslateKnownMissingChars(ref code) ? Get(code, heightHint) : FontChar.Null;
		}

		public void Dispose()
		{
		}

		void ICollection<FontChar>.Add(FontChar item)
		{
			charMap[item.Char] = item;
			charList.Add(item);
		}

		void ICollection<FontChar>.Clear()
		{
			charList.Clear();
			charMap.Clear();
		}

		bool ICollection<FontChar>.Contains(FontChar item)
		{
			return Contains(item.Char);
		}

		void ICollection<FontChar>.CopyTo(FontChar[] array, int arrayIndex)
		{
			charList.CopyTo(array, arrayIndex);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return charList.GetEnumerator();
		}

		IEnumerator<FontChar> IEnumerable<FontChar>.GetEnumerator()
		{
			return charList.GetEnumerator();
		}

		bool ICollection<FontChar>.Remove(FontChar item)
		{
			charMap[item.Char] = null;
			return charList.Remove(item);
		}
	}

	public struct KerningPair
	{
		[YuzuMember]
		public char Char;
		[YuzuMember]
		public float Kerning;
	}

	/// <summary>
	/// Class representing the single font character and it's geometry
	/// </summary>
	public class FontChar
	{
		[YuzuMember]
		public char Char;
		/// <summary>
		/// The left-top glyph's position in the font texture
		/// </summary>
		[YuzuMember]
		public Vector2 UV0;
		/// <summary>
		/// The right-bottom glyph's position in the font texture
		/// </summary>
		[YuzuMember]
		public Vector2 UV1;
		/// <summary>
		/// Width of the glyph in pixels
		/// </summary>
		[YuzuMember]
		public float Width;
		/// <summary>
		/// Height of the glyph in pixels
		/// </summary>
		[YuzuMember]
		public float Height;
		/// <summary>
		/// TrueType channels intensity ot grayscale
		/// </summary>
		[YuzuMember]
		public bool RgbIntensity = false;
		/// <summary>
		/// Contains the A and C spacing of the character.
		/// The A spacing is the distance to add to the current position before drawing the character glyph.
		/// The C spacing is the distance to add to the current position to provide white space to the right of the character glyph.
		/// </summary>
		[YuzuMember]
		public Vector2 ACWidths;
		/// <summary>
		/// List of kerning pairs, related to this character
		/// </summary>
		[YuzuMember]
		public List<KerningPair> KerningPairs;
		/// <summary>
		/// Font texture's index which contains the given glyph
		/// </summary>
		[YuzuMember]
		public int TextureIndex;
		/// <summary>
		/// Mostly stores only negative offset which is useful for chars with diacritics.
		/// </summary>
		[YuzuMember]
		public int VerticalOffset;
		/// <summary>
		/// The null-character which denotes any missing character in a font
		/// </summary>
		public static FontChar Null = new FontChar();

		/// <summary>
		/// Cached texture reference.
		/// </summary>
		public ITexture Texture;

		public float Kerning(FontChar prevChar)
		{
			if (prevChar != null && prevChar.KerningPairs != null)
				foreach (var pair in prevChar.KerningPairs) {
					if (pair.Char == Char) {
						return pair.Kerning;
					}
				}
			return 0;
		}
	}
}
