using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Orange.FbxImporter
{
	public class NodeAttribute : FbxObject
	{
		#region Pinvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern FbxNodeType GetAttributeType(IntPtr node);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr GetCameraAttribute(IntPtr node);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr GetAttribute(IntPtr node);

		#endregion

		public static NodeAttribute Empty = new NodeAttribute(IntPtr.Zero);

		protected NodeAttribute(IntPtr ptr) : base(ptr)
		{ }

		public enum FbxNodeType
		{
			NONE,
			UNKNOWN,
			MESH,
			CAMERA
		};

		public static NodeAttribute GetFromNode(IntPtr ptr)
		{
			var attribute = GetAttribute(ptr);
			switch (GetAttributeType(ptr)) {
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
		public override string ToString(int level)
		{
			return "Empty";
		}
	}
}
