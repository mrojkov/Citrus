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
		private LocalizationDictionary dictionary;

		public void ExtractDictionary()
		{
			dictionary = new LocalizationDictionary();
			ExtractTexts();
			CleanupAndSaveDictionary(GetFileName());
		}

		private static string GetFileName()
		{
			return Path.ChangeExtension("Dictionary.txt", CreateSerializer().GetFileExtension());
		}

		private void CleanupAndSaveDictionary(string path)
		{
			var result = new LocalizationDictionary();
			LoadDictionary(result, path);
			MergeDictionaries(result, dictionary);
			SaveDictionary(result, path);
		}

		private void MergeDictionaries(LocalizationDictionary current, LocalizationDictionary modified)
		{
			int added = 0, deleted = 0;
			foreach (var key in modified.Keys.ToList()) {
				if (!current.ContainsKey(key)) {
					Logger.Write("+ " + key);
					added++;
					current[key] = modified[key];
				}
			}
			foreach (var key in current.Keys.ToList()) {
				if (!modified.ContainsKey(key) && !LocalizationDictionary.IsComment(key)) {
					Logger.Write("- " + key);
					deleted++;
					current.Remove(key);
				}
			}
			Logger.Write("Added {0}\nDeleted {1}", added, deleted);
		}

		private static void SaveDictionary(LocalizationDictionary dictionary, string path)
		{
			using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
				using (var stream = new FileStream(path, FileMode.Create)) {
					dictionary.WriteToStream(CreateSerializer(), stream);
				}
			}
		}

		private static ILocalizationDictionarySerializer CreateSerializer()
		{
			var format = The.Workspace.ProjectJson.GetValue("LocalizationDictionaryFormat", "Text");
			if (format == "Text") {
				return new LocalizationDictionaryTextSerializer();
			} else {
				throw new Lime.Exception();
			}
		}

		private static void LoadDictionary(LocalizationDictionary dictionary, string path)
		{
			using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
				if (File.Exists(path)) {
					using (var stream = new FileStream(path, FileMode.Open)) {
						dictionary.ReadFromStream(CreateSerializer(), stream);
					}
				}
			}
		}

		private void ExtractTexts()
		{
			var sourceFiles = new FileEnumerator(The.Workspace.ProjectDirectory) {
				EnumerationFilter = (f) => !f.Path.Contains("/bin/") && !f.Path.Contains("/obj/"),
			};
			using (new DirectoryChanger(The.Workspace.ProjectDirectory)) {
				var files = sourceFiles.Enumerate(".cs");
				foreach (var fileInfo in files) {
					ProcessSourceFile(fileInfo.Path);
				}
			}
			using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
				var files = The.Workspace.AssetFiles.Enumerate(".scene").Concat(The.Workspace.AssetFiles.Enumerate(".tan"));
				foreach (var fileInfo in files) {
					// First of all scan lines like this: "[]..."
					ProcessSourceFile(fileInfo.Path);
					// Then like this: Text "..."
					if (!ShouldLocalizeOnlyTaggedSceneTexts()) {
						ProcessSceneFile(fileInfo.Path);
					}
				}
			}
		}

		private bool ShouldLocalizeOnlyTaggedSceneTexts()
		{
			return The.Workspace.ProjectJson.GetValue("LocalizeOnlyTaggedSceneTexts", false);
		}

		private void ProcessSourceFile(string file)
		{
			const string quotedStringPattern =
				@"(?<prefix>[@$])?""(?<string>[^""\\]*(?:\\.[^""\\]*)*)""";
			var code = File.ReadAllText(file, Encoding.Default);
			var context = GetContext(file);
			foreach (var match in Regex.Matches(code, quotedStringPattern)) {
				var m = match as Match;
				var prefix = m.Groups["prefix"].Value;
				var s = m.Groups["string"].Value;
				if (HasAlphabeticCharacters(s) && IsCorrectTaggedString(s)) {
					if (string.IsNullOrEmpty(prefix)) {
						AddToDictionary(s, context);
					} else {
						var type = prefix == "@" ? "verbatim" : "interpolated";
						Logger.Write($"WARNING: Ignoring {type} string: {s}");
					}
				}
			}
		}

		void ProcessSceneFile(string file)
		{
			const string textPropertiesPattern = @"^(\s*Text)\s""([^""\\]*(?:\\.[^""\\]*)*)""$";
			var code = File.ReadAllText(file, Encoding.Default);
			var context = GetContext(file);
			foreach (var match in Regex.Matches(code, textPropertiesPattern, RegexOptions.Multiline)) {
				var s = ((Match)match).Groups[2].Value;
				if (HasAlphabeticCharacters(s)) {
					AddToDictionary(s, context);
				}
			}
		}

		private string GetContext(string file)
		{
			return file;
		}

		private static bool IsCorrectTaggedString(string str)
		{
			return Regex.Match(str, @"^\[.*\](.+)$").Success;
		}

		private void AddToDictionary(string key, string context)
		{
			var match = Regex.Match(key, @"^\[(.*)\](.*)$");
			if (match.Success) {
				// The line starts with "[...]..."
				var value = Unescape(match.Groups[2].Value);
				if (key.StartsWith("[]")) {
					key = key.Substring(2);
				}
				AddToDictionaryHelper(Unescape(key), value, context);
			} else {
				// The line has no [] prefix, but still should be localized.
				// E.g. most of texts in scene files.
				AddToDictionaryHelper(Unescape(key), Unescape(key), context);
			}
		}

		private static bool HasAlphabeticCharacters(string text)
		{
			return text.Any(c => char.IsLetter(c));
		}

		private void AddToDictionaryHelper(string key, string value, string context)
		{
			var e = dictionary.GetEntry(key);
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

		private static string Unescape(string text)
		{
			return text.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\'", "'");
		}
	}
}
