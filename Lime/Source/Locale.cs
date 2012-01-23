using System.Collections.Generic;
using System.IO;

namespace Lime
{
	public class LocalizationDictionary
	{
		Dictionary<int, string> strings = new Dictionary<int, string>();

		public LocalizationDictionary()
		{
		}

		public LocalizationDictionary(Stream stream)
		{
			ReadFromStream(stream);
		}

		public LocalizationDictionary(string file)
		{
			using (var stream = AssetsBundle.Instance.OpenFile(file)) {
				ReadFromStream(stream);
			}
		}

		public void Add(int tag, string value)
		{
			strings[tag] = value;
		}

		public bool Contains(int tag)
		{
			return strings.ContainsKey(tag);
		}

		public bool TryGetString(int tag, out string value)
		{
			return strings.TryGetValue(tag, out value);
		}

		public void ReadFromStream(Stream stream)
		{
		}

		public void WriteToStream(Stream stream)
		{
			using (var w = new StreamWriter(stream)) {
				foreach(KeyValuePair<int, string> p in strings) {
					w.WriteLine(string.Format("[{0}]", p.Key));
					w.WriteLine(p.Value);
					w.WriteLine();
				}
			}
		}
	}

	public static class Locale
	{
		public static LocalizationDictionary Dictionary = new LocalizationDictionary();

		public static string GetString(string taggedString)
		{
			if (string.IsNullOrEmpty(taggedString))
				return taggedString;
			if (taggedString[0] == '[') {
				int closeBrackedPos = 0;
				for (int i = 1; i < taggedString.Length; i++) {
					if (taggedString[i] == ']') {
						closeBrackedPos = i;
						break;
					}
					if (!char.IsDigit(taggedString, i)) {
						break;
					}
				}
				if (closeBrackedPos >= 1) {
					string text;
					if (closeBrackedPos > 1) {
						int key = ParseInt(taggedString, 1, closeBrackedPos - 1);
						if (Dictionary.TryGetString(key, out text)) {
							return text;
						}
					}
					// key/value pair not defined or key is empty ("[]" case).
					text = taggedString.Substring(closeBrackedPos + 1);
					return text;
				}
			}
			return taggedString;
		}

		static int ParseInt(string source, int index, int length)
		{
			int v = 0;
			for (int i = index; i < index + length; i++) {
				v *= 10;
				v += (int)source[i] - (int)'0';
			}
			return v;
		}
	}
}