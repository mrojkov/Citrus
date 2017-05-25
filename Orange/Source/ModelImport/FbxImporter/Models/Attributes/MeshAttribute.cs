using Lime;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Orange.FbxImporter
{
	public class Bone
	{
		public Matrix44 Offset { get; set; }

		public string Name { get; set; }
	}

	public class MeshAttribute : NodeAttribute
	{
		public int[] Indices { get; private set; }

		public Mesh3D.Vertex[] Vertices { get; private set; }

		public Bone[] Bones { get; private set; }

		public override FbxNodeType Type { get; } = FbxNodeType.MESH;

		public MeshAttribute(IntPtr ptr) : base(ptr)
		{
			var mesh = FbxNodeGetMeshAttribute(NativePtr).To<MeshData>();
			var colors = mesh.colors.ToStructArray<Vec4>(mesh.verticesCount);
			var verices = mesh.points.ToStructArray<Vec3>(mesh.verticesCount);
			var materialCount = FbxNodeGetMeshMaterialCount(NativePtr);
			var uv = mesh.uvCoords.ToStructArray<Vec2>(mesh.verticesCount);
			var weights = mesh.weights.ToStructArray<WeightData>(mesh.verticesCount);
			var bones = mesh.bones.ToStructArray<BoneData>(mesh.boneCount);

			Indices = mesh.vertices.ToIntArray(mesh.verticesCount);
			Vertices = new Mesh3D.Vertex[mesh.verticesCount];
			Bones = new Bone[mesh.boneCount];

			for (int i = 0; i < mesh.boneCount; i++ ) {
				Bones[i] = new Bone();
				Bones[i].Name = bones[i].name.ToCharArray();
				Bones[i].Offset = bones[i].offset.ToStruct<Mat4x4>().ToLime();
			}

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
				
				if (weights[i].I1 != -1) {
					Vertices[i].BlendIndices.Index0 = (byte)weights[i].I1;
					Vertices[i].BlendWeights.Weight0 = weights[i].W1;
				}

				if (weights[i].I2 != -1) {
					Vertices[i].BlendIndices.Index1 = (byte)weights[i].I2;
					Vertices[i].BlendWeights.Weight1 = weights[i].W2;
				}

				if (weights[i].I3 != -1) {
					Vertices[i].BlendIndices.Index2 = (byte)weights[i].I3;
					Vertices[i].BlendWeights.Weight2 = weights[i].W3;
				}

				if (weights[i].I4 != -1) {
					Vertices[i].BlendIndices.Index3 = (byte)weights[i].I4;
					Vertices[i].BlendWeights.Weight3 = weights[i].W4;
				}
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

			public IntPtr weights;

			[MarshalAs(UnmanagedType.I4)]
			public int boneCount;

			public IntPtr bones;
		}

		[StructLayout(LayoutKind.Sequential)]
		private class BoneData
		{
			public IntPtr name;

			public IntPtr offset;
		}

		[StructLayout(LayoutKind.Sequential)]
		private class WeightData
		{
			[MarshalAs(UnmanagedType.R4)]
			public float W1;

			[MarshalAs(UnmanagedType.I1)]
			public sbyte I1;

			[MarshalAs(UnmanagedType.R4)]
			public float W2;

			[MarshalAs(UnmanagedType.I1)]
			public sbyte I2;

			[MarshalAs(UnmanagedType.R4)]
			public float W3;

			[MarshalAs(UnmanagedType.I1)]
			public sbyte I3;

			[MarshalAs(UnmanagedType.R4)]
			public float W4;

			[MarshalAs(UnmanagedType.I1)]
			public sbyte I4;
		}

		#endregion
	}
}
