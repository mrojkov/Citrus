using System;
using System.Runtime.InteropServices;

namespace Orange.FbxImporter
{
	public class NodeAttribute : FbxObject
	{
		public static NodeAttribute Empty = new NodeAttribute(IntPtr.Zero);

		public virtual FbxNodeType Type { get; } = FbxNodeType.NONE;

		protected NodeAttribute(IntPtr ptr) : base(ptr)
		{ }

		public enum FbxNodeType
		{
			NONE,
			UNKNOWN,
			MESH,
			CAMERA
		};

		public static NodeAttribute GetFromNode(IntPtr ptr, int idx)
		{
			var attribute = FbxNodeGetAttribute(ptr, idx);
			if (attribute == IntPtr.Zero) {
				return NodeAttribute.Empty;
			}
			switch (FbxNodeGetAttributeType(ptr, idx)) {
				case FbxNodeType.NONE:
					return Empty;
				case FbxNodeType.UNKNOWN:
					return Empty;
				case FbxNodeType.MESH:
					return new MeshAttribute(attribute);
				case FbxNodeType.CAMERA:
					return new CameraAttribute(attribute);
				default:
					return Empty;
			}
		}

		#region Pinvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern FbxNodeType FbxNodeGetAttributeType(IntPtr node, int idx);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxNodeGetAttribute(IntPtr node, int idx);

		#endregion
	}
}
