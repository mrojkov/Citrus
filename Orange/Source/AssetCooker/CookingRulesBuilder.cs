using System;
using System.IO;
using System.Collections.Generic;

namespace Orange
{
	// NB: When packing textures into atlases, Orange chooses texture format with highest value
	// amoung all atlas items.
	public enum PVRFormat
	{
		Compressed,
		RGB565,
		RGBA4,
		ARGB8,
	}

	public enum DDSFormat
	{
		DXTi,
		Uncompressed
	}

	public struct CookingRules
	{
		public const string MainBundleName = "Main";

		public string TextureAtlas;
		public bool MipMaps;
		public PVRFormat PVRFormat;
		public DDSFormat DDSFormat;
		public DateTime LastChangeTime;
		public string[] Bundles;

		public static readonly CookingRules Default = new CookingRules {
			TextureAtlas = null,
			MipMaps = false,
			PVRFormat = PVRFormat.Compressed,
			DDSFormat = DDSFormat.DXTi,
			LastChangeTime = new DateTime(0),
			Bundles = new[] { MainBundleName }
		};
	}
	
	public class CookingRulesBuilder
	{
		public static Dictionary<string, CookingRules> Build(FileEnumerator fileEnumerator)
		{
			var shouldRescanEnumerator = false;
			var pathStack = new Stack<string>();
			var rulesStack = new Stack<CookingRules>();
			var map = new Dictionary<string, CookingRules>();
			pathStack.Push("");
			rulesStack.Push(CookingRules.Default);
			using (new DirectoryChanger(fileEnumerator.Directory)) {
				foreach (var fileInfo in fileEnumerator.Enumerate()) {
					var path = fileInfo.Path;
					while (!path.StartsWith(pathStack.Peek())) {
						rulesStack.Pop();
						pathStack.Pop();
					}
					if (Path.GetFileName(path) == "#CookingRules.txt") {
						pathStack.Push(Lime.AssetPath.GetDirectoryName(path));
						rulesStack.Push(ParseCookingRules(rulesStack.Peek(), path));
					} else if (Path.GetExtension(path) != ".txt") {
						var rules = rulesStack.Peek();
						var rulesFile = path + ".txt";
						if (File.Exists(rulesFile)) {
							rules = ParseCookingRules(rulesStack.Peek(), rulesFile);
						}
						if (rules.LastChangeTime > fileInfo.LastWriteTime) {
							File.SetLastWriteTime(path, rules.LastChangeTime);
							shouldRescanEnumerator = true;
						}
						map[path] = rules;
					}
				}
			}
			if (shouldRescanEnumerator) {
				fileEnumerator.Rescan();
			}
			return map;
		}

		static bool ParseBool(string value)
		{
			if (value != "Yes" && value != "No")
				throw new Lime.Exception("Invalid value. Must be either 'Yes' or 'No'");
			return value == "Yes";
		}

		static DDSFormat ParseDDSFormat(string value)
		{
			switch (value) {
				case "DXTi":
					return DDSFormat.DXTi;
				case "ARGB8":
				case "RGBA8":
					return DDSFormat.Uncompressed;
				default:
					throw new Lime.Exception("Error parsing DDS format. Must be either DXTi or ARGB8");
			}
		}

		static PVRFormat ParsePVRFormat(string value)
		{
			switch (value) {
				case "PVRTC4":
					return PVRFormat.Compressed;
				case "RGBA4":
					return PVRFormat.RGBA4;
				case "RGB565":
					return PVRFormat.RGB565;
				case "ARGB8":
					return PVRFormat.ARGB8;
				case "RGBA8":
					return PVRFormat.ARGB8;
				default:
					throw new Lime.Exception("Error parsing PVR format. Must be one of: PVRTC4, RGBA4, RGB565, ARGB8");
			}
		}

		static CookingRules ParseCookingRules(CookingRules basicRules, string path)
		{ 
			var rules = basicRules;
			try {
				rules.LastChangeTime = File.GetLastWriteTime(path);
				using (var s = new FileStream(path, FileMode.Open)) {
					TextReader r = new StreamReader(s);
					string line;
					while ((line = r.ReadLine()) != null) {
						line = line.Trim();
						if (line == "") {
							continue;
						}
						var words = line.Split(' ');
						if (words.Length < 2) {
							throw new Lime.Exception("Invalid rule format");
						}
						switch (words[0]) {
							case "TextureAtlas":
								if (words[1] == "None")
									rules.TextureAtlas = null;
								else if (words[1] == "${DirectoryName}") {
									string atlasName = Path.GetFileName(Lime.AssetPath.GetDirectoryName(path));
									if (string.IsNullOrEmpty(atlasName)) {
										throw new Lime.Exception("Atlas directory is empty. Choose another atlas name");
									}
									rules.TextureAtlas = atlasName;
								} else {
									rules.TextureAtlas = words[1];
								}
								break;
							case "MipMaps":
								rules.MipMaps = ParseBool(words[1]);
								break;
							case "PVRFormat":
								rules.PVRFormat = ParsePVRFormat(words[1]);
								break;
							case "DDSFormat":
								rules.DDSFormat = ParseDDSFormat(words[1]);
								break;
							case "Bundle":
								rules.Bundles = new string[words.Length - 1];
								for (var i = 0; i < rules.Bundles.Length; i++) {
									rules.Bundles[i] = ParseBundle(words[i + 1]);
								}
								break;
							default:
								throw new Lime.Exception("Unknown attribute {0}", words[0]);
						}
					}
				}
			} catch (Lime.Exception e) {
				throw new Lime.Exception("Syntax error in {0}: {1}", path, e.Message);
			}
			return rules;
		}

		static string ParseBundle(string word)
		{
			if (word.ToLowerInvariant() == "<default>" || word.ToLowerInvariant() == "data") {
				return CookingRules.MainBundleName;
			} else {
				return word;
			}
		}
	}
}

