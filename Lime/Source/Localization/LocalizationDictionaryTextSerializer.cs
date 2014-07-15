using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lime
{
	public class LocalizationDictionaryTextSerializer : ILocalizationDictionarySerializer
	{
		public string GetFileExtension()
		{
			return ".txt";
		}

		public void Read(LocalizationDictionary dictionary, Stream stream)
		{
			using (var r = new StreamReader(stream)) {
				string line = r.ReadLine();
				while (line != null) {
					line = line.Trim();
					if (!ValidateKey(line))
						throw new Lime.Exception("Invalid key format: {0}", line);
					var key = Unescape(line.Substring(1, line.Length - 2));
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
					dictionary.Add(key, text, context);
				}
			}
		}

		private static bool ValidateKey(string s)
		{
			var l = s.Length;
			return l >= 2 && s[0] == '[' && s[l - 1] == ']';
		}

		public void Write(LocalizationDictionary dictionary, Stream stream)
		{
			using (var w = new StreamWriter(stream, new UTF8Encoding(true))) {
				foreach (var p in dictionary) {
					w.WriteLine('[' + Escape(p.Key) + ']');
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

		private static string Escape(string text)
		{
			return text.Replace("\n", "\\n").Replace("\"", "\\\"");
		}

		private static string Unescape(string text)
		{
			return text.Replace("\\n", "\n").Replace("\\\"", "\"");
		}
	}
}
