using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Lime;

namespace Orange
{
	public class DictionaryExtractor
	{
		enum Pass
		{
			ExtractTaggedStrings,
			TagUntaggedStrings
		}

		CitrusProject project;

		public DictionaryExtractor(CitrusProject project)
		{
			this.project = project;
		}
		
		public void ExtractDictionary()
		{
			const string dictionary = "Dictionary.txt";
			Locale.Dictionary.Clear();
			for (int pass = 0; pass < 2; pass++) {
				using (new DirectoryChanger(project.ProjectDirectory)) {
					var files = Helpers.GetAllFiles(".", "*.cs", true);
					foreach (string file in files) {
						if (pass == 0)
							Console.WriteLine("* " + file);
						ProcessSourceFile(file, (Pass)pass);
					}
				}
				using (new DirectoryChanger(project.AssetsDirectory)) {
					var files = Helpers.GetAllFiles(".", "*.scene", true);
					foreach (string file in files) {
						if (pass == 0)
							Console.WriteLine("* " + file);
						ProcessSceneFile(file, (Pass)pass);
					}
				}
			}
			using (new DirectoryChanger(project.AssetsDirectory)) {
				using (var stream = new FileStream(dictionary, FileMode.Create)) {
					Locale.Dictionary.WriteToStream(stream);
				}
			}
		}

		void ProcessSourceFile(string file, Pass pass)
		{
			var origText = File.ReadAllText(file);
			var text = EscapeQuotes(origText);
			text = Regex.Replace(text, @"""(\[\d*\][^""]*)""",
				(match) => {
					string str = match.Groups[1].Value;
					if (pass == Pass.TagUntaggedStrings || IsStringTagged(str))
						str = EscapeQuotes(AddStringToDictionary(UnescapeQuotes(str)));
					return '"' + str + '"';
				});
			text = UnescapeQuotes(text);
			if (text != origText) {
				File.WriteAllText(file, text);
			}
		}

		void ProcessSceneFile(string file, Pass pass)
		{
			var origText = File.ReadAllText(file);
			var text = Regex.Replace(origText, @"^(\s*Text)\s""([^""]*)""$", 
				(match) => {
					string prefix = match.Groups[1].Value;
					string str = match.Groups[2].Value;
					if (pass == Pass.TagUntaggedStrings || IsStringTagged(str))
						str = AddStringToDictionary(str);
					string result = string.Format(@"{0} ""{1}""", prefix, str);
					return result;
				}, RegexOptions.Multiline);
			if (origText != text) {
				File.WriteAllText(file, text);
			}
		}

		bool IsStringTagged(string str)
		{
			return Regex.Match(str, @"^\[(\d+)\](.*)$").Success;
		}

		string AddStringToDictionary(string str)
		{
			var match = Regex.Match(str, @"^\[(\d*)\](.*)$");
			if (match.Success) {
				if (match.Groups[1].Length > 0) {
					// case of "[123]..."
					int tag = int.Parse(match.Groups[1].Value);
					string value = match.Groups[2].Value;
					if (!Locale.Dictionary.Contains(tag)) {
						Locale.Dictionary.Add(tag, value);
					}
					return str;
				} else {
					// case of "[]..."
					int tag = GenerateTag();
					string value = match.Groups[2].Value;
					Locale.Dictionary.Add(tag, value);
					str = string.Format("[{0}]{1}", tag, value);
					return str;
				}
			} else {
				int tag = GenerateTag();
				Locale.Dictionary.Add(tag, str);
				str = string.Format("[{0}]{1}", tag, str);
				return str;
			}
		}

		int GenerateTag()
		{
			string s;
			for (int tag = 1; ; tag++) {
				if (!Locale.Dictionary.TryGetString(tag, out s))
					return tag;
			}
		}

		static string EscapeQuotes(string str)
		{
			return str.Replace("\\\"", "&quote;");
		}

		static string UnescapeQuotes(string str)
		{
			return str.Replace("&quote;", "\\\"");
		}
	}
}
