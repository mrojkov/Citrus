using Lime;
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

		public string Name { get; private set; }

		public Matrix44 GlobalTranform { get; private set; }

		public Matrix44 LocalTranform { get; private set; }

		public Node(IntPtr ptr) : base (ptr)
		{
			int count = FbxNodeGetChildCount(NativePtr);
			var materialPtr = FbxNodeGetMaterial(NativePtr);
			Material = new Material(materialPtr);

			Name = Marshal.PtrToStringAnsi(FbxNodeGetName(NativePtr));
			Attribute = NodeAttribute.GetFromNode(NativePtr, 0);
			for (int i = 0; i < count; i++) {
				Children.Add(new Node(FbxNodeGetChildNode(NativePtr, i)));
			}
			GlobalTranform = FbxNodeGetGlobalTransform(NativePtr).To<Mat4x4>().ToLime();
			LocalTranform =  FbxNodeGetLocalTransform(NativePtr).To<Mat4x4>().ToLime();
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

		#region PInvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxNodeGetChildNode(IntPtr node, int index);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FbxNodeGetChildCount(IntPtr node);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxNodeGetMaterial(IntPtr node);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxNodeGetGlobalTransform(IntPtr node);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxNodeGetLocalTransform(IntPtr node);

		[DllImport(ImportConfig.LibName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxNodeGetName(IntPtr node);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FbxNodeGetAttributeCount(IntPtr node);

		#endregion
	}
}
