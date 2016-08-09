using System;
using System.Collections;
using System.Collections.Generic;

using ProtoBuf;
using Yuzu;

namespace Lime
{
	public interface IFont : IDisposable
	{
		string About { get; }
		IFontCharSource Chars { get; }
		void ClearCache();
	}

	public interface IFontCharSource : IDisposable
	{
		FontChar Get(char code, float heightHint);
		bool Contains(char code);
	}

	[ProtoContract]
	public class Font : IFont
	{
		[ProtoMember(1)]
		[YuzuMember]
		public string About { get; set; }
		[ProtoMember(2)]
		[YuzuMember]
		// It is better to move it to FontCharCollection, but leave it here for compatibility reasons.
		public List<ITexture> Textures { get { return CharCollection.Textures; } }
		[ProtoMember(3)]
		[YuzuMember]
		public FontCharCollection CharCollection { get; private set; }
		public IFontCharSource Chars { get { return CharCollection; } }

		public Font()
		{
			CharCollection = new FontCharCollection();
		}

		public void Dispose()
		{
			foreach (var texture in Textures) {
				texture.Discard();
			}
		}

		public void ClearCache() { }
	}

	[ProtoContract]
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

	[ProtoContract]
	public struct KerningPair
	{
		[ProtoMember(1)]
		[YuzuMember]
		public char Char;
		[ProtoMember(2)]
		[YuzuMember]
		public float Kerning;
	}

	/// <summary>
	/// Class representing the single font character and it's geometry
	/// </summary>
	[ProtoContract]
	public class FontChar
	{
		[ProtoMember(1)]
		[YuzuMember]
		public char Char;
		/// <summary>
		/// The left-top glyph's position in the font texture
		/// </summary>
		[ProtoMember(2)]
		[YuzuMember]
		public Vector2 UV0;
		/// <summary>
		/// The right-bottom glyph's position in the font texture
		/// </summary>
		[ProtoMember(3)]
		[YuzuMember]
		public Vector2 UV1;
		/// <summary>
		/// Width of the glyph in pixels
		/// </summary>
		[ProtoMember(4)]
		[YuzuMember]
		public float Width;
		/// <summary>
		/// Height of the glyph in pixels
		/// </summary>
		[ProtoMember(5)]
		[YuzuMember]
		public float Height;
		/// <summary>
		/// Contains the A and C spacing of the character.
		/// The A spacing is the distance to add to the current position before drawing the character glyph.
		/// The C spacing is the distance to add to the current position to provide white space to the right of the character glyph.
		/// </summary>
		[ProtoMember(6)]
		[YuzuMember]
		public Vector2 ACWidths;
		/// <summary>
		/// List of kerning pairs, related to this character
		/// </summary>
		[ProtoMember(7)]
		[YuzuMember]
		public List<KerningPair> KerningPairs;
		/// <summary>
		/// Font texture's index which contains the given glyph
		/// </summary>
		[ProtoMember(8)]
		[YuzuMember]
		public int TextureIndex;
		/// <summary>
		/// Mostly stores only negative offset which is useful for chars with diacritics.
		/// </summary>
		[ProtoMember(9)]
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