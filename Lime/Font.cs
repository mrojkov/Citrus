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
        [ProtoMember(3)]
		public readonly FontPairCollection Pairs = new FontPairCollection ();
	}

    [ProtoContract]
	public class FontCharCollection : ICollection<FontChar>
	{
		[ProtoMember(1)]
		public List<FontChar> charList = new List<FontChar> ();
		[ProtoMember(2)]
		public Dictionary<uint, FontChar> charDic = new Dictionary<uint, FontChar> ();

		public FontChar this [uint code] { 
			get { return charDic [code]; }
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
			charDic.Clear ();
		}
		
		public bool Contains (FontChar item)
		{
			return charDic.ContainsKey (item.Code);
		}
		
		public bool Remove (FontChar item)
		{
			charDic.Remove (item.Code);
			return charList.Remove (item);
		}
		
		bool ICollection<FontChar>.IsReadOnly { 
			get { return false; }
		}

		public void Add (FontChar c)
		{
			charDic [c.Code] = c;
			charList.Add (c);
		}
	}

    [ProtoContract]
	public struct FontChar
	{
        [ProtoMember(1)]
		public uint Code;
        [ProtoMember(2)]
		public Vector2 Position;
        [ProtoMember(3)]
		public Vector2 Size;
        [ProtoMember(4)]
		public Vector2 ACWidths;
	}

    [ProtoContract]
	public struct FontPair
	{
        [ProtoMember(1)]
		public uint Code1;
        [ProtoMember(2)]
		public uint Code2;
        [ProtoMember(3)]
		public float Delta;
	}

    [ProtoContract]
	public class FontPairCollection : ICollection<FontPair>
	{
		[ProtoMember(1)]
		List<FontPair> pairList = new List<FontPair> ();
		
		[ProtoMember(2)]
		Dictionary<uint, float> pairDic = new Dictionary<uint, float> ();

		public float Get (uint Left, uint Right)
		{
			float value;
			var key = (Left << 16) | Right;
			if (pairDic.TryGetValue (key, out value))
				return value;
			return 0.0f;
		}

		IEnumerator<FontPair> IEnumerable<FontPair>.GetEnumerator ()
		{
			return pairList.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return pairList.GetEnumerator ();
		}
	
		void ICollection<FontPair>.CopyTo (FontPair[] a, int index)
		{
			pairList.CopyTo (a, index);
		}

		public int Count { get { return pairList.Count; } }

		public void Add (FontPair pair)
		{
			var key = (pair.Code1 << 16) | pair.Code2;
			pairDic [key] = pair.Delta;
			pairList.Add (pair);
		}

		public bool Contains (FontPair item)
		{
			var key = (item.Code1 << 16) | item.Code2;
			return pairDic.ContainsKey (key);
		}

		bool ICollection<FontPair>.IsReadOnly { 
			get { return false; }
		}

		public bool Remove (FontPair item)
		{
			var key = (item.Code1 << 16) | item.Code2;
			pairDic.Remove (key);
			return pairList.Remove (item);
		}
		
		public void Clear ()
		{
			pairList.Clear ();
			pairDic.Clear ();
		}
	}
}