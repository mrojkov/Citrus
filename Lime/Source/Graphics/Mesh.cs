using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Yuzu;

namespace Lime
{
	[YuzuSpecializeWith(typeof(Lime.Mesh3D.Vertex))]
	public unsafe partial class Mesh<T> : IMesh, IGLObject, IDisposable where T : unmanaged
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
				var bindings = new[] {
					new VertexInputLayoutBinding {
						Slot = 0,
						Stride = sizeof(T)
					}
				};
				var attributes = new List<VertexInputLayoutAttribute>();
				foreach (var elementDescription in GetElementDescriptions()) {
					attributes.Add(new VertexInputLayoutAttribute {
						Slot = 0,
						Location = AttributeLocations[attributes.Count],
						Offset = elementDescription.Offset,
						Format = elementDescription.Format,
					});
				}
				inputLayout = VertexInputLayout.New(bindings, attributes.ToArray());
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
				result = new ElementDescription { Format = Format.R32_SFloat, Offset = offset };
				offset += 4;
			} else if (type == typeof(Vector2)) {
				result = new ElementDescription { Format = Format.R32G32_SFloat, Offset = offset };
				offset += 8;
			} else if (type == typeof(Vector3)) {
				result = new ElementDescription { Format = Format.R32G32B32_SFloat, Offset = offset };
				offset += 12;
			} else if (type == typeof(Vector4)) {
				result = new ElementDescription { Format = Format.R32G32B32A32_SFloat, Offset = offset };
				offset += 16;
			} else if (type == typeof(Color4)) {
				result = new ElementDescription { Format = Format.R8G8B8A8_UNorm, Offset = offset };
				offset += 4;
			} else if (type == typeof(Mesh3D.BlendIndices)) {
				result = new ElementDescription { Format = Format.R32G32B32A32_SFloat, Offset = offset };
				offset += 16;
			} else if (type == typeof(Mesh3D.BlendWeights)) {
				result = new ElementDescription { Format = Format.R32G32B32A32_SFloat, Offset = offset };
				offset += 16;
			}
			return result;
		}

		private class ElementDescription
		{
			public Format Format;
			public int Offset;
		}
	}
}
