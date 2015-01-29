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
		public List<SerializableTexture> Textures = new List<SerializableTexture>();
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
				if (code == 160) { // ����������� ������
					return this[' ']; // ���� ��� ������� ������
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
		public float Width;
		[ProtoMember(5)]
		public float Height;
		[ProtoMember(6)]
		public Vector2 ACWidths;
		[ProtoMember(7)]
		public List<KerningPair> KerningPairs;
		[ProtoMember(8)]
		public int TextureIndex;
		public static FontChar Null = new FontChar();
	}
}