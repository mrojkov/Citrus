using System;
using System.IO;
using System.Collections.Generic;

namespace Orange
{
	// NB: When packing textures into atlases, Orange chooses texture format with highest value
	// amoung all atlas items.
	public enum PVRFormat
	{
		PVRTC4,
		PVRTC2,
		ETC1,
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
		public bool Ignore;
		public int ADPCMLimit; // Kb

		public static readonly CookingRules Default = new CookingRules {
			TextureAtlas = null,
			MipMaps = false,
			PVRFormat = PVRFormat.PVRTC4,
			DDSFormat = DDSFormat.DXTi,
			LastChangeTime = new DateTime(0),
			Bundles = new[] { MainBundleName },
			Ignore = false,
			ADPCMLimit = 100,
		};
	}
	
	public class CookingRulesBuilder
	{
		public static Dictionary<string, CookingRules> Build(FileEnumerator fileEnumerator, TargetPlatform platform, string target)
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
						var rules = ParseCookingRules(rulesStack.Peek(), path, platform, target);
						rulesStack.Push(rules);
						// Add 'ignore' cooking rules for this #CookingRules.txt itself
						var ignoreRules = rules;
						ignoreRules.Ignore = true;
						map[path] = ignoreRules;
					} else if (Path.GetExtension(path) != ".txt") {
						var rules = rulesStack.Peek();
						var rulesFile = path + ".txt";
						if (File.Exists(rulesFile)) {
							rules = ParseCookingRules(rulesStack.Peek(), rulesFile, platform, target);
							// Add 'ignore' cooking rules for this cooking rules text file
							var ignoreRules = rules;
							ignoreRules.Ignore = true;
							map[rulesFile] = ignoreRules;
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

		static PVRFormat ParsePVRFormat(string value, TargetPlatform platform)
		{
			switch (value) {
				case "":
				case "PVRTC4":
					return (platform == TargetPlatform.Android) ? PVRFormat.ETC1 : PVRFormat.PVRTC4;
				case "PVRTC2":
					return (platform == TargetPlatform.Android) ? PVRFormat.ETC1 : PVRFormat.PVRTC2;
				case "RGBA4":
					return PVRFormat.RGBA4;
				case "RGB565":
					return PVRFormat.RGB565;
				case "ARGB8":
					return PVRFormat.ARGB8;
				case "RGBA8":
					return PVRFormat.ARGB8;
				default:
					throw new Lime.Exception("Error parsing PVR format. Must be one of: PVRTC4, PVRTC2, RGBA4, RGB565, ARGB8");
			}
		}

		static CookingRules ParseCookingRules(CookingRules basicRules, string path, TargetPlatform platform, string target)
		{ 
			var rules = basicRules;
			try {
				rules.LastChangeTime = File.GetLastWriteTime(path);
				using (var s = new FileStream(path, FileMode.Open)) {
					TextReader r = new StreamReader(s);
					string line;
					string currentTarget = null;
					while ((line = r.ReadLine()) != null) {
						line = line.Trim();
						if (line == "") {
							continue;
						}
						if (line[0] == '[') {
							currentTarget = line.Split('[', ']')[1];
							continue;
						}
						if (currentTarget != null && currentTarget != target) {
							continue;
						}
						var words = line.Split(' ');
						if (words.Length < 2) {
							throw new Lime.Exception("Invalid rule format");
						}
						// platform-specific cooking rules
						if (words[0].EndsWith(")")) {
							int cut = words[0].LastIndexOf('(');
							if (cut >= 0) {
								string platformTag = words[0].Substring(cut + 1, words[0].Length - cut - 2);
								words[0] = words[0].Substring(0, cut);
								if (platformTag != Toolbox.GetTargetPlatformString(platform)) {
									continue;
								}
							}
						}
						switch (words[0]) {
							case "TextureAtlas":
								if (platform == TargetPlatform.UltraCompression)
									// Disable atlases for now, since they could make decompression more jerky and increase memory footprint.
									rules.TextureAtlas = null;
								else if (words[1] == "None")
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
								rules.PVRFormat = ParsePVRFormat(words[1], platform);
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
							case "Ignore":
								rules.Ignore = ParseBool(words[1]);
								break;
							case "ADPCMLimit":
								rules.ADPCMLimit = int.Parse(words[1]);
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

