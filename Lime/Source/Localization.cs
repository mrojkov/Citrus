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
		public LocalizationEntry GetEntry(string key)
		{
			LocalizationEntry e;
			if (TryGetValue(key, out e)) {
				return e;
			} else {
				e = new LocalizationEntry();
				Add(key, e);
				return e;
			}
		}

		public void Add(string key, string text, string context)
		{
			var e = GetEntry(key);
			e.Text = text;
			e.Context = context;
		}

		public bool TryGetText(string key, out string value)
		{
			value = null;
			LocalizationEntry e;
			if (TryGetValue(key, out e)) {
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
					if (!ValidateKey(line))
						throw new Lime.Exception("Invalid key format: {0}", line);
					var key = line.Substring(1, line.Length - 2);
					string context = null;
					string text = "";
					while (true) {
						line = r.ReadLine();
						if (line == null || ValidateKey(line))
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

		private static bool ValidateKey(string s)
		{
			var l = s.Length;
			return l >= 2 && s[0] == '[' && s[l - 1] == ']';
		}

		public void WriteToStream(Stream stream)
		{
			using (var w = new StreamWriter(stream, new UTF8Encoding(true))) {
				foreach (var p in this) {
					w.WriteLine('[' + p.Key + ']');
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

		public static string GetString(string format, params object[] args)
		{
			string s = GetString(format);
			return string.Format(s, args);
		}

		public static string GetString(string key)
		{
			if (string.IsNullOrEmpty(key)) {
				return key;
			}
			if (key.Length >= 2 && key[0] == '[' && key[1] == ']') {
				key = key.Substring(2);
			}
			string text;
			return Dictionary.TryGetText(key, out text) ? text : key;
		}
	}
}