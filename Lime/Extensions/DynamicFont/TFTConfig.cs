using System.Collections.Generic;
using System.Runtime.InteropServices;
using Yuzu;

namespace Lime
{
	public class TftConfig
	{
		public const string Extension = "tftconf";

		public class CharSet
		{
			[YuzuMember]
			public string Chars { get; set; } = "";
			[YuzuMember]
			public string Font { get; set; }
			// Do not extract anything if it's null or empty
			[YuzuMember]
			public string ExtractFromDictionaries { get; set; }
		}

		[YuzuMember]
		public int Height { get; set; }
		[YuzuMember]
		public int Padding { get; set; }
		[YuzuMember]
		public Dictionary<char, List<KerningPair>> CustomKerningPairs { get; private set; } = new Dictionary<char, List<KerningPair>>();
		[YuzuMember]
		public List<CharSet> CharSets { get; private set; } = new List<CharSet>();
		[YuzuMember]
		public string ExcludeChars { get; set; } = "";
		[YuzuMember]
		public bool IsSdf { get; set; }
		[YuzuMember]
		public float SdfScale { get; set; } = 0.25f;
		[YuzuMember]
		public Size TextureSize { get; set; }
	}
}
