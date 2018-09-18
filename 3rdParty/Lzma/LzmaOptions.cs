using System;
using SevenZip.Compression.LZMA;

namespace Lzma
{
	public class LzmaOptions
	{
		public static readonly LzmaOptions Default = new LzmaOptions();

		public int DictionarySize { get; set; }
		public int PosStateBits { get; set; }
		public int LiteralContextBits { get; set; }
		public int LiteralPosBits { get; set; }
		public int AlgorithmVersion { get; set; }
		public int NumFastBytes { get; set; }
		public LzmaMatchFinder MatchFinder { get; set; }
		public int MaxChunkSize { get; set; }

		public LzmaOptions()
		{
			DictionarySize = 8 * 1024 * 1024;
			PosStateBits = 2;
			LiteralContextBits = 3;
			LiteralPosBits = 0;
			AlgorithmVersion = 2;
			NumFastBytes = 32;
			MatchFinder = LzmaMatchFinder.BT4;
			MaxChunkSize = 100 * 1024 * 1024;
		}
	}

	public enum LzmaMatchFinder
	{
		BT2,
		BT4
	}
}