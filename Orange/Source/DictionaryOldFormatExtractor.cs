using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Lime;

namespace Orange
{
	public class DictionaryOldFormatExtractor
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
					ProcessSourceFile(fileInfo.Path, pass, Encoding.UTF8);
				}
			}
			using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
				var files = The.Workspace.AssetFiles.Enumerate(".scene");
				foreach (var fileInfo in files) {
					if (pass == 0)
						Console.WriteLine("* " + fileInfo.Path);
					// Сначала прогоним все строки вида: "[]blah-blah.."
					ProcessSourceFile(fileInfo.Path, pass, Encoding.Default);
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

		void ProcessSourceFile(string file, LocalizationPass pass, Encoding saveEncoding)
		{
			const string quotedStringPattern = @"""([^""\\]*(?:\\.[^""\\]*)*)""";
			var originalCode = File.ReadAllText(file);
			var processedCode = Regex.Replace(originalCode, quotedStringPattern,
				(match) => {
					string s = match.Groups[1].Value;
 					if (pass == LocalizationPass.TagUntaggedStrings || IsStringTagged(s)) {
						s = ProcessTextLine(s, processStringsWithoutBrackets: false);
					}
					return '"' + s + '"';
				});
			if (processedCode != originalCode) {
				File.WriteAllText(file, processedCode, saveEncoding);
			}
		}

		void ProcessSceneFile(string file, LocalizationPass pass)
		{
			var originalCode = File.ReadAllText(file, Encoding.Default);
			var processedCode = Regex.Replace(originalCode, @"^(\s*Text)\s""([^""\\]*(?:\\.[^""\\]*)*)""$", 
				(match) => {
					string prefix = match.Groups[1].Value;
					string text = match.Groups[2].Value;
					if (pass == LocalizationPass.TagUntaggedStrings || IsStringTagged(text)) {
						text = ProcessTextLine(text, processStringsWithoutBrackets: true);
					}
					string result = string.Format(@"{0} ""{1}""", prefix, text);
					return result;
				}, RegexOptions.Multiline);
			if (originalCode != processedCode) {
				File.WriteAllText(file, processedCode, Encoding.UTF8);
			}
		}

		bool IsStringTagged(string str)
		{
			return Regex.Match(str, @"^\[(\d+)\](.*)$").Success;
		}

		string ProcessTextLine(string text, bool processStringsWithoutBrackets)
		{
			var match = Regex.Match(text, @"^\[(\d*)\](.*)$");
			if (match.Success) {
				if (match.Groups[1].Length > 0) {
					// The line starts with "[123]..."
					var key = match.Groups[1].Value;
					if (Localization.Dictionary.ContainsKey(key)) {
						// Put a text from the dictionary back to the source file
						// buz: отключил
						/*text = Localization.Dictionary[key].Text;
						AddTextToDictionary(key, text);
						text = string.Format("[{0}]{1}", key, Escape(text));*/
					} else {
						AddTextToDictionary(key, Unescape(text));
					}
				} else {
					// The line starts with "[]..."
					string value = match.Groups[2].Value;
					if (HasAlphabeticCharacters(value)) {
						var key = GenerateTagForText(Unescape(value));
						AddTextToDictionary(key, Unescape(value));
						text = string.Format("[{0}]{1}", key, value);
					}
				}
			} else if (processStringsWithoutBrackets) {
				if (HasAlphabeticCharacters(text)) {
					// The line has no [] prefix, but still should be localized. 
					// E.g. most of texts in scene files.
					var key = GenerateTagForText(Unescape(text));
					AddTextToDictionary(key, Unescape(text));
					text = string.Format("[{0}]{1}", key, text);
				}
			}
			return text;
		}

		private bool HasAlphabeticCharacters(string text)
		{
			return text.Any(c => char.IsLetter(c));
		}

		private static void AddTextToDictionary(string key, string value)
		{
			var e = Localization.Dictionary.GetEntry(key);
			e.Text = value;
			e.Context = null;
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
		private static string GenerateTagForText(string text)
		{
			foreach (var pair in Localization.Dictionary) {
				if (pair.Value.Text == text) {
					return pair.Key;
				}
			}
			return GenerateKey();
		}

		private static string GenerateKey()
		{
			for (int i = 1; ; i++) {
				if (!Localization.Dictionary.ContainsKey(i.ToString())) {
					return i.ToString();
				}
			}
		}
	}
}
