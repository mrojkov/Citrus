using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	/// <summary>
	/// Configuration file that is used for Tangerine Font (.tft) generation.
	/// </summary>
	public class TftConfig
	{
		/// <summary>
		/// Configuration file extension.
		/// </summary>
		public const string Extension = "tftconf";


		public class CharSet
		{
			/// <summary>
			/// A set of characters to be used for font generation.
			/// </summary>
			[YuzuMember]
			public string Chars { get; set; } = "";
			/// <summary>
			/// Path (relative to asset directory) to font file (e.g. ttf or otf)
			/// from which glyphs are taken.
			/// </summary>
			[YuzuMember]
			public string Font { get; set; }
			/// <summary>
			/// A comma separated set of localizations (e.g. "EN,RU,CN").
			/// If it's null or empty existing Chars are used, otherwise
			/// Chars are extracted from localization dictionaries (e.g. "Dictionary.RU.txt" or
			/// "Dictionary.txt" for EN).
			/// </summary>
			[YuzuMember]
			public string ExtractFromDictionaries { get; set; }
		}

		/// <summary>
		/// Font height.
		/// </summary>
		[YuzuMember]
		public int Height { get; set; }
		/// <summary>
		/// Padding between adjacent characters.
		/// </summary>
		[YuzuMember]
		public int Padding { get; set; } = 1;
		/// <summary>
		/// User-defined kerning pairs.
		/// </summary>
		[YuzuMember]
		public Dictionary<char, List<KerningPair>> CustomKerningPairs { get; private set; } = new Dictionary<char, List<KerningPair>>();
		/// <summary>
		/// A list of CharSet for font generation.
		/// </summary>
		[YuzuMember]
		public List<CharSet> CharSets { get; private set; } = new List<CharSet>();
		/// <summary>
		/// Characters that will be ignored during font generation step.
		/// </summary>
		[YuzuMember]
		public string ExcludeChars { get; set; } = "";
		/// <summary>
		/// Is generated font will be Signed Distance Field font or not.
		/// </summary>
		[YuzuMember]
		public bool IsSdf { get; set; }
		/// <summary>
		/// Used only when IsSdf is true. Scale factor for generated textures.
		/// </summary>
		[YuzuMember]
		public float SdfScale { get; set; } = 0.25f;
		/// <summary>
		/// Size for each generated texture.
		/// </summary>
		[YuzuMember]
		public Size TextureSize { get; set; }
	}
}
