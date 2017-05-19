using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Orange.FbxImporter
{
	public class MeshAttribute : NodeAttribute
	{
		public int[] Indices { get; private set; }

		public Mesh3D.Vertex[] Vertices { get; private set; }

		public override FbxNodeType Type { get; } = FbxNodeType.MESH;

		public MeshAttribute(IntPtr ptr) : base(ptr)
		{
			var mesh = GetMeshAttribute(NativePtr).To<MeshData>();
			Indices = mesh.vertices.ToIntArray(mesh.verticesCount);
			Vertices = new Mesh3D.Vertex[mesh.verticesCount];
			var colors = mesh.colors.ToStructArray<Vec4>(mesh.colorsCount);
			var verices = mesh.points.ToStructArray<Vec3>(mesh.pointsCount);
			var uv = mesh.uvCoords.ToStructArray<Vec2>(mesh.uvCount);
			for (var i = 0; i < mesh.pointsCount; i++) {
				var vertex = 
				Vertices[i].Pos = new Vector3(
					verices[i].V1,
					verices[i].V2,
					verices[i].V3);
				Vertices[i].Color = colors.Length != 0 ?
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
		public static extern IntPtr GetMeshAttribute(IntPtr node);

		#endregion

		#region MarshalingStructures

		[StructLayout(LayoutKind.Sequential)]
		private class MeshData
		{
			public IntPtr vertices;

			[MarshalAs(UnmanagedType.I4)]
			public int verticesCount;

			public IntPtr points;

			[MarshalAs(UnmanagedType.I4)]
			public int pointsCount;

			public IntPtr colors;

			[MarshalAs(UnmanagedType.I4)]
			public int colorsCount;

			public IntPtr uvCoords;

			[MarshalAs(UnmanagedType.I4)]
			public int uvCount;

			~MeshData()
			{
				Utils.ReleaseNative(vertices);
				Utils.ReleaseNative(colors);
				Utils.ReleaseNative(uvCoords);
				Utils.ReleaseNative(points);
			}
		}
		#endregion
	}
}
