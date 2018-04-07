using System;
using System.Runtime.InteropServices;

namespace Orange.FbxImporter
{
	public class NodeAttribute : FbxObject
	{
		public static NodeAttribute Empty = new NodeAttribute(IntPtr.Zero);

		public virtual FbxNodeType Type { get; } = FbxNodeType.None;

		protected NodeAttribute(IntPtr ptr) : base(ptr)
		{ }

		public enum FbxNodeType
		{
			None,
			Mesh,
			Camera
		};

		public static NodeAttribute GetFromNode(IntPtr ptr)
		{
			var attribute = FbxNodeGetAttribute(ptr);
			if (attribute == IntPtr.Zero) {
				return Empty;
			}
			switch (FbxNodeGetAttributeType(ptr)) {
				case FbxNodeType.Mesh:
					var meshAttribute = MeshAttribute.FromSubmesh(attribute);
					var count = FbxNodeGetAttributeCount(ptr);
					for (var i = 1; i < count; i++) {
						meshAttribute = MeshAttribute.Combine(
							meshAttribute,
							MeshAttribute.FromSubmesh(FbxNodeGetAttribute(ptr, i)));
					}
					return meshAttribute;
				case FbxNodeType.Camera:
					return new CameraAttribute(attribute);
				default:
					return Empty;
			}
		}

		#region Pinvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern FbxNodeType FbxNodeGetAttributeType(IntPtr node, int idx = 0);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxNodeGetAttribute(IntPtr node, int idx = 0);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FbxNodeGetAttributeCount(IntPtr node);

		#endregion
	}
}
