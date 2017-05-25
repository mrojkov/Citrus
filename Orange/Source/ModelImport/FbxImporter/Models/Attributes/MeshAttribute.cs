using Lime;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Orange.FbxImporter
{
	public class MeshAttribute : NodeAttribute
	{
		public int[] Indices { get; private set; }

		public Mesh3D.Vertex[] Vertices { get; private set; }

		public override FbxNodeType Type { get; } = FbxNodeType.MESH;

		public MeshAttribute(IntPtr ptr) : base(ptr)
		{
			var mesh = FbxNodeGetMeshAttribute(NativePtr).To<MeshData>();
			Indices = mesh.vertices.ToIntArray(mesh.verticesCount);
			Vertices = new Mesh3D.Vertex[mesh.verticesCount];
			var colors = mesh.colors.ToStructArray<Vec4>(mesh.verticesCount);
			var verices = mesh.points.ToStructArray<Vec3>(mesh.verticesCount);
			var materialCount = FbxNodeGetMeshMaterialCount(NativePtr);
			var uv = mesh.uvCoords.ToStructArray<Vec2>(mesh.verticesCount);

			for (var i = 0; i < mesh.verticesCount; i++) {
				var vertex = 
				Vertices[i].Pos = new Vector3(
					verices[i].V1,
					verices[i].V2,
					verices[i].V3);
				Vertices[i].Color = colors != null ? 
					Color4.FromFloats(
						colors[i].V1,
						colors[i].V2,
						colors[i].V3,
						colors[i].V4) :
					Color4.White;
				Vertices[i].UV1 = new Vector2(
					uv[i].V1,
					uv[i].V2
				);
			}
		}	

		public override string ToString(int level)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("Points: ".ToLevel(level + 1));
			if (Vertices == null) {
				builder.AppendLine("None");
			} else {
				builder.AppendLine("[".ToLevel(level + 2));
				for (int i =0; i < Vertices.Length; i+=3) {
					builder.AppendLine($"[{Vertices[i]}, {Vertices[i + 1]}, {Vertices[i + 2]}],".ToLevel(level + 3));
				}
				builder.AppendLine("[".ToLevel(level + 2));
			}
			return builder.ToString();
		}

		#region Pinvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxNodeGetMeshAttribute(IntPtr node);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FbxNodeGetMeshMaterialCount(IntPtr pMesh);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxNodeGetMeshMaterial(IntPtr pMesh, int idx);

		#endregion

		#region MarshalingStructures

		[StructLayout(LayoutKind.Sequential)]
		private class MeshData
		{
			[MarshalAs(UnmanagedType.I4)]
			public int verticesCount;

			public IntPtr vertices;

			public IntPtr points;

			public IntPtr colors;

			public IntPtr uvCoords;
		}

		#endregion
	}
}
