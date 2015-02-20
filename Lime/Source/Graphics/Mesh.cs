using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	interface IPlatformMesh : IDisposable
	{
		void Render(int startIndex, int count);
	}

	public class Mesh : IDisposable
	{
		[Flags]
		public enum Attributes
		{
			None = 0,
			Vertex = 1,
			Color = 2,
			UV1 = 4,
			UV2 = 8,
			UV3 = 16,
			UV4 = 32,
			VertexColorUV12 = Vertex | Color | UV1 | UV2
		}

		public Vector2[] Vertices;
		public Color4[] Colors;
		public Vector2[] UV1;
		public Vector2[] UV2;
		public Vector2[] UV3;
		public Vector2[] UV4;
		public int[] Indices;

		public Attributes DirtyAttributes;
		public bool IndicesDirty;

		private IPlatformMesh platformMesh;

		public void Render(int startIndex, int count)
		{
			if (platformMesh == null) {
				platformMesh = new PlatformMesh(this);
			}
			platformMesh.Render(startIndex, count);
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
