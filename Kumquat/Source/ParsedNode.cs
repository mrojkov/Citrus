using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;

namespace Kumquat
{
	public class ParsedNode
	{
		public readonly string Id;
		public readonly string Name;
		public string Type;
		public readonly string ClassName;
		public readonly string FieldName;
		public readonly string ContentsPath;
		public readonly List<string> Markers = new List<string>();

		public bool IsExternalScene => !string.IsNullOrEmpty(ContentsPath);

		public ParsedNode(ParsedNode parsedNode)
		{
			Id = parsedNode.Id;
			Name = parsedNode.Name;
			Type = parsedNode.Type;
			ClassName = parsedNode.ClassName;
			FieldName = parsedNode.FieldName;
			ContentsPath = parsedNode.ContentsPath;
			Markers = parsedNode.Markers;
		}

		public ParsedNode(Node node, string name)
		{
			Id = node.Id;
			Name = name;
			Type = node.GetType().Name;
			ClassName = name;
			FieldName = "_" + name;
			ContentsPath = AssetPath.CorrectSlashes(node.ContentsPath ?? "");

			if (IsExternalScene) {
				string externalName;
				string externalBaseName;
				ScenesCodeCooker.ParseCommonName(Path.GetFileNameWithoutExtension(ContentsPath), out externalName, out externalBaseName);
				ClassName = externalName;
			}

			foreach (var marker in node.Markers.Where(marker => !string.IsNullOrEmpty(marker.Id))) {
				Markers.Add(marker.Id);
			}
		}

		public string GenerateCode(string template)
		{
			var result = template;
			result = result.Replace("<%CLASS_NAME%>", ClassName);
			result = result.Replace("<%ANIMATIONS%>", GenerateAnimations());
			result = result.Replace("<%IT%>", GenerateIt());
			return result;
		}

		public string GenerateIt()
		{
			var result = string.Format("public Lime.{0} It => (Lime.{0})Node;", Type);
			return result;
		}

		public string GenerateAnimations(string customType = null)
		{
			var result = "";
			foreach (var marker in Markers) {
				var safeMarker = marker;
				if (safeMarker.StartsWith("@")) {
					safeMarker = safeMarker.Substring(1);
				}
				result += string.Format("public {1} RunAnimation{0}() \n", safeMarker, customType ?? Type);
				result += "{ \n";
				result += string.Format("Node.RunAnimation(\"{0}\");\n", marker);
				result += string.Format("return ({0})Node;\n", customType ?? Type);
				result += "} \n";
			}
			return result;
		}
	}
}
