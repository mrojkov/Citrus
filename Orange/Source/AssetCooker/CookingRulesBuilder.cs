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
		ETC2,
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
		string CustomRule { get; }
		TextureWrapMode WrapMode { get; }
		TextureFilter MinFilter { get; }
		TextureFilter MagFilter { get; }
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

		[YuzuRequired]
		public TextureWrapMode WrapMode { get; set; }

		[YuzuRequired]
		public TextureFilter MinFilter { get; set; }

		[YuzuRequired]
		public TextureFilter MagFilter { get; set; }

		[YuzuRequired]
		public string CustomRule { get; set; }

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
				PVRFormat = platform == TargetPlatform.Android ? PVRFormat.ETC2 : PVRFormat.PVRTC4,
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
		public string SourceFilename;
		public Dictionary<Target, ParticularCookingRules> TargetRules = new Dictionary<Target, ParticularCookingRules>();
		public ParticularCookingRules CommonRules;
		public CookingRules Parent;
		public static List<string> KnownBundles = new List<string>();

		public string TextureAtlas => EffectiveRules.TextureAtlas;
		public bool MipMaps => EffectiveRules.MipMaps;
		public bool HighQualityCompression => EffectiveRules.HighQualityCompression;
		public float TextureScaleFactor => EffectiveRules.TextureScaleFactor;
		public PVRFormat PVRFormat => EffectiveRules.PVRFormat;
		public DDSFormat DDSFormat => EffectiveRules.DDSFormat;
		public string[] Bundle => EffectiveRules.Bundle;
		public int ADPCMLimit => EffectiveRules.ADPCMLimit;
		public AtlasOptimization AtlasOptimization => EffectiveRules.AtlasOptimization;
		public ModelCompression ModelCompressing => EffectiveRules.ModelCompressing;
		public string AtlasPacker => EffectiveRules.AtlasPacker;
		public TextureWrapMode WrapMode => EffectiveRules.WrapMode;
		public TextureFilter MinFilter => EffectiveRules.MinFilter;
		public TextureFilter MagFilter => EffectiveRules.MagFilter;
		public string CustomRule => EffectiveRules.CustomRule;

		public bool Ignore
		{
			get { return EffectiveRules.Ignore; }
			set
			{
				foreach (var target in The.Workspace.Targets) {
					TargetRules[target].Ignore = value;
				}
				CommonRules.Ignore = value;
				if (EffectiveRules != null) {
					EffectiveRules.Ignore = value;
				}
			}
		}

		public ParticularCookingRules EffectiveRules { get; private set; }

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
			if (EffectiveRules != null) {
				r.CommonRules = EffectiveRules.InheritClone();
				r.EffectiveRules = EffectiveRules.InheritClone();
			} else {
				r.CommonRules = CommonRules.InheritClone();
			}
			return r;
		}

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

		public string FieldValueToString(Meta.Item yi, object value)
		{
			if (value == null) {
				return "";
			} if (yi.Name == "Bundle") {
				var vlist = (string[])value;
				return string.Join(",", vlist);
			} else if (value is bool) {
				return (bool)value ? "Yes" : "No";
			} else if (value is DDSFormat) {
				return (DDSFormat)value == DDSFormat.DXTi ? "DXTi" : "RGBA8";
			} else if (yi.Name == "TextureAtlas") {
				var fi = new System.IO.FileInfo(SourceFilename);
				var atlasName = fi.DirectoryName.Substring(The.Workspace.AssetsDirectory.Length).Replace('\\', '#');
				if (!atlasName.StartsWith("#")) {
					atlasName = "#" + atlasName;
				}
				if (atlasName == value.ToString()) {
					return CookingRulesBuilder.DirectoryNameToken;
				} else {
					return value.ToString();
				}
			} else {
				return value.ToString();
			}
		}

		private void SaveCookingRules(StreamWriter sw, ParticularCookingRules rules, Target target)
		{
			var targetString = target == null ? "" : $"({target.Name})";
			foreach (var yi in rules.FieldOverrides) {
				var value = yi.GetValue(rules);
				var valueString = FieldValueToString(yi, value);
				if (!string.IsNullOrEmpty(valueString)) {
					sw.Write($"{yi.Name}{targetString} {valueString}\n");
				}
			}
		}

		public void DeduceEffectiveRules(Target target)
		{
			EffectiveRules = CommonRules.InheritClone();
			if (target != null) {
				var targetRules = TargetRules[target];
				foreach (var i in targetRules.FieldOverrides) {
					i.SetValue(EffectiveRules, i.GetValue(targetRules));
				}
				// TODO: implement this workaround in a general way
				if (target.Platform == TargetPlatform.Android) {
					switch (EffectiveRules.PVRFormat) {
						case PVRFormat.PVRTC2:
						case PVRFormat.PVRTC4:
						case PVRFormat.PVRTC4_Forced:
							EffectiveRules.PVRFormat = PVRFormat.ETC2;
							break;
					}
				}
				if (EffectiveRules.WrapMode != TextureWrapMode.Clamp) {
					EffectiveRules.TextureAtlas = null;
				}
			}
		}

		public bool HasOverrides()
		{
			var r = false;
			r = r || CommonRules.FieldOverrides.Count > 0;
			foreach (var cr in TargetRules) {
				r = r || cr.Value.FieldOverrides.Count > 0;
			}
			return r;
		}
	}

	public class CookingRulesBuilder
	{
		public const string MainBundleName = "Main";
		public const string CookingRulesFilename = "#CookingRules.txt";
		public const string DirectoryNameToken = "${DirectoryName}";

		public static Dictionary<string, CookingRules> Build(IFileEnumerator fileEnumerator, Target target)
		{
			var shouldRescanEnumerator = false;
			var pathStack = new Stack<string>();
			var rulesStack = new Stack<CookingRules>();
			var map = new Dictionary<string, CookingRules>();
			pathStack.Push("");
			var rootRules = new CookingRules();
			rootRules.DeduceEffectiveRules(target);
			rulesStack.Push(rootRules);
			using (new DirectoryChanger(fileEnumerator.Directory)) {
				foreach (var fileInfo in fileEnumerator.Enumerate()) {
					var path = fileInfo.Path;
					while (!path.StartsWith(pathStack.Peek())) {
						rulesStack.Pop();
						pathStack.Pop();
					}
					if (Path.GetFileName(path) == CookingRulesFilename) {
						var dirName = Lime.AssetPath.GetDirectoryName(path);
						pathStack.Push(dirName == string.Empty ? "" : dirName + "/");
						var rules = ParseCookingRules(rulesStack.Peek(), path, target);
						rules.SourceFilename = AssetPath.Combine(fileEnumerator.Directory, path);
						rulesStack.Push(rules);
						// Add 'ignore' cooking rules for this #CookingRules.txt itself
						var ignoreRules = rules.InheritClone();
						ignoreRules.Ignore = true;
						map[path] = ignoreRules;
						var directoryName = pathStack.Peek();
						if (!string.IsNullOrEmpty(directoryName)) {
							directoryName = directoryName.Remove(directoryName.Length - 1);
							// it is possible for map to not contain this directoryName since not every IFileEnumerator enumerates directories
							if (map.ContainsKey(directoryName)) {
								map[directoryName] = rules;
							}
						}
					} else  {
						if (Path.GetExtension(path) == ".txt") {
							var filename = path.Remove(path.Length - 4);
							if (File.Exists(filename) || Directory.Exists(filename)) {
								continue;
							}
						}
						var rulesFile = path + ".txt";
						var rules = rulesStack.Peek();
						if (File.Exists(rulesFile)) {
							rules = ParseCookingRules(rulesStack.Peek(), rulesFile, target);
							rules.SourceFilename = AssetPath.Combine(fileEnumerator.Directory, rulesFile);
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
				if (!Path.IsPathRooted(path)) {
					path = Path.Combine(Directory.GetCurrentDirectory(), path);
				}
				throw new Lime.Exception("Syntax error in {0}: {1}", path, e.Message);
			}
			rules.DeduceEffectiveRules(target);
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
				case DirectoryNameToken:
					string atlasName = "#" + Lime.AssetPath.GetDirectoryName(path).Replace('/', '#');
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
			case "CustomRule":
				rules.CustomRule = words[1];
				break;
			case "WrapMode":
				rules.WrapMode = ParseWrapMode(words[1]);
				break;
			case "MinFilter":
				rules.MinFilter = ParseTextureFilter(words[1]);
				break;
			case "MagFilter":
				rules.MagFilter = ParseTextureFilter(words[1]);
				break;
			default:
				throw new Lime.Exception("Unknown attribute {0}", words[0]);
			}
			rules.Override(words[0]);
		}

		private static TextureFilter ParseTextureFilter(string value)
		{
			switch (value) {
			case "":
			case "Linear":
				return TextureFilter.Linear;
			case "Nearest":
				return TextureFilter.Nearest;
			default:
				throw new Lime.Exception("Error parsing TextureFtiler. Must be one of: Linear, Nearest");
			}
		}

		private static TextureWrapMode ParseWrapMode(string value)
		{
			switch (value) {
			case "":
			case "Clamp":
				return TextureWrapMode.Clamp;
			case "Repeat":
				return TextureWrapMode.Repeat;
			case "MirroredRepeat":
				return TextureWrapMode.MirroredRepeat;
			default:
				throw new Lime.Exception("Error parsing AtlasOptimization. Must be one of: Memory, DrawCalls");
			}
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

