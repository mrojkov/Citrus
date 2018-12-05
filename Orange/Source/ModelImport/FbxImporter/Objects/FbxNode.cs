using Lime;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Orange.FbxImporter
{
	public class FbxNode : FbxObject
	{
		public FbxNodeAttribute Attribute { get; }

		public List<FbxNode> Children { get; } = new List<FbxNode>();

		public FbxMaterial[] Materials { get; }

		public Matrix44 LocalTranform { get; }

		public string Name { get; }

		public FbxNode(IntPtr ptr) : base(ptr)
		{
			var nodeCount = FbxNodeGetChildCount(NativePtr);
			var materialsCount = FbxNodeGetMaterialCount(NativePtr);

			Name = FbxNodeGetName(NativePtr);
			Materials = new FbxMaterial[materialsCount];
			for (int i = 0; i < materialsCount; i++) {
				Materials[i] = new FbxMaterial(FbxNodeGetMaterial(NativePtr, i));
				Materials[i].Name = $"{Materials[i].Name}-{Name}";
			}

			Attribute = FbxNodeAttribute.GetFromNode(NativePtr);

			for (int i = 0; i < nodeCount; i++) {
				Children.Add(new FbxNode(FbxNodeGetChildNode(NativePtr, i)));
			}

			LocalTranform = FbxNodeGetLocalTransform(NativePtr).ToStruct<Mat4x4>().ToLime();
		}

		#region PInvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FbxNodeGetChildNode(IntPtr node, int index);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern int FbxNodeGetChildCount(IntPtr node);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FbxNodeGetMaterial(IntPtr node, int idx);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FbxNodeGetLocalTransform(IntPtr node);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl, CharSet = ImportConfig.Charset)]
		private static extern string FbxNodeGetName(IntPtr node);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern int FbxNodeGetMaterialCount(IntPtr node);

		#endregion
	}
}
