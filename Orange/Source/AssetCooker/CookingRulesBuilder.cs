using System;
using System.Collections;
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

	public enum ModelCompression
	{
		None,
		Deflate,
		LZMA,
	}

	public interface ICookingRules
	{
		string TextureAtlas { get; }
		bool MipMaps { get; }
		bool HighQualityCompression { get; }
		float TextureScaleFactor { get; }
		PVRFormat PVRFormat { get; }
		DDSFormat DDSFormat { get; }
		string[] Bundle { get; }
		bool Ignore { get; }
		int ADPCMLimit { get; }
		AtlasOptimization AtlasOptimization { get; }
		ModelCompression ModelCompressing { get; }
		string AtlasPacker { get; }
	}

	public class ParticularCookingRules : ICookingRules
	{
		// NOTE: function `Override` uses the fact that rule name being parsed matches the field name
		// for all fields marked with `YuzuRequired`. So don't rename them or do so with cautiousness.
		// e.g. don't rename `Bundle` to `Bundles`

		[YuzuRequired]
		public string TextureAtlas { get; set; }

		[YuzuRequired]
		public bool MipMaps { get; set; }

		[YuzuRequired]
		public bool HighQualityCompression { get; set; }

		[YuzuRequired]
		public float TextureScaleFactor { get; set; }

		[YuzuRequired]
		public PVRFormat PVRFormat { get; set; }

		[YuzuRequired]
		public DDSFormat DDSFormat { get; set; }

		[YuzuRequired]
		public string[] Bundle { get; set; }

		[YuzuRequired]
		public bool Ignore { get; set; }

		[YuzuRequired]
		public int ADPCMLimit { get; set; } // Kb

		[YuzuRequired]
		public AtlasOptimization AtlasOptimization { get; set; }

		[YuzuRequired]
		public ModelCompression ModelCompressing { get; set; }

		[YuzuRequired]
		public string AtlasPacker { get; set; }

		public DateTime LastChangeTime;

		public HashSet<Meta.Item> FieldOverrides;

		public ParticularCookingRules Parent;

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
				ModelCompressing = ModelCompression.Deflate,
				FieldOverrides = new HashSet<Meta.Item>(),
			});
			return defaultRules[platform];
		}

		public ParticularCookingRules InheritClone()
		{
			var r = (ParticularCookingRules)MemberwiseClone();
			r.FieldOverrides = new HashSet<Meta.Item>();
			r.Parent = this;
			return r;
		}
	}

	public class CookingRules : ICookingRules
	{
		public Dictionary<Target, ParticularCookingRules> TargetRules = new Dictionary<Target, ParticularCookingRules>();
		public ParticularCookingRules CommonRules;
		private ParticularCookingRules effectiveRules;
		public CookingRules Parent;
		public static List<string> KnownBundles = new List<string>();

		public string TextureAtlas => effectiveRules.TextureAtlas;
		public bool MipMaps => effectiveRules.MipMaps;
		public bool HighQualityCompression => effectiveRules.HighQualityCompression;
		public float TextureScaleFactor => effectiveRules.TextureScaleFactor;
		public PVRFormat PVRFormat => effectiveRules.PVRFormat;
		public DDSFormat DDSFormat => effectiveRules.DDSFormat;
		public string[] Bundle => effectiveRules.Bundle;
		public int ADPCMLimit => effectiveRules.ADPCMLimit;
		public AtlasOptimization AtlasOptimization => effectiveRules.AtlasOptimization;
		public ModelCompression ModelCompressing => effectiveRules.ModelCompressing;
		public string AtlasPacker => effectiveRules.AtlasPacker;

		public bool Ignore
		{
			get { return effectiveRules.Ignore; }
			set
			{
				foreach (var target in The.Workspace.Targets) {
					TargetRules[target].Ignore = value;
				}
				CommonRules.Ignore = value;
				if (effectiveRules != null) {
					effectiveRules.Ignore = value;
				}
			}
		}

		public IEnumerable<KeyValuePair<Target, ParticularCookingRules>> Enumerate()
		{
			yield return new KeyValuePair<Target, ParticularCookingRules>(null, CommonRules);
			foreach (var kv in TargetRules) {
				yield return kv;
			}
		}

		public CookingRules(bool initialize = true)
		{
			if (!initialize) {
				return;
			}
			foreach (var target in The.Workspace.Targets) {
				TargetRules.Add(target, ParticularCookingRules.GetDefault(target.Platform));
			}
			CommonRules = ParticularCookingRules.GetDefault(The.Workspace.ActivePlatform);
		}

		public CookingRules InheritClone()
		{
			var r = new CookingRules(false);
			r.Parent = this;
			foreach (var kv in TargetRules) {
				r.TargetRules.Add(kv.Key, kv.Value.InheritClone());
			}
			if (effectiveRules != null) {
				r.CommonRules = effectiveRules.InheritClone();
				r.effectiveRules = effectiveRules.InheritClone();
			} else {
				r.CommonRules = CommonRules.InheritClone();
			}
			return r;
		}

		public string SourceFilename;
		public void Save()
		{
			using (var fs = new FileStream(SourceFilename, FileMode.Create)) {
				using (var sw = new StreamWriter(fs)) {
					SaveCookingRules(sw, CommonRules, null);
					foreach (var kv in TargetRules) {
						SaveCookingRules(sw, kv.Value, kv.Key);
					}
				}
			}
		}

		private string StringForValue(Meta.Item yi, object value)
		{
			if (value is bool) {
				return (bool)value ? "Yes" : "No";
			} else if (value is DDSFormat) {
				return (DDSFormat)value == DDSFormat.DXTi ? "DXTi" : "RGBA8";
			} else if (yi.Name == "TextureAtlas") {
				var fi = new System.IO.FileInfo(SourceFilename);
				if (fi.Directory.Name == value.ToString()) {
					return "${DirectoryName}";
				} else {
					return value.ToString();
				}
			} else {
				return value.ToString();
			}
		}

		public void SaveCookingRules(StreamWriter sw, ParticularCookingRules rules, Target target)
		{
			var targetString = target == null ? "" : $"({target.Name})";
			foreach (var yi in rules.FieldOverrides) {
				var value = yi.GetValue(rules);
				var valueString = StringForValue(yi, value);
				sw.Write($"{yi.Name}{targetString} {valueString}\n");
			}
		}

		public void DeduceEffectiveRules(string path, Target target)
		{
			effectiveRules = CommonRules.InheritClone();
			var targetRules = TargetRules[target];
			foreach (var i in targetRules.FieldOverrides) {
				i.SetValue(effectiveRules, i.GetValue(targetRules));
			}
			// TODO: implement this workaround in a general way
			if (target.Platform == TargetPlatform.Android) {
				switch (effectiveRules.PVRFormat) {
					case PVRFormat.PVRTC2:
					case PVRFormat.PVRTC4:
					case PVRFormat.PVRTC4_Forced:
						effectiveRules.PVRFormat = PVRFormat.ETC1;
						break;
				}
			}
		}
	}

	public class CookingRulesBuilder
	{
		public const string MainBundleName = "Main";

		public static Dictionary<string, CookingRules> Build(IFileEnumerator fileEnumerator, Target target)
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
						var rules = ParseCookingRules(rulesStack.Peek(), path, target);
						rules.SourceFilename = Path.Combine(fileEnumerator.Directory, path);
						rulesStack.Push(rules);
						// Add 'ignore' cooking rules for this #CookingRules.txt itself
						var ignoreRules = rules.InheritClone();
						ignoreRules.Ignore = true;
						map[path] = ignoreRules;
					} else if (Path.GetExtension(path) != ".txt") {
						var rulesFile = path + ".txt";
						var rules = rulesStack.Peek();
						if (File.Exists(rulesFile)) {
							rules = ParseCookingRules(rulesStack.Peek(), rulesFile, target);
							rules.SourceFilename = Path.Combine(fileEnumerator.Directory, rulesFile);
							// Add 'ignore' cooking rules for this cooking rules text file
							var ignoreRules = rules.InheritClone();
							ignoreRules.Ignore = true;
							map[rulesFile] = ignoreRules;
						}
						if (rules.CommonRules.LastChangeTime > fileInfo.LastWriteTime) {
							try {
								File.SetLastWriteTime(path, rules.CommonRules.LastChangeTime);
							} catch (UnauthorizedAccessException e) {
								// In case this is a folder
							}
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

		private static ModelCompression ParseModelCompressing(string value)
		{
			switch (value) {
				case "None":
					return ModelCompression.None;
				case "":
				case "Deflate":
					return ModelCompression.Deflate;
				case "LZMA":
					return ModelCompression.LZMA;
				default:
					throw new Lime.Exception("Error parsing ModelCompressing. Must be one of: None, Deflate, LZMA");
			}
		}

		private static CookingRules ParseCookingRules(CookingRules basicRules, string path, Target target)
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
						// target-specific cooking rules
						if (words[0].EndsWith(")")) {
							int cut = words[0].IndexOf('(');
							if (cut >= 0) {
								string targetName = words[0].Substring(cut + 1, words[0].Length - cut - 2);
								words[0] = words[0].Substring(0, cut);
								currentRules = null;
								foreach (var t in The.Workspace.Targets) {
									if (targetName == t.Name) {
										currentRules = rules.TargetRules[t];
									}
								}
								if (currentRules == null) {
									throw new Lime.Exception($"Invalid platform or target: {targetName}");
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
			rules.DeduceEffectiveRules(path, target);
			return rules;
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
	}
}

