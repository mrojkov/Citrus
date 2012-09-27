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
			Localization.Dictionary.Clear();
			using (new DirectoryChanger(project.AssetsDirectory)) {
				if (File.Exists(dictionary)) {
					using (var stream = new FileStream(dictionary, FileMode.Open)) {
						Localization.Dictionary.ReadFromStream(stream);
					}
				}
			}
			for (int pass = 0; pass < 2; pass++) {
				var sourceFiles = new FileEnumerator(project.ProjectDirectory);
				using (new DirectoryChanger(project.ProjectDirectory)) {
					var files = sourceFiles.Enumerate(".cs");
					foreach (var fileInfo in files) {
						if (pass == 0)
							Console.WriteLine("* " + fileInfo.Path);
						ProcessSourceFile(fileInfo.Path, (Pass)pass);
					}
				}
				using (new DirectoryChanger(project.AssetsDirectory)) {
					var files = project.AssetFiles.Enumerate(".scene");
					foreach (var fileInfo in files) {
						if (pass == 0)
							Console.WriteLine("* " + fileInfo.Path);
						// Сначала прогоним все строки вида: "[]blah-blah.."
						ProcessSourceFile(fileInfo.Path, (Pass)pass);
						// Затем прогоним все строки вида: Text "blah-blah.."
						ProcessSceneFile(fileInfo.Path, (Pass)pass);
					}
				}
			}
			using (new DirectoryChanger(project.AssetsDirectory)) {
				using (var stream = new FileStream(dictionary, FileMode.Create)) {
					Localization.Dictionary.WriteToStream(stream);
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
					if (!Localization.Dictionary.Contains(tag)) {
						Localization.Dictionary.Add(tag, ReplaceNewlines(value));
					}
					return str;
				} else {
					// case of "[]..."
					int tag = GenerateTag();
					string value = match.Groups[2].Value;
					Localization.Dictionary.Add(tag, ReplaceNewlines(value));
					str = string.Format("[{0}]{1}", tag, value);
					return str;
				}
			} else {
				int tag = GenerateTag();
				Localization.Dictionary.Add(tag, ReplaceNewlines(str));
				str = string.Format("[{0}]{1}", tag, str);
				return str;
			}
		}

		string ReplaceNewlines(string s)
		{
			return s.Replace("\\n", "\n");
		}

		int GenerateTag()
		{
			string s;
			for (int tag = 1; ; tag++) {
				if (!Localization.Dictionary.TryGetString(tag, out s))
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
