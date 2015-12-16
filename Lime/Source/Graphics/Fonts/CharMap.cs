
namespace Lime
{
	public class CharMap
	{
		private FontChar[][] charMap = new FontChar[256][];

		public static bool TranslateKnownMissingChars(ref char code)
		{
			var origCode = code;
			// Can use normal space instead of unbreakable space
			if (code == 160) {
				code = ' ';
			}
			// Can use 'middle dot' instead of 'bullet operator'
			if (code == 8729) {
				code = (char)183;
			}
			// Can use 'degree symbol' instead of 'masculine ordinal indicator'
			if (code == 186) {
				code = (char)176;
			}
			// Use '#' instead of 'numero sign'
			if (code == 8470) {
				code = '#';
			}
			return code != origCode;
		}

		public FontChar this[char code]
		{
			get
			{
				var hb = (byte)(code >> 8);
				var lb = (byte)(code & 255);
				if (charMap[hb] != null) {
					return charMap[hb][lb];
				}
				return null;
			}
			set
			{
				var hb = (byte)(code >> 8);
				var lb = (byte)(code & 255);
				if (charMap[hb] == null) {
					charMap[hb] = new FontChar[256];
				}
				charMap[hb][lb] = value;
			}
		}

		public bool Contains(char code)
		{
			return this[code] != null;
		}

		public void Clear()
		{
			for (int i = 0; i < 256; i++) {
				charMap[i] = null;
			}
		}
	}
}
