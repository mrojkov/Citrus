using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class Font
	{
		[ProtoMember(1)]
		public string About;
		[ProtoMember(2)]
		public List<ITexture> Textures = new List<ITexture>();
		[ProtoMember(3)]
		public readonly FontCharCollection Chars = new FontCharCollection();
	}

	[ProtoContract]
	public class FontCharCollection : ICollection<FontChar>
	{
		public List<FontChar> CharList = new List<FontChar>();
		public FontChar[][] CharMap = new FontChar[256][];

		public FontChar this[char code] { 
			get { 
				byte hb = (byte)(code >> 8);
				byte lb = (byte)(code & 255);
				if (CharMap[hb] != null) {
					var c = CharMap[hb][lb];
					if (c != null)
						return c;
				}
				// Can use normal space instead of unbreakable space
				if (code == 160) {
					return this[' '];
				}
				// Can use 'middle dot' instead of 'bullet operator'
				if (code == 8729) {
					return this[(char)183];
				}
				// Can use 'degree symbol' instead of 'masculine ordinal indicator'
				if (code == 186) {
					return this[(char)176];
				}
				// Use '#' instead of 'numero sign'
				if (code == 8470) {
					return this['#'];
				}
				return FontChar.Null;
			}
		}

		public void CopyTo(Array a, int index)
		{
		}

		public int Count { get { return CharList.Count; } }

		IEnumerator<FontChar> IEnumerable<FontChar>.GetEnumerator()
		{
			return CharList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return CharList.GetEnumerator();
		}

		void ICollection<FontChar>.CopyTo(FontChar[] a, int index)
		{
			CharList.CopyTo(a, index);
		}
		
		public void Clear()
		{
			CharList.Clear();
			for (int i = 0; i < 256; i++) {
				CharMap[i] = null;
			}
		}
		
		public bool Contains(FontChar item)
		{
			byte hb = (byte)(item.Char >> 8);
			byte lb = (byte)(item.Char & 255);
			if (CharMap[hb] != null) {
				return CharMap[hb][lb] != null;
			}
			return false;
		}
		
		public bool Remove(FontChar item)
		{
			byte hb = (byte)(item.Char >> 8);
			byte lb = (byte)(item.Char & 255);
			if (CharMap[hb] != null) {
				CharMap[hb][lb] = null;
			}
			return CharList.Remove(item);
		}
		
		bool ICollection<FontChar>.IsReadOnly { 
			get { return false; }
		}

		public void Add(FontChar c)
		{
			byte hb = (byte)(c.Char >> 8);
			byte lb = (byte)(c.Char & 255);
			if (CharMap[hb] == null) {
				CharMap[hb] = new FontChar[256];
			}
			CharMap[hb][lb] = c;
			CharList.Add(c);
		}
	}

	[ProtoContract]
	public struct KerningPair
	{
		[ProtoMember(1)]
		public char Char;
		[ProtoMember(2)]
		public float Kerning;
	}

	/// <summary>
	/// Class representing the single font character and it's geometry
	/// </summary>
	[ProtoContract]
	public class FontChar
	{
		[ProtoMember(1)]
		public char Char;
		/// <summary>
		/// The left-top glyph's position in the font texture
		/// </summary>
		[ProtoMember(2)]
		public Vector2 UV0;
		/// <summary>
		/// The right-bottom glyph's position in the font texture
		/// </summary>
		[ProtoMember(3)]
		public Vector2 UV1;
		/// <summary>
		/// Width of the glyph in pixels
		/// </summary>
		[ProtoMember(4)]
		public float Width;
		/// <summary>
		/// Height of the glyph in pixels
		/// </summary>
		[ProtoMember(5)]
		public float Height;
		/// <summary>
		/// Contains the A and C spacing of the character.
		/// The A spacing is the distance to add to the current position before drawing the character glyph.
		/// The C spacing is the distance to add to the current position to provide white space to the right of the character glyph.
		/// </summary>
		[ProtoMember(6)]
        public Vector2 ACWidths;
		/// <summary>
		/// List of kerning pairs, related to this character
		/// </summary>
		[ProtoMember(7)]
		public List<KerningPair> KerningPairs;
		/// <summary>
		/// Font texture's index which contains the given glyph
		/// </summary>
		[ProtoMember(8)]
		public int TextureIndex;
		/// <summary>
		/// The null-character which denotes any missing character in a font
		/// </summary>
		public static FontChar Null = new FontChar();

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