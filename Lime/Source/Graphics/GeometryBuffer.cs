using System;
using Yuzu;
using System.Runtime.InteropServices;

namespace Lime
{
	interface IPlatformGeometryBuffer : IDisposable
	{
		void Render(int startIndex, int indexCount);
	}

	[YuzuCompact]
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size=4)]
	public struct BlendIndices
	{
		[YuzuMember("0")]
		public byte Index0;

		[YuzuMember("1")]
		public byte Index1;

		[YuzuMember("2")]
		public byte Index2;

		[YuzuMember("3")]
		public byte Index3;
	}

	[YuzuCompact]
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
	public struct BlendWeights
	{
		[YuzuMember("0")]
		public float Weight0;

		[YuzuMember("1")]
		public float Weight1;

		[YuzuMember("2")]
		public float Weight2;

		[YuzuMember("3")]
		public float Weight3;
	}

	public class GeometryBuffer : IDisposable
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

		[YuzuMember]
		public Vector3[] Vertices;

		[YuzuMember]
		public Color4[] Colors;

		[YuzuMember]
		public Vector2[] UV1;

		[YuzuMember]
		public Vector2[] UV2;

		[YuzuMember]
		public Vector2[] UV3;

		[YuzuMember]
		public Vector2[] UV4;

		[YuzuMember]
		public ushort[] Indices;

		[YuzuMember]
		public BlendIndices[] BlendIndices;

		[YuzuMember]
		public BlendWeights[] BlendWeights;

		public Attributes DirtyAttributes;
		public bool IndicesDirty;

		private IPlatformGeometryBuffer platformBuffer;

		[YuzuAfterDeserialization]
		public void AfterDeserialization()
		{
			DirtyAttributes = Attributes.All;
		}

		public void Render(int startIndex, int indexCount)
		{
			if (platformBuffer == null) {
				platformBuffer = new PlatformGeometryBuffer(this);
			}
			platformBuffer.Render(startIndex, indexCount);
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
			if (platformBuffer != null) {
				platformBuffer.Dispose();
				platformBuffer = null;
			}
		}

		public GeometryBuffer Clone()
		{
			var clone = new GeometryBuffer();
			clone.Vertices = CloneElements(Vertices);
			clone.Colors = CloneElements(Colors);
			clone.UV1 = CloneElements(UV1);
			clone.UV2 = CloneElements(UV2);
			clone.UV3 = CloneElements(UV3);
			clone.UV4 = CloneElements(UV4);
			clone.BlendIndices = CloneElements(BlendIndices);
			clone.BlendWeights = CloneElements(BlendWeights);
			clone.Indices = CloneElements(Indices);
			clone.DirtyAttributes = Attributes.All;
			clone.IndicesDirty = true;
			return clone;
		}

		private static T[] CloneElements<T>(T[] source)
		{
			if (source != null) {
				return source.Clone() as T[];
			}
			return null;
		}
	}
}
