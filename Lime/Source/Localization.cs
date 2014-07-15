using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lime
{
	public class LocalizationEntry
	{
		public string Text;
		public string Context;
	}

	public class LocalizationDictionary : Dictionary<string, LocalizationEntry>
	{
		public LocalizationEntry GetEntry(string tag)
		{
			LocalizationEntry e;
			if (TryGetValue(tag, out e)) {
				return e;
			} else {
				e = new LocalizationEntry();
				Add(tag, e);
				return e;
			}
		}

		public void Add(string tag, string text, string context)
		{
			var e = GetEntry(tag);
			e.Text = text;
			e.Context = context;
		}

		public bool TryGetText(string tag, out string value)
		{
			value = null;
			LocalizationEntry e;
			if (TryGetValue(tag, out e)) {
				value = e.Text;
			}
			return value != null;
		}

		public void ReadFromStream(Stream stream)
		{
			using (var r = new StreamReader(stream)) {
				string line = r.ReadLine();
				while (line != null) {
					line = line.Trim();
					if (line.Length < 2 || line[0] != '[')
						throw new Lime.Exception("Invalid key");
					var key = line.Substring(1, line.Length - 2);
					string context = null;
					string text = "";
					while (true) {
						line = r.ReadLine();
						if (line == null || line.Length > 0 && line[0] == '[')
							break;
						if (line.Length > 0 && line[0] == '#') {
							context = (context ?? "") + line.Substring(1).Trim() + '\n';
						} else {
							text += line + '\n';
						}
					}
					text = text.TrimEnd('\n');
					if (context != null) {
						context = context.TrimEnd('\n');
					}
					Add(key, text, context);
				}
			}
		}

		public void WriteToStream(Stream stream)
		{
			using (var w = new StreamWriter(stream, new UTF8Encoding(true))) {
				foreach (var p in this) {
					w.WriteLine(p.Key);
					if (!string.IsNullOrWhiteSpace(p.Value.Context)) {
						foreach (var i in p.Value.Context.Split('\n')) {
							w.WriteLine("# " + i);
						}
					}
					w.WriteLine(p.Value.Text);
					w.WriteLine();
				}
			}
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
						var key = taggedString.Substring(1, closeBrackedPos - 1);
						if (Dictionary.TryGetText(key, out text)) {
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
	}
}