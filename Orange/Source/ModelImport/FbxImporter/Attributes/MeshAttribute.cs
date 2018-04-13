using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using Lime;

namespace Orange.FbxImporter
{
	public class Bone
	{
		public Matrix44 Offset { get; set; }

		public string Name { get; set; }
	}

	public class Submesh
	{
		public int[] Indices { get; set; }

		public int MaterialIndex { get; set; }

		public Vector3[] Normals { get; set; }

		public Mesh3D.Vertex[] Vertices { get; set; }

		public Bone[] Bones { get; set; }
	}

	public class MeshAttribute : NodeAttribute
	{
		private const float NoWeight = -1;

		public List<Submesh> Submeshes { get; private set; } = new List<Submesh>();

		public override FbxNodeType Type { get; } = FbxNodeType.Mesh;

		private MeshAttribute() : base(IntPtr.Zero)
		{
		}

		public MeshAttribute(IntPtr ptr) : base(ptr)
		{
			Submeshes = ImportSubmeshes(ptr);
		}

		private static List<Submesh> ImportSubmeshes(IntPtr ptr)
		{
			var list = new List<Submesh>();
			var mesh = FbxNodeGetMeshAttribute(ptr, true);
			var indices = mesh.Vertices.ToStruct<SizedArray>().GetData<int>();
			var controlPoints = mesh.Points.ToStruct<SizedArray>().GetData<Vec3>();
			var weights = mesh.Weigths.ToStruct<SizedArray>().GetData<WeightData>();
			var boneData = mesh.Bones.ToStruct<SizedArray>().GetData<BoneData>();
			var colorsContainer = mesh.Colors.ToStruct<Element>();
			var normalsContainer = mesh.Normals.ToStruct<Element>();
			var uvContainer = mesh.UV.ToStruct<Element>();
			var colors = colorsContainer.GetData<Vec4>();
			var normals = normalsContainer.GetData<Vec3>();
			var uv = uvContainer.GetData<Vec2>();

			var size = ushort.MaxValue;
			var count = indices.Length / size;
			var bones = new Bone[boneData.Length];

			for (var i = 0; i < boneData.Length; i++) {
				bones[i] = new Bone {
					Name = boneData[i].Name,
					Offset = boneData[i].OffsetMatrix.ToStruct<Mat4x4>().ToLime()
				};
			}

			for (var i = 0; i <= count; i++) {
				var newSize = i == count ? indices.Length - (size * count) : size;

				var submesh = new Submesh {
					MaterialIndex = mesh.MaterialIndex,
					Indices = new int[newSize],
					Vertices = new Mesh3D.Vertex[newSize],
					Normals = new Vector3[newSize],
					Bones = bones.ToArray(),
				};

				for (var j = 0; j < submesh.Vertices.Length; j++) {
					var index = i * size + j;
					var controlPointIndex = indices[index];
					var controlPoint = controlPoints[controlPointIndex];
					submesh.Indices[j] = j;
					submesh.Vertices[j].Pos = controlPoint.ToLime();
					if (colorsContainer.Size != 0 && colorsContainer.Mode != ReferenceMode.None) {
						submesh.Vertices[j].Color = colorsContainer.Mode == ReferenceMode.ControlPoint ?
							colors[controlPointIndex].ToLimeColor() : colors[index].ToLimeColor();
					} else {
						submesh.Vertices[j].Color = Color4.White;
					}

					if (normalsContainer.Size != 0 && normalsContainer.Mode != ReferenceMode.None) {
						submesh.Vertices[j].Normal = normalsContainer.Mode == ReferenceMode.ControlPoint ?
							normals[controlPointIndex].ToLime() : normals[index].ToLime();
					}

					if (uvContainer.Size != 0 && uvContainer.Mode != ReferenceMode.None) {
						submesh.Vertices[j].UV1 = normalsContainer.Mode == ReferenceMode.ControlPoint ?
							uv[controlPointIndex].ToLime() : uv[index].ToLime();
						submesh.Vertices[j].UV1.Y = 1 - submesh.Vertices[j].UV1.Y;
					}

					if (weights.Length == 0) continue;
					byte idx;
					float weight;
					var weightData = weights[controlPointIndex];
					for (var k = 0; k < ImportConfig.BoneLimit; k++) {
						if (weightData.Weights[k] == NoWeight) continue;
						idx = weightData.Indices[k];
						weight = weightData.Weights[k];
						switch (k) {
							case 0:
								submesh.Vertices[j].BlendIndices.Index0 = idx;
								submesh.Vertices[j].BlendWeights.Weight0 = weight;
								break;
							case 1:
								submesh.Vertices[j].BlendIndices.Index1 = idx;
								submesh.Vertices[j].BlendWeights.Weight1 = weight;
								break;
							case 2:
								submesh.Vertices[j].BlendIndices.Index2 = idx;
								submesh.Vertices[j].BlendWeights.Weight2 = weight;
								break;
							case 3:
								submesh.Vertices[j].BlendIndices.Index3 = idx;
								submesh.Vertices[j].BlendWeights.Weight3 = weight;
								break;
						}
					}
				}
				list.Add(submesh);
			}
			return list;
		}

		public static MeshAttribute Combine(MeshAttribute meshAttribute1, MeshAttribute meshAttribute2)
		{
			var sm = new List<Submesh>();
			sm.AddRange(meshAttribute1.Submeshes);
			sm.AddRange(meshAttribute2.Submeshes);
			return new MeshAttribute {
				Submeshes = sm
			};
		}

		#region Pinvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern MeshData FbxNodeGetMeshAttribute(IntPtr node, bool limitBoneWeights);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FbxNodeGetMeshMaterial(IntPtr pMesh, int idx);

		#endregion

		#region MarshalingStructures

		private enum ReferenceMode
		{
			None,
			ControlPoint,
			PolygonVertex
		}

		[StructLayout(LayoutKind.Sequential)]
		private class MeshData
		{
			public IntPtr Vertices;

			public IntPtr Points;

			public IntPtr Weigths;

			public IntPtr Colors;

			public IntPtr UV;

			public IntPtr Normals;

			public IntPtr Bones;

			[MarshalAs(UnmanagedType.I4)]
			public int MaterialIndex;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = ImportConfig.Charset)]
		private class BoneData
		{
			public string Name;

			public IntPtr OffsetMatrix;
		}

		[StructLayout(LayoutKind.Sequential)]
		private class WeightData
		{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = ImportConfig.BoneLimit)]
			public byte[] Indices;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = ImportConfig.BoneLimit)]
			public float[] Weights;
		}

		[StructLayout(LayoutKind.Sequential)]
		private class Element : SizedArray
		{
			public ReferenceMode Mode;
		}

		#endregion
	}
}
