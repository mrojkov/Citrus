using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using Lime;

namespace Orange.FbxImporter
{
	public class FbxBone
	{
		public Matrix44 Offset { get; set; }

		public string Name { get; set; }
	}

	public class FbxSubmesh
	{
		public int[] Indices { get; set; }

		public int MaterialIndex { get; set; }

		public Vector3[] Normals { get; set; }

		public Mesh3D.Vertex[] Vertices { get; set; }

		public FbxBone[] Bones { get; set; }
	}

	public class FbxMeshAttribute : FbxNodeAttribute
	{
		private const float NoWeight = -1;

		public List<FbxSubmesh> Submeshes { get; private set; } = new List<FbxSubmesh>();

		public override FbxNodeType Type { get; } = FbxNodeType.Mesh;

		public SkinningMode SkinningMode;

		private FbxMeshAttribute() : base(IntPtr.Zero) { }

		public FbxMeshAttribute(IntPtr ptr) : base(ptr)
		{
			Submeshes = ImportSubmeshes(ptr);
		}

		private List<FbxSubmesh> ImportSubmeshes(IntPtr ptr)
		{
			var list = new List<FbxSubmesh>();
			var mesh = FbxNodeGetMeshAttribute(ptr, WindingOrder.CW, true);
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
			SkinningMode = GetSkinningMode(mesh.SkinningMode);
			var size = ushort.MaxValue;
			var count = indices.Length / size;
			var bones = new FbxBone[boneData.Length];

			for (var i = 0; i < boneData.Length; i++) {
				bones[i] = new FbxBone {
					Name = boneData[i].Name,
					Offset = boneData[i].OffsetMatrix.ToStruct<Mat4x4>().ToLime()
				};
			}

			for (var i = 0; i <= count; i++) {
				var newSize = i == count ? indices.Length - (size * count) : size;

				var submesh = new FbxSubmesh {
					MaterialIndex = mesh.MaterialIndex,
					Indices = new int[newSize],
					Vertices = new Mesh3D.Vertex[newSize],
					Normals = new Vector3[newSize],
					Bones = bones,
				};
				var hasColors = colorsContainer.Size != 0 && colorsContainer.Mode != ReferenceMode.None;
				var hasNormals = normalsContainer.Size != 0 && normalsContainer.Mode != ReferenceMode.None;
				var hasUV = uvContainer.Size != 0 && uvContainer.Mode != ReferenceMode.None;
				for (var j = 0; j < submesh.Vertices.Length; j++) {
					var index = i * size + j;
					var controlPointIndex = indices[index];
					var controlPoint = controlPoints[controlPointIndex];
					submesh.Indices[j] = j;
					submesh.Vertices[j].Pos = controlPoint.ToLime();
					if (hasColors) {
						submesh.Vertices[j].Color = colorsContainer.Mode == ReferenceMode.ControlPoint ?
							colors[controlPointIndex].ToLimeColor() : colors[index].ToLimeColor();
					} else {
						submesh.Vertices[j].Color = Color4.White;
					}

					if (hasNormals) {
						submesh.Vertices[j].Normal = normalsContainer.Mode == ReferenceMode.ControlPoint ?
							normals[controlPointIndex].ToLime() : normals[index].ToLime();
					}

					if (hasUV) {
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
				if (hasUV) {
					if (!hasNormals) {
						ComputeNormals(submesh.Vertices, submesh.Indices);
					}
					ComputeTangents(submesh.Vertices, submesh.Indices);
				}
				list.Add(submesh);
			}
			return list;
		}

		private static void ComputeNormals(Mesh3D.Vertex[] vertices, int[] indices)
		{
			for (var i = 0; i < vertices.Length; i++) {
				vertices[i].Normal = Vector3.Zero;
			}
			for (var i = 0; i < indices.Length - 2; i += 3) {
				var idx1 = indices[i];
				var idx2 = indices[i + 1];
				var idx3 = indices[i + 2];
				var v1 = vertices[idx1];
				var v2 = vertices[idx2];
				var v3 = vertices[idx3];
				var e1 = v2.Pos - v1.Pos;
				var e2 = v3.Pos - v1.Pos;
				var n = Vector3.CrossProduct(e1, e2).Normalized;
				vertices[idx1].Normal += n;
				vertices[idx2].Normal += n;
				vertices[idx3].Normal += n;
			}
			for (var i = 0; i < vertices.Length; i++) {
				vertices[i].Normal = vertices[i].Normal.Normalized;
			}
		}

		private static void ComputeTangents(Mesh3D.Vertex[] vertices, int[] indices)
		{
			var tangents = new Vector3[vertices.Length];
			var bitangents = new Vector3[vertices.Length];
			for (var i = 0; i < indices.Length - 2; i += 3) {
				var idx1 = indices[i];
				var idx2 = indices[i + 1];
				var idx3 = indices[i + 2];
				var v1 = vertices[idx1];
				var v2 = vertices[idx2];
				var v3 = vertices[idx3];
				var d1p = v2.Pos - v1.Pos;
				var d2p = v3.Pos - v1.Pos;
				var d1uv = v2.UV1 - v1.UV1;
				var d2uv = v3.UV1 - v1.UV1;
				var r = 1.0f / (d1uv.X * d2uv.Y - d2uv.X * d1uv.Y);
				var tangent = ((d1p * d2uv.Y - d2p * d1uv.Y) * r).Normalized;
				var bitangent = ((d2p * d1uv.X - d1p * d2uv.X) * r).Normalized;
				tangents[idx1] += tangent;
				tangents[idx2] += tangent;
				tangents[idx3] += tangent;
				bitangents[idx1] += bitangent;
				bitangents[idx2] += bitangent;
				bitangents[idx3] += bitangent;
			}
			for (var i = 0; i < vertices.Length; i++) {
				var n = vertices[i].Normal;
				var t = tangents[i];
				Orthonormalize(ref n, ref t);
				if (Vector3.DotProduct(Vector3.CrossProduct(n, t), bitangents[i]) < 0.0f) {
					t = -t;
				}
				vertices[i].Tangent = t;
			}
		}

		private static void Orthonormalize(ref Vector3 a, ref Vector3 b)
		{
			a = a.Normalized;
			b -= Vector3.DotProduct(a, b) * a;
			b = b.Normalized;
		}

		private SkinningMode GetSkinningMode(FbxSkinningMode meshSkinningMode)
		{
			switch (meshSkinningMode) {
				case FbxSkinningMode.Linear:
					return SkinningMode.Linear;
				case FbxSkinningMode.DualQuaternion:
					return SkinningMode.DualQuaternion;
				default:
					throw new ArgumentOutOfRangeException(nameof(meshSkinningMode), meshSkinningMode, null);
			}
		}

		public static FbxMeshAttribute Combine(FbxMeshAttribute meshAttribute1, FbxMeshAttribute meshAttribute2)
		{
			var sm = new List<FbxSubmesh>();
			sm.AddRange(meshAttribute1.Submeshes);
			sm.AddRange(meshAttribute2.Submeshes);
			var skinningMode = meshAttribute1.SkinningMode == meshAttribute2.SkinningMode ?
				meshAttribute1.SkinningMode : SkinningMode.DualQuaternion;
			return new FbxMeshAttribute {
				SkinningMode = skinningMode,
				Submeshes = sm
			};
		}

		#region Pinvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern MeshData FbxNodeGetMeshAttribute(IntPtr node, WindingOrder preferedWindingOrder, bool limitBoneWeights);

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

		private enum FbxSkinningMode
		{
			Linear,
			DualQuaternion,
		}

		private enum WindingOrder
		{
			CCW,
			CW
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

			public FbxSkinningMode SkinningMode;

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
