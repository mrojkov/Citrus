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

		public void ExtractDictionary()
		{
			const string dictionary = "Dictionary.txt";
			Localization.Dictionary.Clear();
			using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
				if (File.Exists(dictionary)) {
					using (var stream = new FileStream(dictionary, FileMode.Open)) {
						Localization.Dictionary.ReadFromStream(stream);
					}
				}
			}
			for (int pass = 0; pass < 2; pass++) {
				var sourceFiles = new FileEnumerator(The.Workspace.ProjectDirectory);
				using (new DirectoryChanger(The.Workspace.ProjectDirectory)) {
					var files = sourceFiles.Enumerate(".cs");
					foreach (var fileInfo in files) {
						if (pass == 0)
							Console.WriteLine("* " + fileInfo.Path);
						ProcessSourceFile(fileInfo.Path, (Pass)pass);
					}
				}
				using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
					var files = The.Workspace.AssetFiles.Enumerate(".scene");
					foreach (var fileInfo in files) {
						if (pass == 0)
							Console.WriteLine("* " + fileInfo.Path);
						// Сначала прогоним все строки вида: "[]blah-blah.."
						ProcessSourceFile(fileInfo.Path, (Pass)pass);
						// Затем прогоним все строки вида: Text "blah-blah.."
						if (!ShouldLocalizeOnlyTaggedSceneTexts()) {
							ProcessSceneFile(fileInfo.Path, (Pass)pass);
						}
					}
				}
			}
			using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
				using (var stream = new FileStream(dictionary, FileMode.Create)) {
					Localization.Dictionary.WriteToStream(stream);
				}
			}
		}

		private bool ShouldLocalizeOnlyTaggedSceneTexts()
		{
			return (bool)The.Workspace.ProjectJson.GetValue("LocalizeOnlyTaggedSceneTexts", false);
		}

		void ProcessSourceFile(string file, Pass pass)
		{
			var origText = File.ReadAllText(file, Encoding.Default);
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
				File.WriteAllText(file, text, Encoding.UTF8);
			}
		}

		void ProcessSceneFile(string file, Pass pass)
		{
			var origText = File.ReadAllText(file, Encoding.Default);
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
				File.WriteAllText(file, text, Encoding.UTF8);
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
					string value = match.Groups[2].Value;
					int tag = Localization.Dictionary.GenerateTagForValue(ReplaceNewlines(value));
					Localization.Dictionary.Add(tag, ReplaceNewlines(value));
					str = string.Format("[{0}]{1}", tag, value);
					return str;
				}
			} else {
				int tag = Localization.Dictionary.GenerateTagForValue(ReplaceNewlines(str));
				Localization.Dictionary.Add(tag, ReplaceNewlines(str));
				str = string.Format("[{0}]{1}", tag, str);
				return str;
			}
		}

		string ReplaceNewlines(string s)
		{
			return s.Replace("\\n", "\n");
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
