using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;

namespace Kumquat
{
	public class ParsedFramesTree
	{
		public ParsedNode ParsedNode;
		public string Name;
		public string ClassName;
		public string FieldName;
		public string ScenePath;
		public string BaseClassName;
		public bool IsInExternalScene;
		public readonly List<ParsedFramesTree> InnerClasses = new List<ParsedFramesTree>();
		public readonly List<ParsedNode> ParsedNodes = new List<ParsedNode>();

		public ParsedFramesTree(ParsedFramesTree parsedFramesTree)
		{
			ParsedNode = new ParsedNode(parsedFramesTree.ParsedNode);
			Name = parsedFramesTree.Name;
			ClassName = parsedFramesTree.ClassName;
			FieldName = parsedFramesTree.FieldName;
			ScenePath = parsedFramesTree.ScenePath;
			BaseClassName = parsedFramesTree.BaseClassName;
			IsInExternalScene = parsedFramesTree.IsInExternalScene;
		}

		public ParsedFramesTree() { }

		public string GenerateCode(ScenesCodeCooker scenesCodeCooker, bool isRootNode = true)
		{
			var result = isRootNode ? scenesCodeCooker.SceneCodeTemplate : scenesCodeCooker.FrameCodeTemplate;
			result = result.Replace("<%CLASS_NAME%>", ClassName);
			result = result.Replace("<%WRAPPED_NODE_TYPE_GENERIC_ARGUMENT%>", ParsedNode.TypeFullName);
			result = result.Replace("<%SCENE_PATH%>", "\"" + Path.ChangeExtension(ScenePath, null) + "\"");
			result = result.Replace("<%FIELDS%>", GenerateFields(scenesCodeCooker));
			result = result.Replace("<%INIT_FIELDS%>", GenerateFieldsInitialization(scenesCodeCooker));
			result = result.Replace("<%INNER_CLASSES%>", GenerateInnerClasses(scenesCodeCooker));
			result = result.Replace("<%ANIMATIONS%>", ParsedNode.GenerateAnimations(isRootNode ? "T" : null));
			result = result.Replace("<%IT%>", ParsedNode.GenerateIt());
			result = result.Replace("<%COMMON_BASE%>", GenerateCommonBase());
			result = result.Replace("<%USING%>", GenerateUsingLibraries(result));
			return result;
		}

		private string GenerateCommonBase()
		{
			return !BaseClassName.IsNullOrWhiteSpace() ? $"\npublic Common.{BaseClassName} {BaseClassName};" : "";
		}

		private string GenerateInnerClasses(ScenesCodeCooker scenesCodeCooker)
		{
			var result = "";

			var checkedNodes = new List<ParsedNode>();
			var checkedClasses = new List<ParsedFramesTree>();

			foreach (var pn in ParsedNodes.Where(c => !c.IsExternalScene)) {
				if (!checkedNodes.Exists(f => f.FieldName == pn.FieldName)) {
					checkedNodes.Add(pn);

					result += pn.GenerateCode(scenesCodeCooker.NodeCodeTemplate) + "\n";
				}
			}

			foreach (var pft in InnerClasses.Where(c => !c.ParsedNode.IsExternalScene)) {
				if (!checkedClasses.Exists(f => f.FieldName == pft.FieldName)) {
					checkedClasses.Add(pft);

					result += pft.GenerateCode(scenesCodeCooker, isRootNode: false) + "\n";
				}
			}

			return result;
		}

		private string GenerateFieldsInitialization(ScenesCodeCooker scenesCodeCooker)
		{
			var result = "";
			if (!BaseClassName.IsNullOrWhiteSpace()) {
				result += $"{BaseClassName} = new Common.{BaseClassName}(Node);\n";
			}

			var multiNodes = new List<ParsedNode>();
			var multiClasses = new List<ParsedFramesTree>();

			foreach (var node in ParsedNodes) {
				bool haveSameFields = ParsedNodes.Count(f => f.FieldName == node.FieldName) > 1;

				if (haveSameFields) {
					if (multiNodes.Exists(n => n.FieldName == node.FieldName)) {
						continue;
					}

					multiNodes.Add(node);

					result += string.Format(
						"@{2} = Node.Descendants.Where(nodeEl => nodeEl.Id == \"{1}\")" +
						".Select(nodeEl => new {0} (nodeEl)).ToList();"  + "\n",
						scenesCodeCooker.GetFullTypeOf(node),
						node.Id,
						node.FieldName
					);

				} else {
					result += string.Format(
						"@{2} = new {0}(Node.Find<Node>(\"{1}\"));" + "\n",
						scenesCodeCooker.GetFullTypeOf(node),
						node.Id,
						node.FieldName
					);
				}
			}

			foreach (var pft in InnerClasses) {
				bool haveSameFields = InnerClasses.Count(f => f.FieldName == pft.FieldName) > 1;

				if (haveSameFields) {
					if (multiClasses.Exists(n => n.FieldName == pft.FieldName)) {
						continue;
					}

					multiClasses.Add(pft);

					result += string.Format(
						"@{2} = Node.Descendants.Where(nodeEl => nodeEl.Id == \"{1}\")" +
						".Select(nodeEl => new {0} (nodeEl)).ToList();"  + "\n",
						scenesCodeCooker.GetFullTypeOf(pft),
						pft.ParsedNode.Id,
						pft.FieldName
					);
				} else {
					result += string.Format(
						"@{2} = new {0}(Node.Find<Node>(\"{1}\"));" + "\n",
						scenesCodeCooker.GetFullTypeOf(pft),
						pft.ParsedNode.Id,
						pft.FieldName
					);
				}
			}

			return result;
		}

		private string GenerateFields(ScenesCodeCooker scenesCodeCooker)
		{
			var result = "";
			var multiFields = new List<ParsedNode>();
			var multiClasses = new List<ParsedFramesTree>();

			foreach (var node in ParsedNodes) {
				bool haveSameFields = ParsedNodes.Count(f => f.FieldName == node.FieldName) > 1;

				if (haveSameFields) {
					if (multiFields.Exists(n => n.FieldName == node.FieldName)) {
						continue;
					}

					multiFields.Add(node);

					result += $"public readonly List<{scenesCodeCooker.GetFullTypeOf(node)}> @{node.FieldName};" + "\n";
				} else {
					result += $"public readonly {scenesCodeCooker.GetFullTypeOf(node)} @{node.FieldName};" + "\n";
				}
			}

			foreach (var pft in InnerClasses) {
				bool haveSameClasses = InnerClasses.Count(f => f.FieldName == pft.FieldName) > 1;

				if (haveSameClasses) {
					if (multiClasses.Exists(n => n.FieldName == pft.FieldName)) {
						continue;
					}

					multiClasses.Add(pft);
					result += $"public readonly List<{scenesCodeCooker.GetFullTypeOf(pft)}> @{pft.FieldName};" + "\n";
				} else {
					result += $"public readonly {scenesCodeCooker.GetFullTypeOf(pft)} @{pft.FieldName};" + "\n";
				}
			}

			return result;
		}

		private string GenerateUsingLibraries(string scene)
		{
			string result = "using Lime;" + "\n";

			if (scene.Contains("List<")) {
				result += "using System.Collections.Generic;"+ "\n";
			}

			if (scene.Contains(".Where") || scene.Contains(".Select")) {
				result += "using System.Linq;" + "\n";
			}

			return result;
		}
	}
}
