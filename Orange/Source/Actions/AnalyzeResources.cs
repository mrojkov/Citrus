using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Lime;
using Widget = Lime.Widget;

namespace Orange
{
	static class AnalyzeResources
	{
		internal struct PathRequestRecord
		{
#pragma warning disable CS0649
			public string path;
			public string bundle;
#pragma warning restore CS0649
		}

		internal static List<PathRequestRecord> requestedPaths;

		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Analyze Resources")]
		public static void AnalyzeResourcesAction()
		{
			var target = The.UI.GetActiveTarget();
			requestedPaths = new List<PathRequestRecord>();
			var crossRefReport = new List<Tuple<string, List<string>>>();
			var missingResourcesReport = new List<string>();
			var suspiciousTexturesReport = new List<string>();
			var bundles = new HashSet<string>();
			var cookingRulesMap = CookingRulesBuilder.Build(The.Workspace.AssetFiles, target);
			AssetBundle.Current = new PackedAssetBundle(The.Workspace.GetBundlePath(target.Platform, CookingRulesBuilder.MainBundleName));
			foreach (var i in cookingRulesMap) {
				if (i.Key.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) {
					if (i.Value.TextureAtlas == null && i.Value.PVRFormat != PVRFormat.PVRTC4 && i.Value.PVRFormat != PVRFormat.PVRTC4_Forced) {
						suspiciousTexturesReport.Add(string.Format("{0}: {1}, atlas: none",
							i.Key, i.Value.PVRFormat));
					}
					if (i.Value.PVRFormat != PVRFormat.PVRTC4 && i.Value.PVRFormat != PVRFormat.PVRTC4_Forced && i.Value.PVRFormat != PVRFormat.PVRTC2) {
						int w;
						int h;
						bool hasAlpha;
						TextureConverterUtils.GetPngFileInfo(
							Path.Combine(The.Workspace.AssetsDirectory, i.Key), out w, out h,
							out hasAlpha, false);
						if (w >= 1024 || h >= 1024) {
							suspiciousTexturesReport.Add(string.Format("{3}: {0}, {1}, {2}, {4}, atlas: {5}",
								w, h, hasAlpha, i.Key, i.Value.PVRFormat, i.Value.TextureAtlas));
						}
					}
				}
				foreach (var bundle in i.Value.Bundles) {
					if (bundle != CookingRulesBuilder.MainBundleName) {
						bundles.Add(bundle);
					}
				}
			}
			AssetBundle.Current = new AggregateAssetBundle(bundles.Select(i => new PackedAssetBundle(The.Workspace.GetBundlePath(target.Platform, i))).ToArray());
			The.Workspace.AssetFiles.EnumerationFilter = (info) => {
				CookingRules rules;
				if (cookingRulesMap.TryGetValue(info.Path, out rules)) {
					if (rules.Ignore)
						return false;
				}
				return true;
			};
			var usedImages = new HashSet<string>();
			var usedSounds = new HashSet<string>();
			foreach (var srcFileInfo in The.Workspace.AssetFiles.Enumerate(".tan")) {
				var srcPath = srcFileInfo.Path;
				using (var scene = (Frame)Node.CreateFromAssetBundle(srcPath)) {
					foreach (var j in scene.Descendants) {
						var checkTexture = new Action<SerializableTexture>((Lime.SerializableTexture t) => {
							if (t == null) {
								return;
							}
							string texPath;
							try {
								texPath = t.SerializationPath;
							} catch {
								return;
							}
							if (string.IsNullOrEmpty(texPath)) {
								return;
							}
							if (texPath.Length == 2 && texPath[0] == '#') {
								switch (texPath[1]) {
									case 'a': case'b': case 'c': case 'd': case 'e': case 'f': case 'g':
										return;
									default:
										suspiciousTexturesReport.Add(string.Format("wrong render target: {0}, {1}", texPath, j.ToString()));
										return;
								}
							}
							string[] possiblePaths = new string[]
							{
								texPath + ".atlasPart",
								texPath + ".pvr",
								texPath + ".jpg",
								texPath + ".png",
								texPath + ".dds",
								texPath + ".jpg",
								texPath + ".png",
							};
							foreach (var tpp in possiblePaths) {
								if (Lime.AssetBundle.Current.FileExists(tpp)) {
									Lime.AssetBundle.Current.OpenFile(tpp);
									usedImages.Add(texPath.Replace('\\', '/'));
									return;
								}
							}
							missingResourcesReport.Add(string.Format("texture missing:\n\ttexture path: {0}\n\tscene path: {1}\n",
								t.SerializationPath, j.ToString()));
						});
						var checkAnimators = new Action<Node>((Node n) => {
							Lime.Animator<Lime.SerializableTexture> ta;
							if (n.Animators.TryFind<SerializableTexture>("Texture", out ta)) {
								foreach (var key in ta.ReadonlyKeys) {
									checkTexture(key.Value);
								}
							}
						});
						if (j is Widget) {
							var w = j as Lime.Widget;
							var serializableTexture = w.Texture as SerializableTexture;
							if (serializableTexture != null) {
								checkTexture(serializableTexture);
							}
							checkAnimators(w);
						} else if (j is ParticleModifier) {
							var pm = j as Lime.ParticleModifier;
							var serializableTexture = pm.Texture as SerializableTexture;
							if (serializableTexture != null) {
								checkTexture(serializableTexture);
							}
							checkAnimators(pm);
						} else if (j is Lime.Audio) {
							var au = j as Lime.Audio;
							var path = au.Sample.SerializationPath + ".sound";
							usedSounds.Add(au.Sample.SerializationPath.Replace('\\', '/'));
							if (!Lime.AssetBundle.Current.FileExists(path)) {
								missingResourcesReport.Add(string.Format("audio missing:\n\taudio path: {0}\n\tscene path: {1}\n",
									path, j.ToString()));
							} else {
								using (var tempStream = Lime.AssetBundle.Current.OpenFile(path)) {

								}
							}
							// FIXME: should we check for audio:Sample animators too?
						}
					}
				}
				var reportList = new List<string>();
				foreach (var rpr in requestedPaths) {
					string pattern = String.Format(@".*[/\\](.*)\.{0}", target.Platform.ToString());
					string bundle = "";
					foreach (Match m in Regex.Matches(rpr.bundle, pattern, RegexOptions.IgnoreCase)) {
						bundle = m.Groups[1].Value;
					}
					int index = Array.IndexOf(cookingRulesMap[srcPath].Bundles, bundle);
					if (index == -1) {
						reportList.Add(string.Format("\t[{0}]=>[{2}]: {1}",
							string.Join(", ", cookingRulesMap[srcPath].Bundles), rpr.path, bundle));
					}
				}
				requestedPaths.Clear();
				if (reportList.Count > 0) {
					crossRefReport.Add(new Tuple<string, List<string>>(srcPath, reportList));
				}
				Lime.Application.FreeScheduledActions();
			}

			var allImages = new Dictionary<string, bool>();
			foreach (var img in The.Workspace.AssetFiles.Enumerate(".png")) {
				var key = Path.Combine(Path.GetDirectoryName(img.Path), Path.GetFileNameWithoutExtension(img.Path)).Replace('\\', '/');
				if (!key.StartsWith("Fonts")) {
					allImages[key] = false;
				}
			}
			foreach (var img in usedImages) {
				allImages[img] = true;
			}
			var unusedImages = allImages.Where(kv => !kv.Value).Select(kv => kv.Key).ToList();

			var allSounds = new Dictionary<string, bool>();
			foreach (var sound in The.Workspace.AssetFiles.Enumerate(".ogg")) {
				var key = Path.Combine(Path.GetDirectoryName(sound.Path), Path.GetFileNameWithoutExtension(sound.Path))
					.Replace('\\', '/');
				allSounds[key] = false;
			}
			foreach (var sound in usedSounds) {
				allSounds[sound] = true;
			}
			var unusedSounds = allSounds.Where(kv => !kv.Value).Select(kv => kv.Key).ToList();

			Action<string> writeHeader = (s) => {
				int n0 = (80 - s.Length) / 2;
				int n1 = (80 - s.Length)%2 == 0 ? n0 : n0 - 1;
				Console.WriteLine("\n" + new String('=', n0) + " " + s + " " + new String('=', n1));
			};
			writeHeader("Cross Bundle Dependencies");
			foreach (var scenePath in crossRefReport) {
				Console.WriteLine("\n" + scenePath.Item1);
				foreach (var refStr in scenePath.Item2) {
					Console.WriteLine(refStr);
				}
			}
			writeHeader("Missing Resources");
			foreach (var s in missingResourcesReport) {
				Console.WriteLine(s);
			}
			writeHeader("Unused Images");
			foreach (var s in unusedImages) {
				Console.WriteLine(s);
			}
			writeHeader("Unused Sounds");
			foreach (var s in unusedSounds) {
				Console.WriteLine(s);
			}
			writeHeader("Suspicious Textures");
			foreach (var s in suspiciousTexturesReport) {
				Console.WriteLine(s);
			}
			The.Workspace.AssetFiles.EnumerationFilter = null;
		}
	}
}
