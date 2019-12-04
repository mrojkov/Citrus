using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;

namespace Kumquat
{
	public class ParsedNode
	{
		public class AnimationMarker
		{
			public readonly string AnimationId;
			public readonly string Name;

			public AnimationMarker(string animationId, string name)
			{
				AnimationId = animationId;
				Name = name;
			}
		}

		public readonly string Id;
		public readonly string Name;
		public string Type;
		public string TypeFullName;
		public readonly string ClassName;
		public readonly string FieldName;
		public readonly string ContentsPath;
		public readonly bool IsInExternalScene;
		public readonly List<string> Markers = new List<string>();
		public readonly List<AnimationMarker> NamedMarkers = new List<AnimationMarker>();

		public bool IsExternalScene => !string.IsNullOrEmpty(ContentsPath);

		public ParsedNode(ParsedNode parsedNode)
		{
			Id = parsedNode.Id;
			Name = parsedNode.Name;
			Type = parsedNode.Type;
			TypeFullName = parsedNode.TypeFullName;
			ClassName = parsedNode.ClassName;
			FieldName = parsedNode.FieldName;
			ContentsPath = parsedNode.ContentsPath;
			IsInExternalScene = parsedNode.IsInExternalScene;
			Markers = parsedNode.Markers;
			NamedMarkers = parsedNode.NamedMarkers;
		}

		public ParsedNode(Node node, string name, bool isInExternalScene)
		{
			Id = node.Id;
			Name = name;
			Type = node.GetType().Name;
			TypeFullName = node.GetType().FullName;
			ClassName = name;
			FieldName = "_" + name;
			ContentsPath = AssetPath.CorrectSlashes(node.ContentsPath ?? "");
			IsInExternalScene = isInExternalScene;

			if (IsExternalScene) {
				string externalName;
				string externalBaseName;
				ScenesCodeCooker.ParseCommonName(Path.GetFileNameWithoutExtension(ContentsPath), out externalName, out externalBaseName);
				ClassName = externalName;
			}

			foreach (var animation in node.Animations) {
				var isDefault = animation == node.DefaultAnimation;
				foreach (var marker in animation.Markers.Where(marker => !string.IsNullOrEmpty(marker.Id))) {
					if (isDefault) {
						Markers.Add(marker.Id);
					}
					NamedMarkers.Add(new AnimationMarker(animation.Id, marker.Id));
				}
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
			var result = string.Format("public {0} It => ({0})Node;", TypeFullName);
			return result;
		}

		public string GenerateAnimations(string customType = null)
		{
			var result = "";
			foreach (var marker in NamedMarkers) {
				var safeMarker = marker.Name;
				if (safeMarker.StartsWith("@")) {
					safeMarker = safeMarker.Substring(1);
				}
				result += $"public {customType ?? TypeFullName} Run{marker.AnimationId ?? "Animation"}{safeMarker}() \n";
				result += "{ \n";
				if (marker.AnimationId == null) {
					result += $"Node.RunAnimation(\"{marker.Name}\");\n";
				} else {
					result += $"Node.RunAnimation(\"{marker.Name}\", \"{marker.AnimationId}\");\n";
				}
				result += $"return ({customType ?? TypeFullName})Node;\n";
				result += "} \n";
			}
			return result;
		}
	}
}
