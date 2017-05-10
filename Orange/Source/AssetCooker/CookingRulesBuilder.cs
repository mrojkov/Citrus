using System;
using System.IO;
using System.Collections.Generic;
using Lime;
using Yuzu;
using Yuzu.Metadata;

namespace Orange
{
	// NB: When packing textures into atlases, Orange chooses texture format with highest value
	// amoung all atlas items.
	public enum PVRFormat
	{
		PVRTC4,
		PVRTC4_Forced,
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

	public enum AtlasOptimization
	{
		Memory,
		DrawCalls
	}

	public struct CookingRules
	{
		public const string MainBundleName = "Main";

		[YuzuRequired]
		public string TextureAtlas;

		[YuzuRequired]
		public bool MipMaps;

		[YuzuRequired]
		public bool HighQualityCompression;

		[YuzuRequired]
		public float TextureScaleFactor;

		[YuzuRequired]
		public PVRFormat PVRFormat;

		[YuzuRequired]
		public DDSFormat DDSFormat;

		public DateTime LastChangeTime;

		[YuzuRequired]
		public string[] Bundles;

		[YuzuRequired]
		public bool Ignore;

		[YuzuRequired]
		public int ADPCMLimit; // Kb

		[YuzuRequired]
		public AtlasOptimization AtlasOptimization;

		[YuzuRequired]
		public AssetAttributes ModelCompressing;

		[YuzuRequired]
		public string AtlasPacker;

		public static CookingRules GetDefault(TargetPlatform platform)
		{
			return new CookingRules {
				TextureAtlas = null,
				MipMaps = false,
				HighQualityCompression = false,
				TextureScaleFactor = 1.0f,
				PVRFormat = platform == TargetPlatform.Android ? PVRFormat.ETC1 : PVRFormat.PVRTC4,
				DDSFormat = DDSFormat.DXTi,
				LastChangeTime = new DateTime(0),
				Bundles = new[] { MainBundleName },
				Ignore = false,
				ADPCMLimit = 100,
				AtlasOptimization = AtlasOptimization.Memory,
				ModelCompressing = AssetAttributes.ZippedDeflate
			};
		}
	}

	public class CookingRulesClass
	{
		public const string MainBundleName = "Main";

		[YuzuRequired]
		public string TextureAtlas;

		[YuzuRequired]
		public bool MipMaps;

		[YuzuRequired]
		public bool HighQualityCompression;

		[YuzuRequired]
		public float TextureScaleFactor;

		[YuzuRequired]
		public PVRFormat PVRFormat;

		[YuzuRequired]
		public DDSFormat DDSFormat;

		public DateTime LastChangeTime;

		[YuzuRequired]
		public string[] Bundle;

		[YuzuRequired]
		public bool Ignore;

		[YuzuRequired]
		public int ADPCMLimit; // Kb

		[YuzuRequired]
		public AtlasOptimization AtlasOptimization;

		[YuzuRequired]
		public AssetAttributes ModelCompressing;

		[YuzuRequired]
		public string AtlasPacker;

		public HashSet<Meta.Item> FieldOverrides;
		public HashSet<Meta.Item> InheritedFieldOverrides;
		private static Meta meta = Meta.Get(typeof (CookingRulesClass), new CommonOptions());

		private static readonly Dictionary<string, Meta.Item> fieldNameToYuzuMetaItemCache =
			new Dictionary<string, Meta.Item>();

		static CookingRulesClass()
		{
			foreach (var item in meta.Items) {
				fieldNameToYuzuMetaItemCache.Add(item.Name, item);
			}
		}

		public void Override(string fieldName)
		{
			FieldOverrides.Add(fieldNameToYuzuMetaItemCache[fieldName]);
		}

		private static Dictionary<TargetPlatform, CookingRulesClass> defaultRules =
			new Dictionary<TargetPlatform, CookingRulesClass>();

		public bool Equal(CookingRules rules)
		{
			var r = TextureAtlas == rules.TextureAtlas &&
			        MipMaps == rules.MipMaps &&
			        HighQualityCompression == rules.HighQualityCompression &&
			        TextureScaleFactor == rules.TextureScaleFactor &&
			        PVRFormat == rules.PVRFormat &&
			        DDSFormat == rules.DDSFormat &&
			        Bundle.Length == rules.Bundles.Length &&
			        Ignore == rules.Ignore &&
			        ADPCMLimit == rules.ADPCMLimit &&
			        AtlasOptimization == rules.AtlasOptimization &&
			        ModelCompressing == rules.ModelCompressing &&
			        AtlasPacker == rules.AtlasPacker;
			if (!r) {
				return false;
			}
			for (int i = 0; i < Bundle.Length; i++) {
				if (Bundle[i] != rules.Bundles[i]) {
					return false;
				}
			}
			return true;
		}

		public static CookingRulesClass GetDefault(TargetPlatform platform)
		{
			if (defaultRules.ContainsKey(platform)) {
				return defaultRules[platform];
			}
			defaultRules.Add(platform, new CookingRulesClass {
				TextureAtlas = null,
				MipMaps = false,
				HighQualityCompression = false,
				TextureScaleFactor = 1.0f,
				PVRFormat = platform == TargetPlatform.Android ? PVRFormat.ETC1 : PVRFormat.PVRTC4,
				DDSFormat = DDSFormat.DXTi,
				LastChangeTime = new DateTime(0),
				Bundle = new[] { MainBundleName },
				Ignore = false,
				ADPCMLimit = 100,
				AtlasOptimization = AtlasOptimization.Memory,
				ModelCompressing = AssetAttributes.ZippedDeflate,
				FieldOverrides = new HashSet<Meta.Item>(),
				InheritedFieldOverrides = new HashSet<Meta.Item>(),
			});
			return defaultRules[platform];
		}

		public CookingRulesClass InheritClone()
		{
			var r = (CookingRulesClass)MemberwiseClone();
			r.FieldOverrides = new HashSet<Meta.Item>();
			r.InheritedFieldOverrides = new HashSet<Meta.Item>(InheritedFieldOverrides);
			r.InheritedFieldOverrides.UnionWith(FieldOverrides);
			return r;
		}
	}

	public class CookingRulesForAllPlatforms
	{
		public Dictionary<TargetPlatform, CookingRulesClass> PlatformRules =
			new Dictionary<TargetPlatform, CookingRulesClass>();

		public Dictionary<string, CookingRulesClass> SubtargetRules = new Dictionary<string, CookingRulesClass>();
		public CookingRulesClass CommonRules;
		public CookingRulesClass ResultingRules;
		public static List<string> KnownBundles = new List<string>();

		public CookingRulesForAllPlatforms(bool initialize = true)
		{
			if (!initialize) {
				return;
			}
			foreach (var platform in (TargetPlatform[])Enum.GetValues(typeof (TargetPlatform))) {
				PlatformRules.Add(platform, CookingRulesClass.GetDefault(platform));
			}
			foreach (var subtarget in The.Workspace.SubTargets) {
				SubtargetRules.Add(subtarget.Name, CookingRulesClass.GetDefault(subtarget.Platform));
			}
			CommonRules = CookingRulesClass.GetDefault(The.Workspace.ActivePlatform);
		}

		public bool Ignore
		{
			get { return ResultingRules.Ignore; }
			set
			{
				foreach (var platform in (TargetPlatform[])Enum.GetValues(typeof (TargetPlatform))) {
					PlatformRules[platform].Ignore = value;
				}
				foreach (var subtarget in The.Workspace.SubTargets) {
					SubtargetRules[subtarget.Name].Ignore = value;
				}
				CommonRules.Ignore = value;
				if (ResultingRules != null) {
					ResultingRules.Ignore = value;
				}
			}
		}

		public CookingRulesForAllPlatforms InheritClone()
		{
			var r = new CookingRulesForAllPlatforms(false);
			foreach (var kv in PlatformRules) {
				r.PlatformRules.Add(kv.Key, kv.Value.InheritClone());
			}
			foreach (var kv in SubtargetRules) {
				r.SubtargetRules.Add(kv.Key, kv.Value.InheritClone());
			}
			if (ResultingRules != null) {
				r.CommonRules = ResultingRules.InheritClone();
				r.ResultingRules = ResultingRules.InheritClone();
			} else {
				r.CommonRules = CommonRules.InheritClone();
			}
			return r;
		}
	}

	public class CookingRulesBuilder
	{
		public static Dictionary<string, CookingRulesForAllPlatforms> BuildWithOverride(FileEnumerator fileEnumerator,
			TargetPlatform platform, string target)
		{
			var shouldRescanEnumerator = false;
			var pathStack = new Stack<string>();
			var rulesStack = new Stack<CookingRulesForAllPlatforms>();
			var map = new Dictionary<string, CookingRulesForAllPlatforms>();
			pathStack.Push("");
			rulesStack.Push(new CookingRulesForAllPlatforms());
			using (new DirectoryChanger(fileEnumerator.Directory)) {
				foreach (var fileInfo in fileEnumerator.Enumerate()) {
					var path = fileInfo.Path;
					while (!path.StartsWith(pathStack.Peek())) {
						rulesStack.Pop();
						pathStack.Pop();
					}
					if (Path.GetFileName(path) == "#CookingRules.txt") {
						pathStack.Push(Lime.AssetPath.GetDirectoryName(path));
						var rules = ParseCookingRulesClass(rulesStack.Peek(), path, platform, target);
						rulesStack.Push(rules);
						// Add 'ignore' cooking rules for this #CookingRules.txt itself
						var ignoreRules = rules.InheritClone();
						ignoreRules.Ignore = true;
						map[path] = ignoreRules;
					} else if (Path.GetExtension(path) != ".txt") {
						var rulesFile = path + ".txt";
						CookingRulesForAllPlatforms rules = rulesStack.Peek();
						if (File.Exists(rulesFile)) {
							rules = ParseCookingRulesClass(rulesStack.Peek(), rulesFile, platform, target);
							// Add 'ignore' cooking rules for this cooking rules text file
							var ignoreRules = rules.InheritClone();
							ignoreRules.Ignore = true;
							map[rulesFile] = ignoreRules;
						}
						if (rules.CommonRules.LastChangeTime > fileInfo.LastWriteTime) {
							File.SetLastWriteTime(path, rules.CommonRules.LastChangeTime);
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

		public static Dictionary<string, CookingRules> Build(FileEnumerator fileEnumerator, TargetPlatform platform,
			string target)
		{
			var shouldRescanEnumerator = false;
			var pathStack = new Stack<string>();
			var rulesStack = new Stack<CookingRules>();
			var map = new Dictionary<string, CookingRules>();
			pathStack.Push("");
			rulesStack.Push(CookingRules.GetDefault(platform));
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

		private static bool ParseBool(string value)
		{
			if (value != "Yes" && value != "No") {
				throw new Lime.Exception("Invalid value. Must be either 'Yes' or 'No'");
			}
			return value == "Yes";
		}

		private static DDSFormat ParseDDSFormat(string value)
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

		private static PVRFormat ParsePVRFormat(string value)
		{
			switch (value) {
				case "":
				case "PVRTC4":
					return PVRFormat.PVRTC4;
				case "PVRTC4_Forced":
					return PVRFormat.PVRTC4_Forced;
				case "PVRTC2":
					return PVRFormat.PVRTC2;
				case "RGBA4":
					return PVRFormat.RGBA4;
				case "RGB565":
					return PVRFormat.RGB565;
				case "ARGB8":
					return PVRFormat.ARGB8;
				case "RGBA8":
					return PVRFormat.ARGB8;
				default:
					throw new Lime.Exception(
						"Error parsing PVR format. Must be one of: PVRTC4, PVRTC4_Forced, PVRTC2, RGBA4, RGB565, ARGB8");
			}
		}

		private static AtlasOptimization ParseAtlasOptimization(string value)
		{
			switch (value) {
				case "":
				case "Memory":
					return AtlasOptimization.Memory;
				case "DrawCalls":
					return AtlasOptimization.DrawCalls;
				default:
					throw new Lime.Exception("Error parsing AtlasOptimization. Must be one of: Memory, DrawCalls");
			}
		}

		private static AssetAttributes ParseModelCompressing(string value)
		{
			switch (value) {
				case "None":
					return AssetAttributes.None;
				case "":
				case "Deflate":
					return AssetAttributes.ZippedDeflate;
				case "LZMA":
					return AssetAttributes.ZippedLZMA;
				default:
					throw new Lime.Exception("Error parsing ModelCompressing. Must be one of: None, Deflate, LZMA");
			}
		}

		private static CookingRulesForAllPlatforms ParseCookingRulesClass(CookingRulesForAllPlatforms basicRules, string path,
			TargetPlatform resultingPlatform, string resultingTarget)
		{
			var rules = basicRules.InheritClone();
			var currentRules = rules.CommonRules;
			try {
				rules.CommonRules.LastChangeTime = File.GetLastWriteTime(path);
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
						// platform-specific cooking rules
						if (words[0].EndsWith(")")) {
							int cut = words[0].IndexOf('(');
							if (cut >= 0) {
								string platformTag = words[0].Substring(cut + 1, words[0].Length - cut - 2);
								words[0] = words[0].Substring(0, cut);
								try {
									var parsedPlatform = ParsePlatform(platformTag);
									currentRules = rules.PlatformRules[parsedPlatform];
								} catch (InvalidPlatformStringException) {
									currentRules = null;
									foreach (var target in The.Workspace.SubTargets) {
										if (platformTag == target.Name) {
											currentRules = rules.SubtargetRules[target.Name];
										}
									}
									if (currentRules == null) {
										throw new Lime.Exception($"Invalid platform or target: {platformTag}");
									}
								}
							}
						} else {
							currentRules = rules.CommonRules;
						}
						ParseRule(currentRules, words, path);
					}
				}
			} catch (Lime.Exception e) {
				throw new Lime.Exception("Syntax error in {0}: {1}", path, e.Message);
			}
			DeduceResultingRules(rules, path, resultingPlatform, resultingTarget);
			return rules;
		}

		private static void DeduceResultingRules(CookingRulesForAllPlatforms rules, string path,
			TargetPlatform resultingPlatform, string resultingTarget)
		{
			rules.ResultingRules = rules.CommonRules.InheritClone();
			var platformRules = rules.PlatformRules[resultingPlatform];
			//foreach (var i in platformRules.InheritedFieldOverrides) {
			//	i.SetValue(rules.ResultingRules, i.GetValue(platformRules));
			//}
			foreach (var i in platformRules.FieldOverrides) {
				i.SetValue(rules.ResultingRules, i.GetValue(platformRules));
			}
			if (!string.IsNullOrEmpty(resultingTarget)) {
				var targetRules = rules.SubtargetRules[resultingTarget];
				//foreach (var i in targetRules.InheritedFieldOverrides) {
				//	i.SetValue(rules.ResultingRules, i.GetValue(targetRules));
				//}
				foreach (var i in targetRules.FieldOverrides) {
					i.SetValue(rules.ResultingRules, i.GetValue(targetRules));
				}
			}
			if (resultingPlatform == TargetPlatform.Android) {
				switch (rules.ResultingRules.PVRFormat) {
					case PVRFormat.PVRTC2:
					case PVRFormat.PVRTC4:
					case PVRFormat.PVRTC4_Forced:
						rules.ResultingRules.PVRFormat = PVRFormat.ETC1;
						break;
				}
			}
		}

		private static void WorkaroundPVRFormatForAndroid(CookingRules rules, TargetPlatform platform)
		{
			if (platform == TargetPlatform.Android) {
				switch (rules.PVRFormat) {
					case PVRFormat.PVRTC2:
					case PVRFormat.PVRTC4:
					case PVRFormat.PVRTC4_Forced:
						rules.PVRFormat = PVRFormat.ETC1;
						break;
				}
			}
		}

		private static void ParseRule(CookingRulesClass rules, IReadOnlyList<string> words, string path)
		{
			switch (words[0]) {
				case "TextureAtlas":
					switch (words[1]) {
						case "None":
							rules.TextureAtlas = null;
							break;
						case "${DirectoryName}":
							string atlasName = Path.GetFileName(Lime.AssetPath.GetDirectoryName(path));
							if (string.IsNullOrEmpty(atlasName)) {
								throw new Lime.Exception(
									"Atlas directory is empty. Choose another atlas name");
							}
							rules.TextureAtlas = atlasName;
							break;
						default:
							rules.TextureAtlas = words[1];
							break;
					}
					break;
				case "MipMaps":
					rules.MipMaps = ParseBool(words[1]);
					break;
				case "HighQualityCompression":
					rules.HighQualityCompression = ParseBool(words[1]);
					break;
				case "PVRFormat":
					rules.PVRFormat = ParsePVRFormat(words[1]);
					break;
				case "DDSFormat":
					rules.DDSFormat = ParseDDSFormat(words[1]);
					break;
				case "Bundle":
					rules.Bundle = new string[words.Count - 1];
					for (var i = 0; i < rules.Bundle.Length; i++) {
						rules.Bundle[i] = ParseBundle(words[i + 1]);
					}
					break;
				case "Ignore":
					rules.Ignore = ParseBool(words[1]);
					break;
				case "ADPCMLimit":
					rules.ADPCMLimit = int.Parse(words[1]);
					break;
				case "TextureScaleFactor":
					rules.TextureScaleFactor = float.Parse(words[1]);
					break;
				case "AtlasOptimization":
					rules.AtlasOptimization = ParseAtlasOptimization(words[1]);
					break;
				case "AtlasPacker":
					rules.AtlasPacker = words[1];
					break;
				case "ModelCompressing":
					rules.ModelCompressing = ParseModelCompressing(words[1]);
					break;
				default:
					throw new Lime.Exception("Unknown attribute {0}", words[0]);
			}
			rules.Override(words[0]);
		}

		private static CookingRules ParseCookingRules(CookingRules basicRules, string path, TargetPlatform platform,
			string target)
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
								switch (words[1]) {
									case "None":
										rules.TextureAtlas = null;
										break;
									case "${DirectoryName}":
										string atlasName = Path.GetFileName(Lime.AssetPath.GetDirectoryName(path));
										if (string.IsNullOrEmpty(atlasName)) {
											throw new Lime.Exception(
												"Atlas directory is empty. Choose another atlas name");
										}
										rules.TextureAtlas = atlasName;
										break;
									default:
										rules.TextureAtlas = words[1];
										break;
								}
								break;
							case "MipMaps":
								rules.MipMaps = ParseBool(words[1]);
								break;
							case "HighQualityCompression":
								rules.HighQualityCompression = ParseBool(words[1]);
								break;
							case "PVRFormat":
								rules.PVRFormat = ParsePVRFormat(words[1]);
								WorkaroundPVRFormatForAndroid(rules, platform);
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
							case "TextureScaleFactor":
								rules.TextureScaleFactor = float.Parse(words[1]);
								break;
							case "AtlasOptimization":
								rules.AtlasOptimization = ParseAtlasOptimization(words[1]);
								break;
							case "AtlasPacker":
								rules.AtlasPacker = words[1];
								break;
							case "ModelCompressing":
								rules.ModelCompressing = ParseModelCompressing(words[1]);
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

		private static string ParseBundle(string word)
		{
			if (word.ToLowerInvariant() == "<default>" || word.ToLowerInvariant() == "data") {
				return CookingRules.MainBundleName;
			} else {
				return word;
			}
		}

		public static TargetPlatform ParsePlatform(string platformString)
		{
			foreach (var v in (TargetPlatform[])Enum.GetValues(typeof (TargetPlatform))) {
				if (Toolbox.GetTargetPlatformString(v) == platformString) {
					return v;
				}
			}
			throw new InvalidPlatformStringException();
		}

		private class InvalidPlatformStringException : SystemException
		{
		}
	}
}

