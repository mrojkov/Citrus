using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES30;
#else
using OpenTK.Graphics.OpenGL;
#endif
using Yuzu;

namespace Lime
{
	[YuzuSpecializeWith(typeof(Lime.Mesh3D.Vertex))]
	public partial class Mesh<T> : IMesh, IGLObject, IDisposable where T : struct
	{
		private bool disposed;

		[YuzuMember]
		public int[] AttributeLocations;

		[YuzuMember]
		public T[] Vertices;

		[YuzuMember]
		[TangerineKeyframeColor(21)]
		public ushort[] Indices;

		[YuzuMember]
		public PrimitiveTopology Topology = PrimitiveTopology.TriangleList;

		public MeshDirtyFlags DirtyFlags = MeshDirtyFlags.All;

		private VertexInputLayout inputLayout;
		private VertexBuffer vertexBuffer;
		private IndexBuffer indexBuffer;

		~Mesh()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (!disposed) {
				if (vertexBuffer != null) {
					vertexBuffer.Dispose();
					vertexBuffer = null;
				}
				if (indexBuffer != null) {
					indexBuffer.Dispose();
					indexBuffer = null;
				}
				disposed = true;
			}
			GC.SuppressFinalize(this);
		}

		public void Discard()
		{
			if (vertexBuffer != null) {
				vertexBuffer.Discard();
			}
			if (indexBuffer != null) {
				indexBuffer.Discard();
			}
			DirtyFlags = MeshDirtyFlags.All;
		}

		public Mesh<T> ShallowClone()
		{
			return (Mesh<T>)MemberwiseClone();
		}

		IMesh IMesh.ShallowClone() => ShallowClone();

		public void Draw(int startVertex, int vertexCount)
		{
			PreDraw();
			PlatformRenderer.Draw(Topology, startVertex, vertexCount);
		}

		public void DrawIndexed(int startIndex, int indexCount, int baseVertex = 0)
		{
			PreDraw();
			PlatformRenderer.DrawIndexed(Topology, startIndex, indexCount, baseVertex);
		}

		private void PreDraw()
		{
			UpdateBuffers();
			UpdateInputLayout();
			PlatformRenderer.SetVertexInputLayout(inputLayout);
			PlatformRenderer.SetVertexBuffer(0, vertexBuffer, 0);
			PlatformRenderer.SetIndexBuffer(indexBuffer, 0, IndexFormat.Index16Bits);
		}

		private void UpdateBuffers()
		{
			if ((DirtyFlags & MeshDirtyFlags.Vertices) != 0) {
				if (vertexBuffer == null) {
					vertexBuffer = new VertexBuffer(true);
				}
				vertexBuffer.SetData(Vertices, Vertices?.Length ?? 0);
				DirtyFlags &= ~MeshDirtyFlags.Vertices;
			}
			if ((DirtyFlags & MeshDirtyFlags.Indices) != 0) {
				if (indexBuffer == null) {
					indexBuffer = new IndexBuffer(true);
				}
				indexBuffer.SetData(Indices, Indices?.Length ?? 0);
				DirtyFlags &= ~MeshDirtyFlags.Indices;
			}
		}

		private void UpdateInputLayout()
		{
			if (inputLayout == null || (DirtyFlags & MeshDirtyFlags.AttributeLocations) != 0) {
				var elements = new List<VertexInputElement>();
				var stride = Toolbox.SizeOf<T>();
				foreach (var elementDescription in GetElementDescriptions()) {
					elements.Add(new VertexInputElement {
						Slot = 0,
						Attribute = AttributeLocations[elements.Count],
						Stride = stride,
						Offset = elementDescription.Offset,
						Format = elementDescription.Format,
					});
				}
				inputLayout = VertexInputLayout.New(elements.ToArray());
				DirtyFlags &= ~MeshDirtyFlags.AttributeLocations;
			}
		}

		[ThreadStatic]
		private static ElementDescription[] elementDescriptions;

		private static ElementDescription[] GetElementDescriptions()
		{
			if (elementDescriptions == null) {
				elementDescriptions = GetElementDescriptionsFromReflection().ToArray();
			}
			return elementDescriptions;
		}

		private static IEnumerable<ElementDescription> GetElementDescriptionsFromReflection()
		{
			int offset = 0;
			var result = GetElementDescription(typeof(T), ref offset);
			if (result != null) {
				yield return result;
				yield break;
			}
			foreach (var field in typeof(T).GetFields()) {
				var attrs = field.GetCustomAttributes(typeof(FieldOffsetAttribute), false);
				if (attrs.Length > 0) {
					offset = (attrs[0] as FieldOffsetAttribute).Value;
				}
				result = GetElementDescription(field.FieldType, ref offset);
				if (result != null) {
					yield return result;
				} else {
					throw new InvalidOperationException();
				}
			}
		}

		private static ElementDescription GetElementDescription(Type type, ref int offset)
		{
			ElementDescription result = null;
			if (type == typeof(float)) {
				result = new ElementDescription { Format = VertexInputElementFormat.Float1, Offset = offset };
				offset += 4;
			} else if (type == typeof(Vector2)) {
				result = new ElementDescription { Format = VertexInputElementFormat.Float2, Offset = offset };
				offset += 8;
			} else if (type == typeof(Vector3)) {
				result = new ElementDescription { Format = VertexInputElementFormat.Float3, Offset = offset };
				offset += 12;
			} else if (type == typeof(Color4)) {
				result = new ElementDescription { Format = VertexInputElementFormat.UByte4Norm, Offset = offset };
				offset += 4;
			} else if (type == typeof(Mesh3D.BlendIndices)) {
				result = new ElementDescription { Format = VertexInputElementFormat.UByte4, Offset = offset };
				offset += 4;
			} else if (type == typeof(Mesh3D.BlendWeights)) {
				result = new ElementDescription { Format = VertexInputElementFormat.Float4, Offset = offset };
				offset += 16;
			}
			return result;
		}

		private class ElementDescription
		{
			public VertexInputElementFormat Format;
			public int Offset;
		}
	}
}
