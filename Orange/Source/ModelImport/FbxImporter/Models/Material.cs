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

		//add Lime Color

		#region Pinvokes

		//Get non native material
		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SerializeMaterial(IntPtr node);

		#endregion

		public Material(IntPtr ptr) : base(ptr)
		{
			var matPtr = SerializeMaterial(NativePtr);
			if (matPtr != IntPtr.Zero) {
				var material = matPtr.To<Texture>();
				Path = material.texturePath;
				var color = material.colorDiffuse;
			}
		}

		public override string ToString(int level)
		{
			return ("Path: " + Path);
		}

		[StructLayout(LayoutKind.Sequential)]
		private class Texture
		{
			[MarshalAs(UnmanagedType.LPStr)]
			public string texturePath;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
			public double[] colorDiffuse;
		}
	}
}
