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

		public string Path { get; private set; }

		public string Name { get; private set; }

		public Color4 DiffuseColor { get; private set; }

		public Material(IntPtr ptr) : base(ptr)
		{
			if (ptr == IntPtr.Zero) {
				DiffuseColor = Color4.White;
			} else {
				var matPtr = FbxNodeSerializeMaterial(NativePtr);
				if (matPtr != IntPtr.Zero) {
					var material = matPtr.To<Texture>();
					Path = material.texturePath;
					Name = material.Name;
					var color = material.colorDiffuse.ToStruct<Vec4> ();
					DiffuseColor = Color4.FromFloats(
						color.V1,
						color.V2,
						color.V3,
						color.V4
					);
				}
			}
			
		}

		public CommonMaterial ToLime(string path) 
		{
			var res = new CommonMaterial();
			res.Name = Name;
			if (!string.IsNullOrEmpty(Path)) {
				res.DiffuseTexture = CreateSerializableTexture(path);
			}
			if (DiffuseColor != null) {
				res.DiffuseColor = DiffuseColor;
			}

			return res;
		}

		private SerializableTexture CreateSerializableTexture(string root)
		{
			var texturePath = Toolbox.ToUnixSlashes(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(root),
				System.IO.Path.GetFileNameWithoutExtension(Toolbox.ToUnixSlashes(Path))));
			return new SerializableTexture(texturePath);
		}

		#region Pinvokes

		//Get non native material
		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxNodeSerializeMaterial(IntPtr node);

		#endregion

		#region MarshalingStructures

		[StructLayout(LayoutKind.Sequential)]
		public class Texture
		{
			[MarshalAs(UnmanagedType.LPStr)]
			public string texturePath;

			public IntPtr colorDiffuse;

			[MarshalAs(UnmanagedType.LPStr)]
			public string Name;
		}

		
		#endregion
	}
}
