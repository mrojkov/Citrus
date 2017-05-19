using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Orange.FbxImporter
{
	public class Material : FbxObject
	{
		public string Path { get; private set; }

		public Color4 DiffuseColor { get; private set; }

		public Material(IntPtr ptr) : base(ptr)
		{
			var matPtr = SerializeMaterial(NativePtr);
			if (matPtr != IntPtr.Zero) {
				var material = matPtr.To<Texture>();
				Path = material.texturePath;
				DiffuseColor = Color4.FromFloats(
					material.colorDiffuse.V1,
					material.colorDiffuse.V2,
					material.colorDiffuse.V3,
					material.colorDiffuse.V4
				);
			}
		}

		public override string ToString(int level)
		{
			return ("Path: " + Path);
		}

		#region Pinvokes

		//Get non native material
		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SerializeMaterial(IntPtr node);

		#endregion

		#region MarshalingStructures

		[StructLayout(LayoutKind.Sequential)]
		private class Texture
		{
			[MarshalAs(UnmanagedType.LPStr)]
			public string texturePath;

			[MarshalAs(UnmanagedType.Struct)]
			public Vec4 colorDiffuse;
		}

		#endregion
	}
}
