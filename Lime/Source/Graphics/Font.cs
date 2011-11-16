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
		public PersistentTexture Texture = new PersistentTexture ();
		[ProtoMember(2)]
		public readonly FontCharCollection Chars = new FontCharCollection ();
	}

	[ProtoContract]
	public class FontCharCollection : ICollection<FontChar>
	{
		public List<FontChar> charList = new List<FontChar> ();
		public FontChar [] [] charMap = new FontChar [256] [];

		public FontChar this [char code] { 
			get { 
				byte hb = (byte)(code >> 8);
				byte lb = (byte)(code & 255);
				if (charMap [hb] != null) {
					return charMap [hb] [lb];
				}
				return null;
			}
		}

		public void CopyTo (Array a, int index)
		{
		}

		public int Count { get { return charList.Count; } }

		IEnumerator<FontChar> IEnumerable<FontChar>.GetEnumerator ()
		{
			return charList.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return charList.GetEnumerator ();
		}

		void ICollection<FontChar>.CopyTo (FontChar[] a, int index)
		{
			charList.CopyTo (a, index);
		}
		
		public void Clear ()
		{
			charList.Clear ();
			for (int i = 0; i < 256; i++) {
				charMap [i] = null;
			}
		}
		
		public bool Contains (FontChar item)
		{
			byte hb = (byte)(item.Char >> 8);
			byte lb = (byte)(item.Char & 255);
			if (charMap [hb] != null) {
				return charMap [hb] [lb] != null;
			}
			return false;
		}
		
		public bool Remove (FontChar item)
		{
			byte hb = (byte)(item.Char >> 8);
			byte lb = (byte)(item.Char & 255);
			if (charMap [hb] != null) {
				charMap [hb] [lb] = null;
			}
			return charList.Remove (item);
		}
		
		bool ICollection<FontChar>.IsReadOnly { 
			get { return false; }
		}

		public void Add (FontChar c)
		{
			byte hb = (byte)(c.Char >> 8);
			byte lb = (byte)(c.Char & 255);
			if (charMap [hb] == null) {
				charMap [hb] = new FontChar [256];
			}
			charMap [hb] [lb] = c;
			charList.Add (c);
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

	[ProtoContract]
	public class FontChar
	{
		[ProtoMember(1)]
		public char Char;
		[ProtoMember(2)]
		public Vector2 UV0;
		[ProtoMember(3)]
		public Vector2 UV1;
		[ProtoMember(4)]
		public Vector2 Size;
		[ProtoMember(5)]
		public Vector2 ACWidths;
		[ProtoMember(6)]
		public List<KerningPair> KerningPairs;
	}
}