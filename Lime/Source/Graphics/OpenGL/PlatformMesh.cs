#if OPENGL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if iOS || ANDROID
using OpenTK.Graphics.ES20;
#elif MAC
using MonoMac.OpenGL;
#elif WIN
using OpenTK.Graphics.OpenGL;
#endif

namespace Lime
{
	public class PlatformMesh : IPlatformMesh
	{
		public static class Attributes
		{
			public const int Vertex = 0;
			public const int Color = 1;
			public const int UV1 = 2;
			public const int UV2 = 3;
			public const int UV3 = 4;
			public const int UV4 = 5;

			public static IEnumerable<ShaderProgram.AttribLocation> GetLocations()
			{
				return new ShaderProgram.AttribLocation[] {
					new ShaderProgram.AttribLocation { Index = Vertex, Name = "inPos" },
					new ShaderProgram.AttribLocation { Index = Color, Name = "inColor" },
					new ShaderProgram.AttribLocation { Index = UV1, Name = "inTexCoords1" },
					new ShaderProgram.AttribLocation { Index = UV2, Name = "inTexCoords2" },
					new ShaderProgram.AttribLocation { Index = UV3, Name = "inTexCoords3" },
					new ShaderProgram.AttribLocation { Index = UV4, Name = "inTexCoords4" },
				};
			}
		}

		private Mesh mesh;
		private VertexBuffer verticesVBO;
		private VertexBuffer colorsVBO;
		private VertexBuffer uv1VBO;
		private VertexBuffer uv2VBO;
		private VertexBuffer uv3VBO;
		private VertexBuffer uv4VBO;
		private IndexBuffer indexBuffer;

		public PlatformMesh(Mesh mesh)
		{
			this.mesh = mesh;
			indexBuffer = new IndexBuffer();
		}

		public void Render(int startIndex, int count)
		{
			UploadVertices();
			UploadIndices();
			int offset = startIndex * sizeof(int);
			GL.DrawElements(PrimitiveType.Triangles, count, DrawElementsType.UnsignedInt, (IntPtr)offset);
		}

		private void UploadIndices()
		{
			indexBuffer.Bind(mesh.Indices, mesh.IndicesDirty);
			mesh.IndicesDirty = false;
		}

		private void UploadVertices()
		{
			var dm = mesh.DirtyAttributes;
			if (mesh.Vertices != null) {
				if (verticesVBO == null) {
					verticesVBO = new VertexBuffer(Attributes.Vertex, VertexAttribPointerType.Float, 2);
				}
				verticesVBO.Bind(mesh.Vertices, (dm & Mesh.Attributes.Vertex) != 0);
			}
			if (mesh.Colors != null) {
				if (colorsVBO == null) {
					colorsVBO = new VertexBuffer(Attributes.Color, VertexAttribPointerType.UnsignedByte, 4);
				}
				colorsVBO.Bind(mesh.Colors, (dm & Mesh.Attributes.Color) != 0);
			}
			if (mesh.UV1 != null) {
				if (uv1VBO == null) {
					uv1VBO = new VertexBuffer(Attributes.UV1, VertexAttribPointerType.Float, 2);
				}
				uv1VBO.Bind(mesh.UV1, (dm & Mesh.Attributes.UV1) != 0);
			}
			if (mesh.UV2 != null) {
				if (uv2VBO == null) {
					uv2VBO = new VertexBuffer(Attributes.UV2, VertexAttribPointerType.Float, 2);
				}
				uv2VBO.Bind(mesh.UV2, (dm & Mesh.Attributes.UV2) != 0);
			}
			if (mesh.UV3 != null) {
				if (uv3VBO == null) {
					uv3VBO = new VertexBuffer(Attributes.UV3, VertexAttribPointerType.Float, 2);
				}
				uv3VBO.Bind(mesh.UV3, (dm & Mesh.Attributes.UV3) != 0);
			}
			if (mesh.UV4 != null) {
				if (uv4VBO == null) {
					uv4VBO = new VertexBuffer(Attributes.UV4, VertexAttribPointerType.Float, 2);
				}
				uv4VBO.Bind(mesh.UV3, (dm & Mesh.Attributes.UV4) != 0);
			}
			mesh.DirtyAttributes = Mesh.Attributes.None;
		}

		public void Dispose()
		{
			DisposeVertexBuffer(ref verticesVBO);
			DisposeVertexBuffer(ref colorsVBO);
			DisposeVertexBuffer(ref uv1VBO);
			DisposeVertexBuffer(ref uv2VBO);
			DisposeVertexBuffer(ref uv3VBO);
			DisposeVertexBuffer(ref uv4VBO);
			if (indexBuffer != null) {
				indexBuffer.Dispose();
				indexBuffer = null;
			}
		}

		private static void DisposeVertexBuffer(ref VertexBuffer verticesVBO)
		{
			if (verticesVBO != null) {
				verticesVBO.Dispose();
				verticesVBO = null;
			}
		}
	}
}
#endif