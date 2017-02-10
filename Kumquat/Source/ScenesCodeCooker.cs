using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Lime;

namespace Kumquat
{
	public class ScenesCodeCooker
	{
		private readonly string directory;
		private readonly string projectName;
		private readonly string bundleName;
		private readonly Dictionary<string, string> sceneToBundleMap;
		private readonly Dictionary<string, Frame> scenes;

		public readonly string SceneCodeTemplate;
		public readonly string FrameCodeTemplate;
		public readonly string NodeCodeTemplate;

		private string currentCookingScene;
		private string BundleNamespace { get { return GetBundleNamespace(projectName, bundleName); } }

		private readonly Dictionary<string, List<ParsedFramesTree>> commonParts = new Dictionary<string, List<ParsedFramesTree>>();

		public ScenesCodeCooker(
			string directory,
			string projectName,
			Dictionary<string, string> sceneToBundleMap,
			Dictionary<string, Frame> scenes
		)
		{
			this.directory = directory;
			this.projectName = projectName;
			this.sceneToBundleMap = sceneToBundleMap;
			this.scenes = scenes;
			bundleName = sceneToBundleMap[scenes.Keys.First()];
			SceneCodeTemplate = GetEmbeddedResource("SceneFile.txt");
			FrameCodeTemplate = GetEmbeddedResource("ParsedFrame.txt");
			NodeCodeTemplate = GetEmbeddedResource("ParsedNode.txt");
		}

		public void Start()
		{
			Console.WriteLine("Generating scenes code for {0} scenes...", scenes.Count);
			var generatedScenesPath = $@"{directory}/{projectName}.GeneratedScenes";
			if (!Directory.Exists(generatedScenesPath)) {
				GenerateProjectFiles(generatedScenesPath);
			}
			var bundleSourcePath = $@"{directory}/{projectName}.GeneratedScenes/Scenes/{bundleName}";
			if (Directory.Exists(bundleSourcePath)) {
				Directory.Delete(bundleSourcePath, true);
			}
			Directory.CreateDirectory(bundleSourcePath);
			foreach (var scene in scenes) {
				currentCookingScene = scene.Key;
				var parsedFramesTree = GenerateParsedFramesTree(scene.Key, scene.Value);
				var code = parsedFramesTree.GenerateCode(this);
				code = code.Replace("<%PROJECT_NAME%>", projectName);
				code = code.Replace("<%NAMESPACE%>", BundleNamespace);
				code = new CodeFormatter(code).FormattedCode;
				File.WriteAllText(bundleSourcePath + "/" + parsedFramesTree.ClassName + ".cs", code);
			}
			GenerateCommonParts(bundleSourcePath);
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
			for (int i = 0; i < typeNames.Count; i++) {
				temp[i] = types[i];
			}
			bool checkPass = false;
			Type tested = null;
			while (!checkPass) {
				tested = temp[0];
				checkPass = true;
				for (int i = 1; i < temp.Length; i++) {
					if (tested == temp[i])
						continue;
					else {
						if (tested == temp[i].BaseType) {
							temp[i] = temp[i].BaseType;
							continue;
						}
						else if (tested.BaseType != null && tested.BaseType == temp[i]) {
							for (int j = 0; j <= i - 1; j++) {
								temp[j] = temp[j].BaseType;
							}
							checkPass = false;
							break;
						}
						else {
							for (int j = 0; j <= i; j++) {
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

		private void GenerateCommonParts(string bundleSourcePath)
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
			var code = $"using Lime;\n namespace {BundleNamespace}.Commons\n{{\n" +
			           $"<%GEN%>\n" +
			           $"}}\n";

			var roots = new List<ParsedFramesTree>();
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
					Dictionary<string, List<ParsedFramesTree>> treeUnion = new Dictionary<string, List<ParsedFramesTree>>();
					Dictionary<string, List<ParsedNode>> leafUnion = new Dictionary<string, List<ParsedNode>>();
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
							var leafClone = new ParsedNode(i.Value.First());
							leafClone.Type = GetCommonBaseClass(i.Value.Select(j => j.Type).ToList()).Name;
							UniteMarkers(leafClone, i.Value);
							nextRoot.ParsedNodes.Add(leafClone);
						}
					}
					foreach (var i in treeUnion) {
						if (i.Value.Count == s.Count) {
							var subtreeClone = new ParsedFramesTree(i.Value.First());
							subtreeClone.ParsedNode.Type = GetCommonBaseClass(i.Value.Select(j => j.ParsedNode.Type).ToList()).Name;
							UniteMarkers(subtreeClone.ParsedNode, i.Value.Select(j => j.ParsedNode).ToList());
							nextRoot.InnerClasses.Add(subtreeClone);
							queue.Enqueue(i.Value);
							sync.Enqueue(subtreeClone);
						}
					}
				}
				if (root.InnerClasses.Count != 0 || root.ParsedNodes.Count != 0) {
					roots.Add(root);
				}
			}
			foreach (var r in roots) {
				code = code.Replace("<%GEN%>", r.GenerateCode(this, false) + "\n<%GEN%>");
			}
			code = code.Replace("<%GEN%>", "");
			code = new CodeFormatter(code).FormattedCode;
			File.WriteAllText(bundleSourcePath + "/Commons.cs", code);
		}

		public string GetFullTypeOf(ParsedFramesTree parsedFramesTree)
		{
			if (!parsedFramesTree.ParsedNode.IsExternalScene) {
				return parsedFramesTree.ClassName;
			}

			var pftType = string.Format("{0}<{1}>", parsedFramesTree.ClassName, parsedFramesTree.ParsedNode.Type);

			var scenePath = parsedFramesTree.ParsedNode.ContentsPath + ".scene";
			string bundleName;
			if (!sceneToBundleMap.TryGetValue(scenePath, out bundleName)) {
				Console.WriteLine("Warning! Can not find external scene \'{0}\' in \'{1}!\'", scenePath, currentCookingScene);
				return pftType;
			}
			var pftBundleNamespace = GetBundleNamespace(projectName, bundleName);

			return BundleNamespace == pftBundleNamespace ? pftType : pftBundleNamespace + '.' + pftType;
		}

		public string GetFullTypeOf(ParsedNode node)
		{
			if (!node.IsExternalScene) {
				return node.ClassName;
			}

			var nodeType = string.Format("{0}<{1}>", node.ClassName, node.Type);

			var scenePath = node.ContentsPath + ".scene";
			string bundleName;
			if (!sceneToBundleMap.TryGetValue(scenePath, out bundleName)) {
				Console.WriteLine("Warning! Can not find external scene \'{0}\' in \'{1}!\'", scenePath, currentCookingScene);
				return nodeType;
			}
			var nodeBundleNamespace = GetBundleNamespace(projectName, bundleName);

			return BundleNamespace == nodeBundleNamespace ? nodeType : nodeBundleNamespace + '.' + nodeType;
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
			}

			var nodesToParse = new List<Node>();
			if (!parsedFramesTree.ParsedNode.IsExternalScene) {
				nodesToParse.Add(node);
			}

			while (nodesToParse.Count > 0) {
				var current = nodesToParse[0];
				nodesToParse.RemoveAt(0);

				foreach (var n in current.Nodes) {
					if (n.Id.StartsWith(">")) {
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
						if (n.Id.StartsWith("@")) {
							parsedNode = new ParsedNode(n, n.Id.Substring(1));
							parsedFramesTree.ParsedNodes.Add(parsedNode);
						}

						if (n.Nodes.Count > 0 && (parsedNode == null || !parsedNode.IsExternalScene)) {
							nodesToParse.Add(n);
						}
					}
				}
			}
			return parsedFramesTree;
		}

		private static string GetBundleNamespace(string projectName, string bundleName)
		{
			return string.Format(
				"{0}.Scenes{1}",
				projectName,
				bundleName == "Main" ? "" : "." + bundleName.Substring(bundleName.LastIndexOf('/') + 1)
			);
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
