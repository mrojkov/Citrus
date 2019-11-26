using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Lime;

namespace Orange.Source.Actions
{
	static partial class Actions
	{
		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Convert Dictionary.txt to the New Format")]
		public static void ConvertDictionaryTxt()
		{
			LoadDictionary("Dictionary.txt");
			Run();
			SaveDictionary("Dictionary.Converted.txt");
		}

		private static void SaveDictionary(string dictionary)
		{
			using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
				var dictionaryPath = dictionary;
				if (Directory.Exists(Localization.DictionariesPath)) {
					dictionaryPath = Path.Combine(Localization.DictionariesPath, dictionary);
				}
				using (var stream = new FileStream(dictionary, FileMode.Create)) {
					Localization.Dictionary.WriteToStream(stream);
				}
			}
		}

		private static void LoadDictionary(string dictionary)
		{
			Localization.Dictionary.Clear();
			using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
				var dictionaryPath = Path.Combine(Localization.DictionariesPath, dictionary);
				if (!File.Exists(dictionaryPath)) {
					// using legacy dictionary path
					dictionaryPath = dictionary;
				}
				if (File.Exists(dictionary)) {
					using (var stream = new FileStream(dictionary, FileMode.Open)) {
						Localization.Dictionary.ReadFromStream(stream);
					}
				}
			}
		}

		private static void Run()
		{
			var sourceFiles = new FileEnumerator(The.Workspace.ProjectDirectory);
			using (new DirectoryChanger(The.Workspace.ProjectDirectory)) {
				var files = sourceFiles.Enumerate(".cs");
				foreach (var fileInfo in files) {
					Console.WriteLine("* " + fileInfo.Path);
					ProcessSourceFile(fileInfo.Path);
				}
			}
			using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
				var files = The.Workspace.AssetFiles.Enumerate(".tan");
				foreach (var fileInfo in files) {
					Console.WriteLine("* " + fileInfo.Path);
					ProcessSourceFile(fileInfo.Path);
				}
			}
		}

		private static void ProcessSourceFile(string file)
		{
			const string quotedStringPattern = @"""([^""\\]*(?:\\.[^""\\]*)*)""";
			var code = File.ReadAllText(file, Encoding.Default);
			var matches = Regex.Matches(code, quotedStringPattern);
			foreach (var match in matches) {
				string s = ((Match)match).Groups[1].Value;
				ProcessTextLine(s);
			}
		}

		private static void ProcessTextLine(string text)
		{
			var match = Regex.Match(text, @"^\[(\d*)\](.*)$");
			if (!match.Success || match.Groups[1].Length == 0) {
				return;
			}
			var key = match.Groups[1].Value;
			LocalizationEntry value;
			if (Localization.Dictionary.TryGetValue(key, out value)) {
				Localization.Dictionary.Remove(key);
				Localization.Dictionary.Add(text, value);
			}
		}
	}
}
