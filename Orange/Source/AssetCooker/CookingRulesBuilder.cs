using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Lime;
using Yuzu;
using Yuzu.Json;
using Yuzu.Metadata;

namespace Orange
{
	// NB: When packing textures into atlases, Orange chooses texture format with highest value
	// among all atlas items.
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

	// Specifying empty target platforms for single cooking rule means no platform must be specified in cooking rules.
	public class TargetPlatformsAttribute : Attribute
	{
		public readonly TargetPlatform[] TargetPlatforms;

		public TargetPlatformsAttribute(params TargetPlatform []TargetPlatforms)
		{
			this.TargetPlatforms = TargetPlatforms;
		}
	}

	public interface ICookingRules
	{
		string TextureAtlas { get; }
		bool MipMaps { get; }
		bool HighQualityCompression { get; }
		bool GenerateOpacityMask { get; }
		float TextureScaleFactor { get; }
		PVRFormat PVRFormat { get; }
		DDSFormat DDSFormat { get; }
		string[] Bundles { get; }
		bool Ignore { get; }
		int ADPCMLimit { get; }
		AtlasOptimization AtlasOptimization { get; }
		ModelCompression ModelCompression { get; }
		string AtlasPacker { get; }
		string CustomRule { get; }
		[TargetPlatforms]
		TextureWrapMode WrapMode { get; }
		[TargetPlatforms]
		TextureFilter MinFilter { get; }
		[TargetPlatforms]
		TextureFilter MagFilter { get; }
		int AtlasItemPadding { get; }
	}

	public class ParticularCookingRules : ICookingRules
	{
		// NOTE: function `Override` uses the fact that rule name being parsed matches the field name
		// for all fields marked with `YuzuMember`. So don't rename them or do so with cautiousness.
		// e.g. don't rename `Bundle` to `Bundles`

		[YuzuMember]
		public string TextureAtlas { get; set; }

		[YuzuMember]
		public bool MipMaps { get; set; }

		[YuzuMember]
		public bool HighQualityCompression { get; set; }

		[YuzuMember]
		public bool GenerateOpacityMask { get; set; }

		[YuzuMember]
		public float TextureScaleFactor { get; set; }

		[YuzuMember]
		public PVRFormat PVRFormat { get; set; }

		[YuzuMember]
		public DDSFormat DDSFormat { get; set; }

		[YuzuMember]
		public string[] Bundles { get; set; }

		[YuzuMember]
		public bool Ignore { get; set; }

		[YuzuMember]
		public int ADPCMLimit { get; set; } // Kb

		[YuzuMember]
		public AtlasOptimization AtlasOptimization { get; set; }

		[YuzuMember]
		public ModelCompression ModelCompression { get; set; }

		[YuzuMember]
		public string AtlasPacker { get; set; }

		[YuzuMember]
		public TextureWrapMode WrapMode { get; set; }

		[YuzuMember]
		public TextureFilter MinFilter { get; set; }

		[YuzuMember]
		public TextureFilter MagFilter { get; set; }

		[YuzuMember]
		public string CustomRule { get; set; }

		[YuzuMember]
		public int AtlasItemPadding { get; set; } = 1;

		private static System.Security.Cryptography.SHA1 sha1 = System.Security.Cryptography.SHA1.Create();
		// using json format for SHA1 since binary one includes all fields definitions header anyway.
		// so adding a field with binary triggers rebuild of all bundles
		private static Yuzu.Json.JsonSerializer yjs = new Yuzu.Json.JsonSerializer();

		public byte[] SHA1 { get { return sha1.ComputeHash(Encoding.UTF8.GetBytes(yjs.ToString(this).ToLower())); } }

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
			// initializing all fields here, so any changes to yuzu default values won't affect us here
			yjs.JsonOptions = new JsonSerializeOptions {
				ArrayLengthPrefix = false,
				ClassTag = "class",
				DateFormat = "O",
				TimeSpanFormat = "c",
				DecimalAsString = false,
				EnumAsString = false,
				FieldSeparator = "",
				IgnoreCompact = false,
				Indent = "",
				Int64AsString = false,
				MaxOnelineFields = 0,
				SaveRootClass = false,
				Unordered = false,
			};
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
				GenerateOpacityMask = false,
				TextureScaleFactor = 1.0f,
				PVRFormat = platform == TargetPlatform.Android ? PVRFormat.ETC2 : PVRFormat.PVRTC4,
				DDSFormat = DDSFormat.DXTi,
				LastChangeTime = new DateTime(0),
				Bundles = new[] { CookingRulesBuilder.MainBundleName },
				Ignore = false,
				ADPCMLimit = 100,
				AtlasOptimization = AtlasOptimization.Memory,
				ModelCompression = ModelCompression.Deflate,
				FieldOverrides = new HashSet<Meta.Item>(),
				AtlasItemPadding = 1,
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
		public bool GenerateOpacityMask => EffectiveRules.GenerateOpacityMask;
		public float TextureScaleFactor => EffectiveRules.TextureScaleFactor;
		public PVRFormat PVRFormat => EffectiveRules.PVRFormat;
		public DDSFormat DDSFormat => EffectiveRules.DDSFormat;
		public string[] Bundles => EffectiveRules.Bundles;
		public int ADPCMLimit => EffectiveRules.ADPCMLimit;
		public AtlasOptimization AtlasOptimization => EffectiveRules.AtlasOptimization;
		public ModelCompression ModelCompression => EffectiveRules.ModelCompression;
		public string AtlasPacker => EffectiveRules.AtlasPacker;
		public TextureWrapMode WrapMode => EffectiveRules.WrapMode;
		public TextureFilter MinFilter => EffectiveRules.MinFilter;
		public TextureFilter MagFilter => EffectiveRules.MagFilter;
		public string CustomRule => EffectiveRules.CustomRule;
		public int AtlasItemPadding => EffectiveRules.AtlasItemPadding;

		public byte[] SHA1 => EffectiveRules.SHA1;

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
			foreach (var t in The.Workspace.Targets) {
				TargetRules.Add(t, ParticularCookingRules.GetDefault(t.Platform));
			}
			CommonRules = ParticularCookingRules.GetDefault(The.UI.GetActiveTarget().Platform);
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
			} if (yi.Name == "Bundles") {
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
			}
			if (EffectiveRules.WrapMode != TextureWrapMode.Clamp) {
				EffectiveRules.TextureAtlas = null;
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

		// pass target as null to build cooking rules disregarding targets
		public static Dictionary<string, CookingRules> Build(IFileEnumerator fileEnumerator, Target target)
		{
			var shouldRescanEnumerator = false;
			var pathStack = new Stack<string>();
			var rulesStack = new Stack<CookingRules>();
			var map = new Dictionary<string, CookingRules>(StringComparer.OrdinalIgnoreCase);
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
							} catch (UnauthorizedAccessException) {
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

		private static ModelCompression ParseModelCompression(string value)
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
					throw new Lime.Exception("Error parsing ModelCompression. Must be one of: None, Deflate, LZMA");
			}
		}

		private static CookingRules ParseCookingRules(CookingRules basicRules, string path, Target target)
		{
			var rules = basicRules.InheritClone();
			var currentRules = rules.CommonRules;
			try {
				rules.CommonRules.LastChangeTime = File.GetLastWriteTime(path);
				using (var s = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
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
								Target currentTarget = null;
								foreach (var t in The.Workspace.Targets) {
									if (targetName == t.Name) {
										currentTarget = t;
									}
								}
								if (currentTarget == null) {
									throw new Lime.Exception($"Invalid target: {targetName}");
								}
								currentRules = rules.TargetRules[currentTarget];
								{
									var targetPlatformAttribute = (TargetPlatformsAttribute)typeof(ICookingRules)
										.GetProperty(words[0]).GetCustomAttribute(typeof(TargetPlatformsAttribute));
									if (targetPlatformAttribute != null && !targetPlatformAttribute.TargetPlatforms.Contains(currentTarget.Platform)) {
										throw new Lime.Exception($"Invalid platform {currentTarget.Platform} for cooking rule {words[0]}");
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
			try {
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
				case "GenerateOpacityMask":
					rules.GenerateOpacityMask = ParseBool(words[1]);
					break;
				case "PVRFormat":
					rules.PVRFormat = ParsePVRFormat(words[1]);
					break;
				case "DDSFormat":
					rules.DDSFormat = ParseDDSFormat(words[1]);
					break;
				case "Bundles":
					rules.Bundles = new string[words.Count - 1];
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
				case "ModelCompression":
					rules.ModelCompression = ParseModelCompression(words[1]);
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
				case "AtlasItemPadding":
					rules.AtlasItemPadding = int.Parse(words[1]);
					break;
				default:
					throw new Lime.Exception("Unknown attribute {0}", words[0]);
				}
				rules.Override(words[0]);
			} catch (System.Exception e) {
				Debug.Write("Failed to parse cooking rules: {0} {1} {2}", string.Join(",", words), path, e);
				throw;
			}
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

