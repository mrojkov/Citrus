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
		}

		public ParsedFramesTree()
		{
		}

		public string GenerateCode(ScenesCodeCooker scenesCodeCooker, bool isRootNode = true)
		{
			var result = isRootNode ? scenesCodeCooker.SceneCodeTemplate : scenesCodeCooker.FrameCodeTemplate;
			result = result.Replace("<%CLASS_NAME%>", ClassName);
			result = result.Replace("<%SCENE_PATH%>", "\"" + Path.ChangeExtension(ScenePath, null) + "\"");
			result = result.Replace("<%FIELDS%>", GenerateFields(scenesCodeCooker));
			result = result.Replace("<%INIT_FIELDS%>", GenerateFieldsInitialization(scenesCodeCooker));
			result = result.Replace("<%INNER_CLASSES%>", GenerateInnerClasses(scenesCodeCooker));
			result = result.Replace("<%ANIMATIONS%>", ParsedNode.GenerateAnimations(isRootNode ? "T" : null));
			result = result.Replace("<%IT%>", ParsedNode.GenerateIt());
			result = result.Replace("<%COMMON_BASE%>", GenerateCommonBase());
			return result;
		}

		private string GenerateCommonBase()
		{
			return !BaseClassName.IsNullOrWhiteSpace() ? $"\npublic Common.{BaseClassName} {BaseClassName};" : "";
		}

		private string GenerateInnerClasses(ScenesCodeCooker scenesCodeCooker)
		{
			var result = "";
			foreach (var pft in InnerClasses.Where(c => !c.ParsedNode.IsExternalScene)) {
				result += pft.GenerateCode(scenesCodeCooker, isRootNode: false);
			}
			foreach (var pn in ParsedNodes.Where(c => !c.IsExternalScene)) {
				result += pn.GenerateCode(scenesCodeCooker.NodeCodeTemplate);
			}
			return result;
		}

		private string GenerateFieldsInitialization(ScenesCodeCooker scenesCodeCooker)
		{
			var result = "";
			if (!BaseClassName.IsNullOrWhiteSpace()) {
				result += $"{BaseClassName} = new Common.{BaseClassName}(Node);\n";
			}
			foreach (var node in ParsedNodes) {
				result += string.Format(
					"@{2} = new {0}(Node.Find<Node>(\"{1}\"));",
					scenesCodeCooker.GetFullTypeOf(node),
					node.Id,
					node.FieldName
				);
				result += "\n";
			}
			foreach (var pft in InnerClasses) {
				result += string.Format(
					"@{2} = new {0}(Node.Find<Node>(\"{1}\"));",
					scenesCodeCooker.GetFullTypeOf(pft),
					pft.ParsedNode.Id,
					pft.FieldName
				);
				result += "\n";
			}
			return result;
		}

		private string GenerateFields(ScenesCodeCooker scenesCodeCooker)
		{
			var result = "";
			foreach (var node in ParsedNodes) {
				result += string.Format("public readonly {0} @{1};", scenesCodeCooker.GetFullTypeOf(node), node.FieldName);
				result += "\n";
			}
			foreach (var pft in InnerClasses) {
				result += string.Format("public readonly {0} @{1};", scenesCodeCooker.GetFullTypeOf(pft), pft.FieldName);
				result += "\n";
			}
			return result;
		}
	}
}
