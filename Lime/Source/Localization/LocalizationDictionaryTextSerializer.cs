using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lime
{
	/// <summary>
	/// Сериалайзер, работающий с текстовыми словарями (словари, использующиеся в большинстве проектов)
	/// </summary>
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
					if (ValidateComment(line) || ValidateSpace(line)) {
						dictionary.AddComment(ValidateSpace(line) ? null : line.Substring(1).Trim());
						line = r.ReadLine();
						continue;
					}
					if (!ValidateKey(line)) {
						throw new Lime.Exception("Invalid key format: {0}", line);
					}
					var key = Unescape(line.Substring(1, line.Length - 2));
					string context = null;
					string text = "";
					while (true) {
						line = r.ReadLine();
						var isCommentAfterValue = line != null && !string.IsNullOrEmpty(text) && ValidateComment(line);
						if (line == null || ValidateKey(line) || isCommentAfterValue)
							break;
						if (ValidateComment(line)) {
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

		private static bool ValidateComment(string s)
		{
			return s.Length > 0 && s[0] == '#';
		}

		private static bool ValidateSpace(string s)
		{
			return s == "";
		}

		public void Write(LocalizationDictionary dictionary, Stream stream)
		{
			using (var w = new StreamWriter(stream, new UTF8Encoding(true))) {
				foreach (var p in dictionary) {
					if (LocalizationDictionary.IsComment(p.Key)) {
						if (!string.IsNullOrEmpty(p.Value.Context)) {
							foreach (var i in p.Value.Context.Split('\n')) {
								w.WriteLine("# " + i);
							}
						} else {
							w.WriteLine();
						}
					} else {
						w.WriteLine('[' + Escape(p.Key) + ']');
						if (!string.IsNullOrEmpty(p.Value.Context)) {
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
