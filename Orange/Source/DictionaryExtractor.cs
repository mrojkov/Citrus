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
		enum LocalizationPass
		{
			ExtractTaggedStrings,
			TagUntaggedStrings
		}

		public void ExtractDictionary()
		{
			const string dictionary = "Dictionary.txt";
			LoadDictionary(dictionary);
			RunLocalizationPass(LocalizationPass.ExtractTaggedStrings);
			RunLocalizationPass(LocalizationPass.TagUntaggedStrings);
			SaveDictionary(dictionary);
		}

		private static void SaveDictionary(string dictionary)
		{
			using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
				using (var stream = new FileStream(dictionary, FileMode.Create)) {
					Localization.Dictionary.WriteToStream(stream);
				}
			}
		}

		private static void LoadDictionary(string dictionary)
		{
			Localization.Dictionary.Clear();
			using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
				if (File.Exists(dictionary)) {
					using (var stream = new FileStream(dictionary, FileMode.Open)) {
						Localization.Dictionary.ReadFromStream(stream);
					}
				}
			}
		}

		private void RunLocalizationPass(LocalizationPass pass)
		{
			var sourceFiles = new FileEnumerator(The.Workspace.ProjectDirectory);
			using (new DirectoryChanger(The.Workspace.ProjectDirectory)) {
				var files = sourceFiles.Enumerate(".cs");
				foreach (var fileInfo in files) {
					if (pass == 0) {
						Console.WriteLine("* " + fileInfo.Path);
					}
					ProcessSourceFile(fileInfo.Path, pass);
				}
			}
			using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
				var files = The.Workspace.AssetFiles.Enumerate(".scene");
				foreach (var fileInfo in files) {
					if (pass == 0)
						Console.WriteLine("* " + fileInfo.Path);
					// Сначала прогоним все строки вида: "[]blah-blah.."
					ProcessSourceFile(fileInfo.Path, pass);
					// Затем прогоним все строки вида: Text "blah-blah.."
					if (!ShouldLocalizeOnlyTaggedSceneTexts()) {
						ProcessSceneFile(fileInfo.Path, pass);
					}
				}
			}
		}

		private bool ShouldLocalizeOnlyTaggedSceneTexts()
		{
			return (bool)The.Workspace.ProjectJson.GetValue("LocalizeOnlyTaggedSceneTexts", false);
		}

		void ProcessSourceFile(string file, LocalizationPass pass)
		{
			const string quotedStringPattern = @"""([^""\\]*(?:\\.[^""\\]*)*)""";
			var originalCode = File.ReadAllText(file, Encoding.Default);
			var context = GetContext(file);
			var processedCode = Regex.Replace(originalCode, quotedStringPattern,
				(match) => {
					string s = match.Groups[1].Value;
 					if (pass == LocalizationPass.TagUntaggedStrings || IsStringTagged(s)) {
						s = ProcessTextLine(s, context, processStringsWithoutBrackets: false);
					}
					return '"' + s + '"';
				});
			if (processedCode != originalCode) {
				File.WriteAllText(file, processedCode, Encoding.UTF8);
			}
		}

		void ProcessSceneFile(string file, LocalizationPass pass)
		{
			var originalCode = File.ReadAllText(file, Encoding.Default);
			var processedCode = Regex.Replace(originalCode, @"^(\s*Text)\s""([^""\\]*(?:\\.[^""\\]*)*)""$", 
				(match) => {
					string context = GetContext(file);
					string prefix = match.Groups[1].Value;
					string text = match.Groups[2].Value;
					if (pass == LocalizationPass.TagUntaggedStrings || IsStringTagged(text)) {
						text = ProcessTextLine(text, context, processStringsWithoutBrackets: true);
					}
					string result = string.Format(@"{0} ""{1}""", prefix, text);
					return result;
				}, RegexOptions.Multiline);
			if (originalCode != processedCode) {
				File.WriteAllText(file, processedCode, Encoding.UTF8);
			}
		}

		private string GetContext(string file)
		{
			return file;	
		}

		bool IsStringTagged(string str)
		{
			return Regex.Match(str, @"^\[(\d+)\](.*)$").Success;
		}

		string ProcessTextLine(string text, string context, bool processStringsWithoutBrackets)
		{
			var match = Regex.Match(text, @"^\[(\d*)\](.*)$");
			if (match.Success) {
				if (match.Groups[1].Length > 0) {
					// The line starts with "[123]..."
					int tag = int.Parse(match.Groups[1].Value);
					if (Localization.Dictionary.ContainsKey(tag)) {
						// Put a text from the dictionary back to the source file
						text = Localization.Dictionary[tag].Text;
						AddTextToDictionary(tag, text, context);
						text = string.Format("[{0}]{1}", tag, Escape(text));
					} else {
						AddTextToDictionary(tag, Unescape(text), context);
					}
				} else {
					// The line starts with "[]..."
					string value = match.Groups[2].Value;
					if (HasAlphabeticCharacters(value)) {
						int tag = GenerateTagForText(Unescape(value));
						AddTextToDictionary(tag, Unescape(value), context);
						text = string.Format("[{0}]{1}", tag, value);
					}
				}
			} else if (processStringsWithoutBrackets) {
				if (HasAlphabeticCharacters(text)) {
					// The line has no [] prefix, but still should be localized. 
					// E.g. most of texts in scene files.
					int tag = GenerateTagForText(Unescape(text));
					AddTextToDictionary(tag, Unescape(text), context);
					text = string.Format("[{0}]{1}", tag, text);
				}
			}
			return text;
		}

		private bool HasAlphabeticCharacters(string text)
		{
			return text.Any(c => char.IsLetter(c));
		}

		private static void AddTextToDictionary(int tag, string value, string context)
		{
			var e = Localization.Dictionary.GetEntry(tag);
			e.Text = value;
			var ctx = new List<string>();
			if (!string.IsNullOrWhiteSpace(e.Context)) {
				ctx = e.Context.Split('\n').ToList();
			}
			if (!ctx.Contains(context)) {
				ctx.Add(context);
			}
			e.Context = string.Join("\n", ctx);
		}

		private static string Escape(string text)
		{
			return text.Replace("\n", "\\n").Replace("\"", "\\\"").Replace("'", "\\'");
		}

		private static string Unescape(string text)
		{
			return text.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\'", "'");
		}

		// Try to look up the value in the dictionary, and if success return an existing key, 
		// else generate a new one
		private static int GenerateTagForText(string text)
		{
			foreach (var pair in Localization.Dictionary) {
				if (pair.Value.Text == text) {
					return pair.Key;
				}
			}
			return GenerateTag();
		}

		private static int GenerateTag()
		{
			string s;
			for (int tag = 1; ; tag++) {
				if (!Localization.Dictionary.TryGetText(tag, out s)) {
					return tag;
				}
			}
		}
	}
}
