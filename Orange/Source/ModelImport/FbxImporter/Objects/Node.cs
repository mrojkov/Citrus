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

		public Material[] Materials { get; private set; }

		public Matrix44 LocalTranform { get; private set; }

		public string Name { get; private set; }

		public Node(IntPtr ptr) : base (ptr)
		{
			var nodeCount = FbxNodeGetChildCount(NativePtr);
			var materialsCount = FbxNodeGetMaterialCount(NativePtr);

			Name = Marshal.PtrToStringAnsi(FbxNodeGetName(NativePtr));
			Materials = new Material[materialsCount];
			for (int i = 0; i < materialsCount; i++) {
				Materials[i] = new Material(FbxNodeGetMaterial(NativePtr, i));
			}

			Attribute = NodeAttribute.GetFromNode(NativePtr);

			for (int i = 0; i < nodeCount; i++) {
				Children.Add(new Node(FbxNodeGetChildNode(NativePtr, i)));
			}

			LocalTranform =  FbxNodeGetLocalTransform(NativePtr).To<Mat4x4>().ToLime();
		}

		#region PInvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxNodeGetChildNode(IntPtr node, int index);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FbxNodeGetChildCount(IntPtr node);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxNodeGetMaterial(IntPtr node, int idx);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxNodeGetLocalTransform(IntPtr node);

		[DllImport(ImportConfig.LibName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxNodeGetName(IntPtr node);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FbxNodeGetMaterialCount(IntPtr node);

		#endregion
	}
}
