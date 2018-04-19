using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Yuzu;
using Lime;

namespace Kumquat
{
	public class SceneRecord
	{
		[YuzuMember]
		public string Bundle;

		[YuzuMember]
		public DateTime DateModified;

		[YuzuMember]
		public List<string> ReferringScenes = new List<string>();
	}

	public class CodeCookerCache
	{
		[YuzuMember]
		public Dictionary<string, SceneRecord> SceneFiles = new Dictionary<string, SceneRecord>();

		[YuzuMember]
		public Dictionary<string, List<string>> CommonPartToReferredScenes = new Dictionary<string, List<string>>();

		[YuzuMember]
		public Dictionary<string, DateTime> GeneratedProjectFileToModificationDate = new Dictionary<string, DateTime>();
	}

	public class ScenesCodeCooker
	{
		private static readonly string[] scenesExtensions = { ".scene", ".tan", ".model" };

		private readonly string directory;
		private readonly string projectName;
		private readonly Dictionary<string, string> sceneToBundleMap;
		private readonly Dictionary<string, Node> scenesForProcessing;
		public readonly string SceneCodeTemplate;
		public readonly string FrameCodeTemplate;
		public readonly string NodeCodeTemplate;
		private string currentCookingScene;
		private readonly Dictionary<string, List<ParsedFramesTree>> commonParts = new Dictionary<string, List<ParsedFramesTree>>();
		private readonly Dictionary<string, HashSet<string>> commonPartsScenes = new Dictionary<string, HashSet<string>>();
		private readonly Dictionary<string, List<string>> referringScenes = new Dictionary<string, List<string>>();
		private readonly Dictionary<string, string> externalSceneToOriginalScenePath = new Dictionary<string, string>();
		private readonly List<string> allScenes;
		private readonly List<string> modifiedScenes;
		private readonly string mainBundleName;
		private readonly CodeCookerCache codeCookerCache;

		public ScenesCodeCooker(
			string directory,
			string projectName,
			string mainBundleName,
			Dictionary<string, string> sceneToBundleMap,
			Dictionary<string, Node> scenesForProcessing,
			List<string> allScenes,
			List<string> modifiedScenes,
			CodeCookerCache codeCookerCache
		)
		{
			this.directory = directory;
			this.projectName = projectName;
			this.mainBundleName = mainBundleName;
			this.sceneToBundleMap = sceneToBundleMap;
			this.scenesForProcessing = scenesForProcessing;
			this.allScenes = allScenes;
			this.modifiedScenes = modifiedScenes;
			this.codeCookerCache = codeCookerCache;
			SceneCodeTemplate = GetEmbeddedResource("SceneFile.txt");
			FrameCodeTemplate = GetEmbeddedResource("ParsedFrame.txt");
			NodeCodeTemplate = GetEmbeddedResource("ParsedNode.txt");
		}

		public static void RetryUntilSuccessDeleteDirectory(string path)
		{
			int maxRetryCount = 32;
			while (true) {
				try {
					Directory.Delete(path, true);
					Console.WriteLine($"Deleted directory {path}");
					break;
				} catch (System.Exception) {
					Console.WriteLine($"Failed to delete {path}");
				}
				System.Threading.Thread.Sleep(100);
				maxRetryCount--;
				if (maxRetryCount == 0) {
					throw new InvalidOperationException($"Unable to remove directory \"{path}\"");
				}
			}
		}

		private static void RetryUntilSuccessCreateDirectory(string path)
		{
			int maxRetryCount = 32;
			while (true) {
				try {
					Directory.CreateDirectory(path);
					Console.WriteLine($"Created directory {path}");
					break;
				} catch (System.Exception) {
					Console.WriteLine($"Failed to create {path}");
				}
				System.Threading.Thread.Sleep(100);
				maxRetryCount--;
				if (maxRetryCount == 0) {
					throw new InvalidOperationException($"Unable to create directory \"{path}\"");
				}
			}
		}

		public void Start()
		{
			Console.WriteLine("Generating scenes code for {0} scenes...", scenesForProcessing.Count);
			var generatedScenesPath = $"{directory}/{projectName}.GeneratedScenes";
			if (!Directory.Exists(generatedScenesPath)) {
				GenerateProjectFiles(generatedScenesPath);
			}
			var scenesPath = $@"{directory}/{projectName}.GeneratedScenes/Scenes";
			RemoveOrphanedGeneratedCodeItems();
			RemoveUpdatingCommonPartsFromCache();
			foreach (var scenePath in allScenes) {
				externalSceneToOriginalScenePath.Add(Path.ChangeExtension(scenePath, null), scenePath);
			}
			var sceneToFrameTree = new List<Tuple<string, ParsedFramesTree>>();
			foreach (var scene in scenesForProcessing) {
				var bundleName = sceneToBundleMap[scene.Key];
				var bundleSourcePath = $"{scenesPath}/{bundleName}";
				if (!Directory.Exists(bundleSourcePath)) {
					RetryUntilSuccessCreateDirectory(bundleSourcePath);
				}
				currentCookingScene = scene.Key;
				var parsedFramesTree = GenerateParsedFramesTree(scene.Key, scene.Value);
				sceneToFrameTree.Add(new Tuple<string, ParsedFramesTree>(scene.Key, parsedFramesTree));
			}
			foreach (var kv in sceneToFrameTree) {
				var parsedFramesTree = kv.Item2;
				currentCookingScene = kv.Item1;
				var k = Path.ChangeExtension(kv.Item1, null);
				k = AssetPath.CorrectSlashes(k);
				var id = scenesForProcessing[kv.Item1].Id;
				var bundleName = sceneToBundleMap[kv.Item1];
				var bundleSourcePath = $"{scenesPath}/{bundleName}";
				var generatedCodePath = bundleSourcePath + "/" + parsedFramesTree.ClassName + ".cs";
				var useful =
					parsedFramesTree.ParsedNodes.Count > 0 ||
					parsedFramesTree.InnerClasses.Count > 0 ||
					(id != null && (id.StartsWith("@") || id.StartsWith(">")));
				if (!useful) {
					if (File.Exists(generatedCodePath)) {
						File.Delete(generatedCodePath);
					}
					continue;
				}
				var code = parsedFramesTree.GenerateCode(this);
				code = code.Replace("<%PROJECT_NAME%>", projectName);
				code = code.Replace("<%NAMESPACE%>", GetBundleNamespace(bundleName));
				code = new CodeFormatter(code).FormattedCode;
				File.WriteAllText(generatedCodePath, code);
			}
			UpdateCache();
			// Collect not loaded scenes which are also contains common parts affected by actually modified and referred to them scenes
			// Because we need those to correctly update common parts. i.e. to calc common parts you need all of common parts referrers
			List<string> reprocessScenes = new List<string>();
			foreach (var kv in codeCookerCache.CommonPartToReferredScenes) {
				if (commonParts.ContainsKey(kv.Key)) {
					foreach (var scene in kv.Value) {
						if (!modifiedScenes.Contains(scene)) {
							reprocessScenes.Add(scene);
						}
					}
				}
			}
			foreach (var scene in reprocessScenes) {
				GenerateParsedFramesTree(scene, Node.CreateFromAssetBundle(scene));
			}
			GenerateCommonParts(scenesPath);
			UpdateCommonPartsCache();
		}

		private void UpdateCommonPartsCache()
		{
			foreach (var kv in commonPartsScenes) {
				List<string> commonPartsFrom;
				if (!codeCookerCache.CommonPartToReferredScenes.ContainsKey(kv.Key)) {
					codeCookerCache.CommonPartToReferredScenes.Add(kv.Key, new List<string>());
				}
				commonPartsFrom = codeCookerCache.CommonPartToReferredScenes[kv.Key];
				// O(N^2) is okay here until we'll have really lots of common parts in different scenes
				foreach (var scenePath in kv.Value) {
					if (!commonPartsFrom.Contains(scenePath)) {
						commonPartsFrom.Add(scenePath);
					}
				}
			}
		}

		private void UpdateCache()
		{
			foreach (var kv in referringScenes) {
				if (!externalSceneToOriginalScenePath.ContainsKey(kv.Key)) {
					continue;
				}
				var key = externalSceneToOriginalScenePath[kv.Key];
				if (codeCookerCache.SceneFiles.ContainsKey(key)) {
					codeCookerCache.SceneFiles.Remove(key);
				}
				codeCookerCache.SceneFiles.Add(key, new SceneRecord {
					Bundle = sceneToBundleMap[key],
					DateModified = AssetBundle.Current.GetFileLastWriteTime(key).ToUniversalTime(),
					ReferringScenes = kv.Value.Select(path => externalSceneToOriginalScenePath[Path.ChangeExtension(path, null)]).ToList()
				});
			}
		}

		private void RemoveUpdatingCommonPartsFromCache()
		{
			foreach (var scenePath in modifiedScenes) {
				foreach (var kv in codeCookerCache.CommonPartToReferredScenes) {
					var scenesToRemove = new List<string>();
					foreach (var path in kv.Value) {
						if (scenePath == path) {
							scenesToRemove.Add(path);
						}
					}
					foreach (var path in scenesToRemove) {
						kv.Value.Remove(path);
					}
				}
			}
		}

		private void RemoveOrphanedGeneratedCodeItems()
		{
			var scenesPath = $@"{directory}/{projectName}.GeneratedScenes/Scenes";
			var scenesToDelete = new List<string>();
			var allScenesSet = new HashSet<string>(allScenes);
			foreach (var scenePath in codeCookerCache.SceneFiles.Keys) {
				if (!allScenesSet.Contains(scenePath)) {
					scenesToDelete.Add(scenePath);
				}
			}
			foreach (var scenePath in scenesToDelete) {
				var bundleName = codeCookerCache.SceneFiles[scenePath].Bundle;
				var bundleSourcePath = $"{scenesPath}/{bundleName}";
				var filename = Path.GetFileNameWithoutExtension(scenePath);
				string name;
				string baseClassName;
				ParseCommonName(filename, out name, out baseClassName);
				File.Delete($"{bundleSourcePath}/{name}.cs");
				codeCookerCache.SceneFiles.Remove(scenePath);
				var commonPartsToRemove = new List<string>();
				foreach (var kv in codeCookerCache.CommonPartToReferredScenes) {
					if (kv.Value.Contains(scenePath)) {
						kv.Value.Remove(scenePath);
						if (kv.Value.Count() == 0) {
							commonPartsToRemove.Add(kv.Key);
						}
					}
				}
				foreach (var commonPart in commonPartsToRemove) {
					File.Delete($"{scenesPath}/Common/{commonPart}.cs");
					codeCookerCache.CommonPartToReferredScenes.Remove(commonPart);
				}
			}
		}

		private void GenerateProjectFiles(string generatedScenesPath)
		{
			Console.WriteLine("Generating generted scenes project files");
			Directory.CreateDirectory(generatedScenesPath);
			var projectWin = GetEmbeddedResource("GeneratedScenes.Win.csproj");
			var projectAndroid = GetEmbeddedResource("GeneratedScenes.Android.csproj");
			var projectiOS = GetEmbeddedResource("GeneratedScenes.iOS.csproj");
			var parsedWidgetSourceCode = GetEmbeddedResource("ParsedWidget.cs");
			File.WriteAllText($@"{generatedScenesPath}/{projectName}.GeneratedScenes.Win.csproj", projectWin.Replace("<%PROJECT_NAME%>", projectName));
			File.WriteAllText($@"{generatedScenesPath}/{projectName}.GeneratedScenes.Android.csproj", projectAndroid.Replace("<%PROJECT_NAME%>", projectName));
			File.WriteAllText($@"{generatedScenesPath}/{projectName}.GeneratedScenes.iOS.csproj", projectiOS.Replace("<%PROJECT_NAME%>", projectName));
			File.WriteAllText($@"{generatedScenesPath}/ParsedWidget.cs", parsedWidgetSourceCode);
		}

		static Type GetCommonBaseClass(IReadOnlyList<string> typeNames)
		{
			var limeAssembly = typeof(Lime.Node).Assembly;
			var types = typeNames.Select(i => limeAssembly.GetType("Lime." + i)).ToList();
			var temp = new Type[typeNames.Count];
			for (var i = 0; i < typeNames.Count; i++) {
				temp[i] = types[i];
			}
			var checkPass = false;
			Type tested = null;
			while (!checkPass) {
				tested = temp[0];
				checkPass = true;
				for (var i = 1; i < temp.Length; i++) {
					if (tested == temp[i])
						continue;
					else {
						if (tested == temp[i].BaseType) {
							temp[i] = temp[i].BaseType;
							continue;
						}
						else if (tested.BaseType != null && tested.BaseType == temp[i]) {
							for (var j = 0; j <= i - 1; j++) {
								temp[j] = temp[j].BaseType;
							}
							checkPass = false;
							break;
						}
						else {
							for (var j = 0; j <= i; j++) {
								temp[j] = temp[j].BaseType;
							}
							checkPass = false;
							break;
						}
					}
				}
			}
			return tested;
		}

		private void GenerateCommonParts(string scenesPath)
		{
			if (commonParts.Count <= 0) {
				return;
			}
			Action<ParsedNode, List<ParsedNode>> UniteMarkers = (dest, source) => {
				var markersUnion = new Dictionary<string, int>();
				foreach (var n in source) {
					foreach (var marker in n.Markers) {
						if (!markersUnion.ContainsKey(marker)) {
							markersUnion.Add(marker, 0);
						}
						markersUnion[marker]++;
					}
				}
				dest.Markers.Clear();
				foreach (var mkv in markersUnion) {
					if (mkv.Value == source.Count) {
						dest.Markers.Add(mkv.Key);
					}
				}
			};
			var codeHeader = $"using Lime;\n namespace {(GetBundleNamespace(mainBundleName))}.Common\n{{\n" +
			                 $"<%GEN%>\n" +
			                 $"}}\n";
			var commonDirectory = $"{scenesPath}/Common";
			RetryUntilSuccessCreateDirectory(commonDirectory);
			foreach (var kv in commonParts) {
				var root = new ParsedFramesTree {
					ClassName = kv.Key,
					ParsedNode = new ParsedNode(kv.Value.First().ParsedNode),
				};
				UniteMarkers(root.ParsedNode, kv.Value.Select(j => j.ParsedNode).ToList());
				var queue = new Queue<List<ParsedFramesTree>>();
				var sync = new Queue<ParsedFramesTree>();
				queue.Enqueue(kv.Value);
				sync.Enqueue(root);
				while (queue.Count != 0) {
					var s = queue.Dequeue();
					var nextRoot = sync.Dequeue();
					var treeUnion = new Dictionary<string, List<ParsedFramesTree>>();
					var leafUnion = new Dictionary<string, List<ParsedNode>>();
					foreach (var ft in s) {
						foreach (var leafNode in ft.ParsedNodes) {
							var k = leafNode.Name + GetFullTypeOf(leafNode);
							if (!leafUnion.ContainsKey(k)) {
								leafUnion.Add(k, new List<ParsedNode>());
							}
							leafUnion[k].Add(leafNode);
						}
						foreach (var treeNode in ft.InnerClasses) {
							var k = treeNode.Name + GetFullTypeOf(treeNode);
							if (!treeUnion.ContainsKey(k)) {
								treeUnion.Add(k, new List<ParsedFramesTree>());
							}
							treeUnion[k].Add(treeNode);
						}
					}
					foreach (var i in leafUnion) {
						if (i.Value.Count == s.Count) {
							var leafClone = new ParsedNode(i.Value.First()) {
								Type = GetCommonBaseClass(i.Value.Select(j => j.Type).ToList()).Name
							};
							UniteMarkers(leafClone, i.Value);
							nextRoot.ParsedNodes.Add(leafClone);
						}
					}
					foreach (var i in treeUnion) {
						if (i.Value.Count == s.Count) {
							var subtreeClone = new ParsedFramesTree(i.Value.First()) {
								ParsedNode = { Type = GetCommonBaseClass(i.Value.Select(j => j.ParsedNode.Type).ToList()).Name }
							};
							UniteMarkers(subtreeClone.ParsedNode, i.Value.Select(j => j.ParsedNode).ToList());
							nextRoot.InnerClasses.Add(subtreeClone);
							queue.Enqueue(i.Value);
							sync.Enqueue(subtreeClone);
						}
					}
				}
				if (root.InnerClasses.Count != 0 || root.ParsedNodes.Count != 0 || root.ParsedNode.Markers.Count != 0) {
					var code = codeHeader.Replace("<%GEN%>", root.GenerateCode(this, false) + "\n<%GEN%>");
					code = code.Replace("<%GEN%>", "");
					code = new CodeFormatter(code).FormattedCode;
					File.WriteAllText($"{commonDirectory}/{kv.Key}.cs", code);
				}
			}
		}

		public string GetFullTypeOf(ParsedFramesTree parsedFramesTree)
		{
			if (!parsedFramesTree.ParsedNode.IsExternalScene) {
				return parsedFramesTree.ClassName;
			}

			var pftType = $"{parsedFramesTree.ClassName}<{parsedFramesTree.ParsedNode.Type}>";
			var bundleName = GetBundleNameOfExternalScene(parsedFramesTree.ParsedNode);
			if (string.IsNullOrEmpty(bundleName)) {
				return pftType;
			}

			var pftBundleNamespace = GetBundleNamespace(bundleName);
			return GetBundleNamespace(sceneToBundleMap[currentCookingScene]) == pftBundleNamespace ? pftType : pftBundleNamespace + '.' + pftType;
		}

		public string GetFullTypeOf(ParsedNode node)
		{
			if (!node.IsExternalScene) {
				return node.ClassName;
			}

			var nodeType = $"{node.ClassName}<{node.Type}>";
			var bundleName = GetBundleNameOfExternalScene(node);
			if (string.IsNullOrEmpty(bundleName)) {
				return nodeType;
			}

			var nodeBundleNamespace = GetBundleNamespace(bundleName);
			return GetBundleNamespace(sceneToBundleMap[currentCookingScene]) == nodeBundleNamespace ? nodeType : nodeBundleNamespace + '.' + nodeType;
		}

		private string GetBundleNameOfExternalScene(ParsedNode node)
		{
			var bundleName = string.Empty;
			var scenePath = AssetPath.CorrectSlashes(node.ContentsPath);
			foreach (var sceneExtension in scenesExtensions) {
				string sceneBundle;
				if (!sceneToBundleMap.TryGetValue(scenePath + sceneExtension, out sceneBundle)) {
					continue;
				}

				bundleName = sceneBundle;
				break;
			}
			if (string.IsNullOrEmpty(bundleName)) {
				Console.WriteLine("Warning! Can not find external scene \'{0}\' in \'{1}!\'", scenePath, currentCookingScene);
			}
			return bundleName;
		}

		public static bool ParseCommonName(string source, out string name, out string commonName)
		{
			commonName = null;
			var m = new Regex("\\.??\\[(.*?)\\](.*)").Match(source);
			if (m.Success) {
				commonName = m.Groups[1].Value;
				name = m.Groups[2].Value;
			} else {
				name = source;
			}
			return m.Success;
		}

		private ParsedFramesTree GenerateParsedFramesTree(string scenePath, Node frame)
		{
			string baseClassName;
			string name;
			var filename = Path.GetFileNameWithoutExtension(scenePath);
			ParseCommonName(filename, out name, out baseClassName);
			var parsedFramesTree = GenerateParsedFramesTreeHelper(frame, name, baseClassName);
			parsedFramesTree.ScenePath = scenePath;
			return parsedFramesTree;
		}

		private void AddReferringSceneSafe(string externalScene, string referringScene)
		{
			externalScene = AssetPath.CorrectSlashes(externalScene);
			referringScene = AssetPath.CorrectSlashes(referringScene);

			if (!referringScenes.ContainsKey(externalScene)) {
				var l = new List<string>();
				referringScenes.Add(externalScene, l);
			}
			referringScenes[externalScene].Add(referringScene);
		}

		private ParsedFramesTree GenerateParsedFramesTreeHelper(Node node, string name, string baseName)
		{
			var parsedFramesTree = new ParsedFramesTree {
				ParsedNode = new ParsedNode(node, name),
				ClassName = name,
				Name = name,
				FieldName = "_" + name,
				BaseClassName = baseName,
			};
			if (parsedFramesTree.ParsedNode.IsExternalScene) {
				AddReferringSceneSafe(parsedFramesTree.ParsedNode.ContentsPath, currentCookingScene);
				string externalName;
				string externalBaseName;
				ParseCommonName(Path.GetFileNameWithoutExtension(parsedFramesTree.ParsedNode.ContentsPath), out externalName, out externalBaseName);
				parsedFramesTree.ClassName = externalName;
				parsedFramesTree.BaseClassName = externalBaseName;

			}
			if (!baseName.IsNullOrWhiteSpace()) {
				if (!commonParts.ContainsKey(baseName)) {
					commonParts.Add(baseName, new List<ParsedFramesTree>());
				}
				commonParts[baseName].Add(parsedFramesTree);
				if (!commonPartsScenes.ContainsKey(baseName )) {
					commonPartsScenes.Add(baseName, new HashSet<string>());
				}
				// Root node tag is always full path to asset, see Node.CreateFromAssetBundle
				commonPartsScenes[baseName].Add(node.GetRoot().Tag);
			}

			var nodesToParse = new List<Node>();
			if (!parsedFramesTree.ParsedNode.IsExternalScene) {
				nodesToParse.Add(node);
			}

			while (nodesToParse.Count > 0) {
				var current = nodesToParse[0];
				nodesToParse.RemoveAt(0);

				foreach (var n in current.Nodes) {
					if (!string.IsNullOrEmpty(n.Id) && n.Id.StartsWith(">")) {
						string nextName;
						string nextBaseName;
						ParsedFramesTree innerClass;
						if (ParseCommonName(n.Id.Substring(1), out nextName, out nextBaseName)) {
							innerClass = GenerateParsedFramesTreeHelper(n, nextName, nextBaseName);
						} else {
							innerClass = GenerateParsedFramesTreeHelper(n, nextName, null);
						}
						parsedFramesTree.InnerClasses.Add(innerClass);
					} else {
						ParsedNode parsedNode = null;
						if (!string.IsNullOrEmpty(n.Id) && n.Id.StartsWith("@")) {
							parsedNode = new ParsedNode(n, n.Id.Substring(1));
							parsedFramesTree.ParsedNodes.Add(parsedNode);
						}
						if (parsedNode != null && parsedNode.IsExternalScene) {
							AddReferringSceneSafe(parsedNode.ContentsPath, currentCookingScene);
						}
						if (n.Nodes.Count > 0 && (parsedNode == null || !parsedNode.IsExternalScene)) {
							nodesToParse.Add(n);
						}
					}
				}
			}
			return parsedFramesTree;
		}

		private string GetBundleNamespace(string bundleName)
		{
			var bundlePart = bundleName == mainBundleName ? "" : "." + Path.GetFileName(bundleName);
			return $"{projectName}.Scenes{bundlePart}";
		}

		#region GetCodeTemplate

		private static UnmanagedMemoryStream GetResourceStream(string resName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var names = assembly.GetManifestResourceNames();
			string resourceName = null;
			foreach (var name in names.Where(name => name.IndexOf(resName, StringComparison.Ordinal) >= 0)) {
				resourceName = name;
			}
			var stream = assembly.GetManifestResourceStream(resourceName);
			return (UnmanagedMemoryStream)stream;
		}

		private static string GetEmbeddedResource(string templateName)
		{
			var file = GetResourceStream(templateName);
			using (var reader = new StreamReader(file)) {
				var result = reader.ReadToEnd();
				return result;
			}
		}

		#endregion
	}
}
