using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Orange.FbxImporter
{
	public class Node : FbxObject
	{
		public NodeAttribute Attribute { get; private set; } = NodeAttribute.Empty;

		public List<Node> Children { get; } = new List<Node>();

		public Material Material { get; private set; }

		#region PInvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr GetChildNode(IntPtr node, int index);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int GetChildCount(IntPtr node);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr GetMaterial(IntPtr node);

		#endregion

		public Node(IntPtr ptr) : base (ptr)
		{
			int count = GetChildCount(NativePtr);
			var materialPtr = GetMaterial(NativePtr);

			if (materialPtr != IntPtr.Zero) {
				Material = new Material(materialPtr);
			}

			Attribute = NodeAttribute.GetFromNode(NativePtr);
			for (int i = 0; i < count; i++) {
				Children.Add(new Node(GetChildNode(NativePtr, i)));
			}
		}

		public override string ToString(int level)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("Node: ".ToLevel(level + 1));
			builder.Append("Attribute: ".ToLevel(level + 2));
			builder.AppendLine(Attribute.ToString(level + 3));
			builder.Append("Material: ".ToLevel(level + 2));
			builder.AppendLine(Material?.ToString(level + 3) ?? "None");
			builder.AppendLine("Children: ".ToLevel(level + 2));
			foreach(var node in Children) {
				builder.Append(node.ToString(level + 1));
			}
			return builder.ToString();
		}
	}
}
