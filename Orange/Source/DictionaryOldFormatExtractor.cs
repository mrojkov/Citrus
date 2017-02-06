#if ORANGE_GUI
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
			CollectExtractedStrings,
			ExtractTaggedStrings,
			TagUntaggedStrings
		}

		public class StringInfo
		{
			public string Key;
			public string Text;
			public bool Allow;
			public HashSet<string> Sources = new HashSet<string>();
		}

		LocalizationPass currentPass;
		string currentSource;
		Dictionary<string, StringInfo> extractedStrings;

		public void ExtractDictionary()
		{
			const string dictionary = "Dictionary.txt";
			LoadDictionary(dictionary);
			extractedStrings = new Dictionary<string, StringInfo>();
			RunLocalizationPass(LocalizationPass.CollectExtractedStrings);
			var stringsArray = new StringInfo[extractedStrings.Count];
			extractedStrings.Values.CopyTo(stringsArray, 0);
			Array.Sort(stringsArray, (item1, item2) => { return item1.Key.CompareTo(item2.Key); });
			var window = new DictionaryExtractorWindow(stringsArray);
			window.OnGo = () => {
				DateTime startTime = DateTime.Now;
				LoadDictionary(dictionary);
				RunLocalizationPass(LocalizationPass.ExtractTaggedStrings);
				RunLocalizationPass(LocalizationPass.TagUntaggedStrings);
				SaveDictionary(dictionary);
				var endTime = DateTime.Now;
				var delta = endTime - startTime;
				Console.WriteLine("Elapsed time {0}:{1}:{2}", delta.Hours, delta.Minutes, delta.Seconds);
			};
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

		private bool NeedProcessFile(string path)
		{
			return !path.StartsWith("TravelMatch.Unity/Assets");
		}

		private void RunLocalizationPass(LocalizationPass pass)
		{
			currentPass = pass;
			Console.WriteLine("=== Running localization pass " + pass.ToString() + " ===");
			var sourceFiles = new FileEnumerator(The.Workspace.ProjectDirectory);
			using (new DirectoryChanger(The.Workspace.ProjectDirectory)) {
				var files = sourceFiles.Enumerate(".cs");
				foreach (var fileInfo in files) {
					if (!NeedProcessFile(fileInfo.Path))
						continue;
					Console.WriteLine("* " + fileInfo.Path);
					ProcessSourceFile(fileInfo.Path, pass, Encoding.UTF8);
				}
			}
			using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
				var files = The.Workspace.AssetFiles.Enumerate(".scene");
				foreach (var fileInfo in files) {
					if (!NeedProcessFile(fileInfo.Path))
						continue;
					Console.WriteLine("* " + fileInfo.Path);
					// Сначала прогоним все строки вида: "[]blah-blah.."
					ProcessSourceFile(fileInfo.Path, pass, Encoding.Default);
					// Затем прогоним все строки вида: Text "blah-blah.."
					if (!ShouldLocalizeOnlyTaggedSceneTexts()) {
						ProcessSceneFile(fileInfo.Path, pass);
					}
				}
			}
			// Gummy Drop extension
			using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
				var files = The.Workspace.AssetFiles.Enumerate(".txt");
				foreach (var fileInfo in files) {
					if (!NeedProcessFile(fileInfo.Path))
						continue;
					if (Path.GetDirectoryName(fileInfo.Path) == "Levels") {
						Console.WriteLine("* " + fileInfo.Path);
						ProcessGummyDropLevelFile(fileInfo.Path, pass, Encoding.UTF8);
					}
				}
			}
		}

		private bool ShouldLocalizeOnlyTaggedSceneTexts()
		{
			return The.Workspace.ProjectJson.GetValue("LocalizeOnlyTaggedSceneTexts", false);
		}

		void ProcessSourceFile(string file, LocalizationPass pass, Encoding saveEncoding)
		{
			currentSource = file;
			const string quotedStringPattern = @"""([^""\\]*(?:\\.[^""\\]*)*)""";
			var originalCode = File.ReadAllText(file);
			var processedCode = Regex.Replace(originalCode, quotedStringPattern,
				(match) => {
					string s = match.Groups[1].Value;
					if (pass != LocalizationPass.ExtractTaggedStrings || IsStringTagged(s)) {
						s = ProcessTextLine(s, processStringsWithoutBrackets: false);
					}
					return '"' + s + '"';
				});
			if (pass != LocalizationPass.CollectExtractedStrings && processedCode != originalCode) {
				File.WriteAllText(file, processedCode, saveEncoding);
			}
		}

		static List<string> ReadStrings(string file)
		{
			List<string> result = new List<string>();
			using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
				using (var r = new StreamReader(stream)) {
					while (true) {
						string line = r.ReadLine();
						if (line == null) {
							break;
						}
						result.Add(line);
					}
				}
			}
			return result;
		}

		static void WriteStrings(string file, List<string> strings)
		{
			List<string> result = new List<string>();
			using (var stream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) {
				using (var w = new StreamWriter(stream)) {
					foreach(var str in strings) {
						w.WriteLine(str);
					}
				}
			}
		}

		void ProcessGummyDropLevelFile(string file, LocalizationPass pass, Encoding saveEncoding)
		{
			if (pass == LocalizationPass.ExtractTaggedStrings) {
				return;
			}
			currentSource = file;
			bool changed = false;
			var lines = ReadStrings(file);
			for (int i = 0; i < lines.Count; i++) {
				string line = lines[i];
				int pos = line.IndexOf(": []");
				if (pos > 0) {
					string value = line.Substring(pos + 4);
					if (HasAlphabeticCharacters(value)) {
						var key = GenerateTagForText(Unescape(value));
						if (AddTextToDictionary(key, Unescape(value))) {
							lines[i] = line.Substring(0, pos) + string.Format(": [{0}]{1}", key, value);
							changed = true;
						}
					}
				}
			}
			if (pass != LocalizationPass.CollectExtractedStrings && changed) {
				WriteStrings(file, lines);
			}
		}

		void ProcessSceneFile(string file, LocalizationPass pass)
		{
			currentSource = file;
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
			if (pass != LocalizationPass.CollectExtractedStrings && originalCode != processedCode) {
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
						if (AddTextToDictionary(key, Unescape(value))) {
							text = string.Format("[{0}]{1}", key, value);
						}
					}
				}
			} else if (processStringsWithoutBrackets) {
				if (HasAlphabeticCharacters(text)) {
					// The line has no [] prefix, but still should be localized.
					// E.g. most of texts in scene files.
					var key = GenerateTagForText(Unescape(text));
					if (AddTextToDictionary(key, Unescape(text))) {
						text = string.Format("[{0}]{1}", key, text);
					}
				}
			}
			return text;
		}

		private bool HasAlphabeticCharacters(string text)
		{
			return text.Any(c => char.IsLetter(c));
		}

		private bool AddTextToDictionary(string key, string value)
		{
			if (currentPass != LocalizationPass.CollectExtractedStrings) {
				StringInfo info;
				if (!extractedStrings.TryGetValue(value, out info))
					return false;
				if (!info.Allow)
					return false;
			}
			var e = Localization.Dictionary.GetEntry(key);
			e.Text = value;
			e.Context = null;
			if (currentPass == LocalizationPass.CollectExtractedStrings) {
				StringInfo info;
				if (!extractedStrings.TryGetValue(value, out info)) {
					info = new StringInfo();
					info.Key = key;
					info.Text = value;
					info.Allow = false;
					extractedStrings.Add(value, info);
				}
				info.Sources.Add(currentSource);
			}
			return true;
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
#endif // ORANGE_GUI
