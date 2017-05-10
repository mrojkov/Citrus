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

	public class ParticularCookingRules
	{
		// NOTE: function `Override` uses the fact that rule name being parsed matches the field name
		// for all fields marked with `YuzuRequired`. So don't rename them or do so with cautiousness.
		// e.g. don't rename `Bundle` to `Bundles`

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

		public DateTime LastChangeTime;

		public HashSet<Meta.Item> FieldOverrides;

		private static readonly Meta meta = Meta.Get(typeof (ParticularCookingRules), new CommonOptions());

		private static readonly Dictionary<string, Meta.Item> fieldNameToYuzuMetaItemCache =
			new Dictionary<string, Meta.Item>();

		private static readonly Dictionary<TargetPlatform, ParticularCookingRules> defaultRules =
			new Dictionary<TargetPlatform, ParticularCookingRules>();

		static ParticularCookingRules()
		{
			foreach (var item in meta.Items) {
				fieldNameToYuzuMetaItemCache.Add(item.Name, item);
			}
		}

		public void Override(string fieldName)
		{
			FieldOverrides.Add(fieldNameToYuzuMetaItemCache[fieldName]);
		}

		public static ParticularCookingRules GetDefault(TargetPlatform platform)
		{
			if (defaultRules.ContainsKey(platform)) {
				return defaultRules[platform];
			}
			defaultRules.Add(platform, new ParticularCookingRules
			{
				TextureAtlas = null,
				MipMaps = false,
				HighQualityCompression = false,
				TextureScaleFactor = 1.0f,
				PVRFormat = platform == TargetPlatform.Android ? PVRFormat.ETC1 : PVRFormat.PVRTC4,
				DDSFormat = DDSFormat.DXTi,
				LastChangeTime = new DateTime(0),
				Bundle = new[] { CookingRulesBuilder.MainBundleName },
				Ignore = false,
				ADPCMLimit = 100,
				AtlasOptimization = AtlasOptimization.Memory,
				ModelCompressing = AssetAttributes.ZippedDeflate,
				FieldOverrides = new HashSet<Meta.Item>(),
			});
			return defaultRules[platform];
		}

		public ParticularCookingRules InheritClone()
		{
			var r = (ParticularCookingRules)MemberwiseClone();
			r.FieldOverrides = new HashSet<Meta.Item>();
			return r;
		}
	}

	public class CookingRules
	{
		public Dictionary<TargetPlatform, ParticularCookingRules> PlatformRules =
			new Dictionary<TargetPlatform, ParticularCookingRules>();

		public Dictionary<string, ParticularCookingRules> TargetRules = new Dictionary<string, ParticularCookingRules>();
		public ParticularCookingRules CommonRules;
		public ParticularCookingRules ResultingRules;
		public static List<string> KnownBundles = new List<string>();

		public string TextureAtlas => ResultingRules.TextureAtlas;
		public bool MipMaps => ResultingRules.MipMaps;
		public bool HighQualityCompression => ResultingRules.HighQualityCompression;
		public float TextureScaleFactor => ResultingRules.TextureScaleFactor;
		public PVRFormat PVRFormat => ResultingRules.PVRFormat;
		public DDSFormat DDSFormat => ResultingRules.DDSFormat;
		public string[] Bundle => ResultingRules.Bundle;
		public int ADPCMLimit => ResultingRules.ADPCMLimit;
		public AtlasOptimization AtlasOptimization => ResultingRules.AtlasOptimization;
		public AssetAttributes ModelCompressing => ResultingRules.ModelCompressing;
		public string AtlasPacker => ResultingRules.AtlasPacker;

		public bool Ignore
		{
			get { return ResultingRules.Ignore; }
			set
			{
				foreach (var platform in (TargetPlatform[])Enum.GetValues(typeof(TargetPlatform))) {
					PlatformRules[platform].Ignore = value;
				}
				foreach (var target in The.Workspace.Targets) {
					TargetRules[target.Name].Ignore = value;
				}
				CommonRules.Ignore = value;
				if (ResultingRules != null) {
					ResultingRules.Ignore = value;
				}
			}
		}

		public CookingRules(bool initialize = true)
		{
			if (!initialize) {
				return;
			}
			foreach (var platform in (TargetPlatform[])Enum.GetValues(typeof (TargetPlatform))) {
				PlatformRules.Add(platform, ParticularCookingRules.GetDefault(platform));
			}
			foreach (var target in The.Workspace.Targets) {
				TargetRules.Add(target.Name, ParticularCookingRules.GetDefault(target.Platform));
			}
			CommonRules = ParticularCookingRules.GetDefault(The.Workspace.ActivePlatform);
		}

		public CookingRules InheritClone()
		{
			var r = new CookingRules(false);
			foreach (var kv in PlatformRules) {
				r.PlatformRules.Add(kv.Key, kv.Value.InheritClone());
			}
			foreach (var kv in TargetRules) {
				r.TargetRules.Add(kv.Key, kv.Value.InheritClone());
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
		public const string MainBundleName = "Main";

		public static Dictionary<string, CookingRules> Build(FileEnumerator fileEnumerator,
			TargetPlatform platform, Target target)
		{
			var targetName = target?.Name;
			var shouldRescanEnumerator = false;
			var pathStack = new Stack<string>();
			var rulesStack = new Stack<CookingRules>();
			var map = new Dictionary<string, CookingRules>();
			pathStack.Push("");
			rulesStack.Push(new CookingRules());
			using (new DirectoryChanger(fileEnumerator.Directory)) {
				foreach (var fileInfo in fileEnumerator.Enumerate()) {
					var path = fileInfo.Path;
					while (!path.StartsWith(pathStack.Peek())) {
						rulesStack.Pop();
						pathStack.Pop();
					}
					if (Path.GetFileName(path) == "#CookingRules.txt") {
						pathStack.Push(Lime.AssetPath.GetDirectoryName(path));
						var rules = ParseCookingRules(rulesStack.Peek(), path, platform, targetName);
						rulesStack.Push(rules);
						// Add 'ignore' cooking rules for this #CookingRules.txt itself
						var ignoreRules = rules.InheritClone();
						ignoreRules.Ignore = true;
						map[path] = ignoreRules;
					} else if (Path.GetExtension(path) != ".txt") {
						var rulesFile = path + ".txt";
						var rules = rulesStack.Peek();
						if (File.Exists(rulesFile)) {
							rules = ParseCookingRules(rulesStack.Peek(), rulesFile, platform, targetName);
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

		private static CookingRules ParseCookingRules(CookingRules basicRules, string path,
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
									foreach (var target in The.Workspace.Targets) {
										if (platformTag == target.Name) {
											currentRules = rules.TargetRules[target.Name];
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

		private static void DeduceResultingRules(CookingRules rules, string path,
			TargetPlatform resultingPlatform, string resultingTarget)
		{
			rules.ResultingRules = rules.CommonRules.InheritClone();
			var platformRules = rules.PlatformRules[resultingPlatform];
			foreach (var i in platformRules.FieldOverrides) {
				i.SetValue(rules.ResultingRules, i.GetValue(platformRules));
			}
			if (!string.IsNullOrEmpty(resultingTarget)) {
				var targetRules = rules.TargetRules[resultingTarget];
				foreach (var i in targetRules.FieldOverrides) {
					i.SetValue(rules.ResultingRules, i.GetValue(targetRules));
				}
			}
			// TODO: implement this workaround in a general way
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

		private static void ParseRule(ParticularCookingRules rules, IReadOnlyList<string> words, string path)
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

		private static string ParseBundle(string word)
		{
			if (word.ToLowerInvariant() == "<default>" || word.ToLowerInvariant() == "data") {
				return MainBundleName;
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

