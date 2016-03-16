using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using System.Runtime.InteropServices;
namespace Lime
{
	interface IPlatformMesh : IDisposable
	{
		void Render(int startIndex, int indexCount);
	}

	[ProtoContract]
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size=4)]
	public struct BlendIndices
	{
		[ProtoMember(1)]
		public byte Index0;

		[ProtoMember(2)]
		public byte Index1;

		[ProtoMember(3)]
		public byte Index2;

		[ProtoMember(4)]
		public byte Index3;
	}

	[ProtoContract]
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
	public struct BlendWeights
	{
		[ProtoMember(1)]
		public float Weight0;

		[ProtoMember(2)]
		public float Weight1;

		[ProtoMember(3)]
		public float Weight2;

		[ProtoMember(4)]
		public float Weight3;
	}

	[ProtoContract]
	public class Mesh : IDisposable
	{
		[Flags]
		public enum Attributes
		{
			None = 0,
			Vertex = 1 << 0,
			Color = 1 << 1,
			UV1 = 1 << 2,
			UV2 = 1 << 3,
			UV3 = 1 << 4,
			UV4 = 1 << 5,
			BlendIndices = 1 << 6,
			BlendWeights = 1 << 7,
			Blend = BlendIndices | BlendWeights,
			VertexColorUV12 = Vertex | Color | UV1 | UV2,
			All = Vertex | Color | UV1 | UV2 | UV3 | UV4 | Blend
		}

		[ProtoMember(1)]
		public Vector3[] Vertices;

		[ProtoMember(2)]
		public Color4[] Colors;

		[ProtoMember(3)]
		public Vector2[] UV1;

		[ProtoMember(4)]
		public Vector2[] UV2;

		[ProtoMember(5)]
		public Vector2[] UV3;

		[ProtoMember(6)]
		public Vector2[] UV4;

		[ProtoMember(7)]
		public ushort[] Indices;

		[ProtoMember(8)]
		public BlendIndices[] BlendIndices;

		[ProtoMember(9)]
		public BlendWeights[] BlendWeights;

		public Attributes DirtyAttributes;
		public bool IndicesDirty;

		private IPlatformMesh platformMesh;

		[ProtoAfterDeserialization]
		public void AfterDeserialization()
		{
			DirtyAttributes = Attributes.All;
		}

		public void Render(int startIndex, int indexCount)
		{
			if (platformMesh == null) {
				platformMesh = new PlatformMesh(this);
			}
			platformMesh.Render(startIndex, indexCount);
		}

		public void Allocate(int vertexCount, int indexCount, Attributes attrs)
		{
			if ((attrs & Attributes.Vertex) != 0) {
				Array.Resize(ref Vertices, vertexCount);
			} else {
				Vertices = null;
			}
			if ((attrs & Attributes.Color) != 0) {
				Array.Resize(ref Colors, vertexCount);
			} else {
				Colors = null;
			}
			if ((attrs & Attributes.UV1) != 0) {
				Array.Resize(ref UV1, vertexCount);
			} else {
				UV1 = null;
			}
			if ((attrs & Attributes.UV2) != 0) {
				Array.Resize(ref UV2, vertexCount);
			} else {
				UV2 = null;
			}
			if ((attrs & Attributes.UV3) != 0) {
				Array.Resize(ref UV3, vertexCount);
			} else {
				UV3 = null;
			}
			if ((attrs & Attributes.UV4) != 0) {
				Array.Resize(ref UV4, vertexCount);
			} else {
				UV4 = null;
			}
			if ((attrs & Attributes.BlendIndices) != 0) {
				Array.Resize(ref BlendIndices, vertexCount);
			} else {
				BlendIndices = null;
			}
			if ((attrs & Attributes.BlendWeights) != 0) {
				Array.Resize(ref BlendWeights, vertexCount);
			} else {
				BlendWeights = null;
			}
			if (Indices == null || Indices.Length != indexCount) {
				Array.Resize(ref Indices, indexCount);
			} else {
				Indices = null;
			}
		}

		public void Dispose()
		{
			if (platformMesh != null) {
				platformMesh.Dispose();
				platformMesh = null;
			}
		}
	}
}
