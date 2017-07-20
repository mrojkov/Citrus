using Lime;
using Orange.FbxImporter;
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

		public int MaterialIndex { get; private set; }

		public Mesh3D.Vertex[] Vertices { get; private set; }

		public Bone[] Bones { get; private set; }

		public override FbxNodeType Type { get; } = FbxNodeType.MESH;

		public MeshAttribute(IntPtr ptr) : base(ptr)
		{
			var native = FbxNodeGetMeshAttribute(NativePtr, true);
			if (native == IntPtr.Zero) {
				throw new FbxAtributeImportException(Type);
			}
			var mesh = native.To<MeshData>();
			var colors = mesh.colors.ToStructArray<Vec4>(mesh.verticesCount);
			var verices = mesh.points.ToStructArray<Vec3>(mesh.verticesCount);

			var uv = mesh.uvCoords.ToStructArray<Vec2>(mesh.verticesCount);
			var weights = mesh.weights.ToStructArray<WeightData>(mesh.verticesCount);
			var bones = mesh.bones.ToStructArray<BoneData>(mesh.boneCount);

			Indices = mesh.vertices.ToIntArray(mesh.verticesCount);
			MaterialIndex = mesh.materialIndex;
			Vertices = new Mesh3D.Vertex[mesh.verticesCount];
			Bones = new Bone[mesh.boneCount];

			for (int i = 0; i < mesh.boneCount; i++ ) {
				Bones[i] = new Bone();
				Bones[i].Name = bones[i].name.ToCharArray();
				Bones[i].Offset = bones[i].offset.ToStruct<Mat4x4>().ToLime();
			}

			for (var i = 0; i < mesh.verticesCount; i++) {
				var vertex =
				Vertices[i].Pos = verices[i].toLime();
				Vertices[i].Color = colors != null ? colors[i].toLimeColor() : Color4.White;
				Vertices[i].UV1 = uv[i].toLime();

				byte index;
				float weight;

				for (int j = 0; j < ImportConfig.BoneLimit; j++) {
					if (weights[i].Weights[j] != -1) {
						index = weights[i].Indices[j];
						weight = weights[i].Weights[j];
						switch (j) {
							case 0:
								Vertices[i].BlendIndices.Index0 = index;
								Vertices[i].BlendWeights.Weight0 = weight;
								break;
							case 1:
								Vertices[i].BlendIndices.Index1 = index;
								Vertices[i].BlendWeights.Weight1 = weight;
								break;
							case 2:
								Vertices[i].BlendIndices.Index2 = index;
								Vertices[i].BlendWeights.Weight2 = weight;
								break;
							case 3:
								Vertices[i].BlendIndices.Index3 = index;
								Vertices[i].BlendWeights.Weight3 = weight;
								break;
							default:
								break;
						}
					}
				}
			}
		}	

		#region Pinvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxNodeGetMeshAttribute(IntPtr node, bool IsLimitBoneWeights);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxNodeGetMeshMaterial(IntPtr pMesh, int idx);

		#endregion

		#region MarshalingStructures

		[StructLayout(LayoutKind.Sequential)]
		private class MeshData
		{
			public IntPtr vertices;

			public IntPtr points;

			public IntPtr colors;

			public IntPtr uvCoords;

			public IntPtr weights;

			public IntPtr bones;

			[MarshalAs(UnmanagedType.I4)]
			public int materialIndex;

			[MarshalAs(UnmanagedType.I4)]
			public int verticesCount;

			[MarshalAs(UnmanagedType.I4)]
			public int boneCount;
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
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = ImportConfig.BoneLimit)]
			public byte[] Indices;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = ImportConfig.BoneLimit)]
			public float[] Weights;
		}

		#endregion
	}
}
