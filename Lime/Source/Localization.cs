using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lime
{
	public class LocalizationDictionary
	{
		Dictionary<int, string> strings = new Dictionary<int, string>();

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

		public int GenerateTag()
		{
			string s;
			for (int tag = 1; ; tag++) {
				if (!TryGetString(tag, out s))
					return tag;
			}
		}

		// Сначала пытается поискать value в словаре, и если оно там есть, то возвращает имеющийся ключ
		public int GenerateTagForValue(string value)
		{
			foreach (var pair in strings) {
				if (pair.Value == value) {
					return pair.Key;
				}
			}
			return GenerateTag();
		}

		public void ReadFromStream(Stream stream)
		{
			using (var r = new StreamReader(stream)) {
				string line = r.ReadLine();
				while (line != null) {
					line = line.Trim();
					if (line.Length < 3 || line[0] != '[' || line[line.Length - 1] != ']')
						throw new Lime.Exception("Invalid tag");
					string tagLine = line.Substring(1, line.Length - 2);
					int tag;
					if (!int.TryParse(tagLine, out tag))
						throw new Lime.Exception("Invalid tag");
					string text = "";
					while (true) {
						line = r.ReadLine();
						if (line == null || line.Length > 0 && line[0] == '[')
							break;
						text += line + '\n';
					}
					text = text.TrimEnd('\n');
					strings[tag] = text;
				}
			}
		}

		public void WriteToStream(Stream stream)
		{
			using (var w = new StreamWriter(stream, new UTF8Encoding(true))) {
				foreach (KeyValuePair<int, string> p in strings) {
					w.WriteLine(string.Format("[{0}]", p.Key));
					w.WriteLine(p.Value);
					w.WriteLine();
				}
			}
		}

		public void Clear()
		{
			strings.Clear();
		}
	}

	public static class Localization
	{
		public static LocalizationDictionary Dictionary = new LocalizationDictionary();
		
		public static string GetCurrentLanguage()
		{
			return System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
		}

		public static string GetString(string taggedFormat, params object[] args)
		{
			string s = GetString(taggedFormat);
			return string.Format(s, args);
		}

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