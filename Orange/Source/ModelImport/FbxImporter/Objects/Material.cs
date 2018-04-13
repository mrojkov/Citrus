using Lime;
using System;
using System.Runtime.InteropServices;

namespace Orange.FbxImporter
{
	public class Material : FbxObject
	{
		internal static IMaterial Default = new CommonMaterial {
			Name = "Default",
		};

		public string Path { get; }

		public string Name { get; }

		public Color4 DiffuseColor { get; }

		public Material(IntPtr ptr) : base(ptr)
		{
			if (ptr == IntPtr.Zero) {
				DiffuseColor = Color4.White;
			} else {
				var matPtr = FbxNodeSerializeMaterial(NativePtr);
				if (matPtr == IntPtr.Zero) return;
				var material = matPtr.ToStruct<Texture>();
				Path = material.TexturePath;
				Name = material.Name;
				DiffuseColor = material.ColorDiffuse.ToLimeColor();
			}

		}

		public CommonMaterial ToLime(string path)
		{
			var material = new CommonMaterial {
				Name = Name
			};
			if (!string.IsNullOrEmpty(Path)) {
				material.DiffuseTexture = CreateSerializableTexture(path);
			}
			material.DiffuseColor = DiffuseColor;
			return material;
		}

		private SerializableTexture CreateSerializableTexture(string root)
		{
			var texturePath = Toolbox.ToUnixSlashes(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(root),
				System.IO.Path.GetFileNameWithoutExtension(Toolbox.ToUnixSlashes(Path))));
			return new SerializableTexture(texturePath);
		}

		#region Pinvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FbxNodeSerializeMaterial(IntPtr node);

		#endregion

		#region MarshalingStructures

		[StructLayout(LayoutKind.Sequential, CharSet = ImportConfig.Charset)]
		public class Texture
		{
			public string TexturePath;

			public string Name;

			public Vec4 ColorDiffuse;
		}

		#endregion
	}
}
