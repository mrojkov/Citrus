using Lime;
using System;
using System.Runtime.InteropServices;

namespace Orange.FbxImporter
{
	public class FbxMaterial : FbxObject
	{
		internal static IMaterial Default = new CommonMaterial {
			Id = "Default",
		};

		public string Path { get; }

		public string Name { get; set; }

		public Lime.TextureWrapMode WrapModeU { get; }

		public Lime.TextureWrapMode WrapModeV { get; }

		public Color4 DiffuseColor { get; }

		public FbxMaterial(IntPtr ptr) : base(ptr)
		{
			if (ptr == IntPtr.Zero) {
				DiffuseColor = Color4.White;
			} else {
				var matPtr = FbxNodeSerializeMaterial(NativePtr);
				if (matPtr == IntPtr.Zero) return;
				var material = matPtr.ToStruct<Texture>();
				Path = material.TexturePath;
				Name = material.Name;
				WrapModeU = (Lime.TextureWrapMode)material.WrapModeU;
				WrapModeV = (Lime.TextureWrapMode)material.WrapModeV;
				DiffuseColor = material.ColorDiffuse.ToStruct<Vec4>().ToLimeColor();
			}

		}

		#region Pinvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FbxNodeSerializeMaterial(IntPtr node);

		#endregion

		#region MarshalingStructures

		private enum TextureWrapMode
		{
			Clamp,
			Repeat
		}

		[StructLayout(LayoutKind.Sequential, CharSet = ImportConfig.Charset)]
		private class Texture
		{
			public TextureWrapMode WrapModeU;

			public TextureWrapMode WrapModeV;

			public string TexturePath;

			public string Name;

			public IntPtr ColorDiffuse;
		}

		#endregion
	}
}
