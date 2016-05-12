#if OPENGL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES20;
#elif MAC
using OpenTK.Graphics.OpenGL;
#elif MONOMAC
using MonoMac.OpenGL;
#endif

namespace Lime
{
	public class PlatformGeometryBuffer : IPlatformGeometryBuffer
	{
		public static class Attributes
		{
			public const int Vertex = 0;
			public const int Color = 1;
			public const int UV1 = 2;
			public const int UV2 = 3;
			public const int UV3 = 4;
			public const int UV4 = 5;
			public const int BlendIndices = 6;
			public const int BlendWeights = 7;

			public static IEnumerable<ShaderProgram.AttribLocation> GetLocations()
			{
				return new ShaderProgram.AttribLocation[] {
					new ShaderProgram.AttribLocation { Index = Vertex, Name = "inPos" },
					new ShaderProgram.AttribLocation { Index = Color, Name = "inColor" },
					new ShaderProgram.AttribLocation { Index = UV1, Name = "inTexCoords1" },
					new ShaderProgram.AttribLocation { Index = UV2, Name = "inTexCoords2" },
					new ShaderProgram.AttribLocation { Index = UV3, Name = "inTexCoords3" },
					new ShaderProgram.AttribLocation { Index = UV4, Name = "inTexCoords4" },
					new ShaderProgram.AttribLocation { Index = BlendIndices, Name = "inBlendIndices" },
					new ShaderProgram.AttribLocation { Index = BlendWeights, Name = "inBlendWeights" }
				};
			}
		}

		private GeometryBuffer source;
		private VertexBuffer verticesVBO;
		private VertexBuffer colorsVBO;
		private VertexBuffer uv1VBO;
		private VertexBuffer uv2VBO;
		private VertexBuffer uv3VBO;
		private VertexBuffer uv4VBO;
		private VertexBuffer blendIndicesVBO;
		private VertexBuffer blendWeightsVBO;
		private IndexBuffer indexBuffer;

		public PlatformGeometryBuffer(GeometryBuffer mesh)
		{
			this.source = mesh;
			indexBuffer = new IndexBuffer();
		}

		public void Render(int startIndex, int count)
		{
			UploadVertices();
			UploadIndices();
			int offset = startIndex * sizeof(short);
#if MAC || MONOMAC
			GL.DrawElements(BeginMode.Triangles, count, DrawElementsType.UnsignedShort, (IntPtr)offset);
#else
			GL.DrawElements(PrimitiveType.Triangles, count, DrawElementsType.UnsignedShort, (IntPtr)offset);
#endif
			PlatformRenderer.CheckErrors();
		}

		private void UploadIndices()
		{
			indexBuffer.Bind(source.Indices, source.IndicesDirty);
			source.IndicesDirty = false;
		}

		private void UploadVertices()
		{
			var dm = source.DirtyAttributes;
			if (source.Vertices != null) {
				if (verticesVBO == null) {
					verticesVBO = new VertexBuffer(Attributes.Vertex, VertexAttribPointerType.Float, 3);
				}
				verticesVBO.Bind(source.Vertices, (dm & GeometryBuffer.Attributes.Vertex) != 0);
			}
			if (source.Colors != null) {
				if (colorsVBO == null) {
					colorsVBO = new VertexBuffer(Attributes.Color, VertexAttribPointerType.UnsignedByte, 4, normalized: true);
				}
				colorsVBO.Bind(source.Colors, (dm & GeometryBuffer.Attributes.Color) != 0);
			}
			if (source.UV1 != null) {
				if (uv1VBO == null) {
					uv1VBO = new VertexBuffer(Attributes.UV1, VertexAttribPointerType.Float, 2);
				}
				uv1VBO.Bind(source.UV1, (dm & GeometryBuffer.Attributes.UV1) != 0);
			}
			if (source.UV2 != null) {
				if (uv2VBO == null) {
					uv2VBO = new VertexBuffer(Attributes.UV2, VertexAttribPointerType.Float, 2);
				}
				uv2VBO.Bind(source.UV2, (dm & GeometryBuffer.Attributes.UV2) != 0);
			}
			if (source.UV3 != null) {
				if (uv3VBO == null) {
					uv3VBO = new VertexBuffer(Attributes.UV3, VertexAttribPointerType.Float, 2);
				}
				uv3VBO.Bind(source.UV3, (dm & GeometryBuffer.Attributes.UV3) != 0);
			}
			if (source.UV4 != null) {
				if (uv4VBO == null) {
					uv4VBO = new VertexBuffer(Attributes.UV4, VertexAttribPointerType.Float, 2);
				}
				uv4VBO.Bind(source.UV3, (dm & GeometryBuffer.Attributes.UV4) != 0);
			}
			if (source.BlendIndices != null) {
				if (blendIndicesVBO == null) {
					blendIndicesVBO = new VertexBuffer(Attributes.BlendIndices, VertexAttribPointerType.UnsignedByte, 4);
				}
				blendIndicesVBO.Bind(source.BlendIndices, (dm & GeometryBuffer.Attributes.BlendIndices) != 0);
			}
			if (source.BlendWeights != null) {
				if (blendWeightsVBO == null) {
					blendWeightsVBO = new VertexBuffer(Attributes.BlendWeights, VertexAttribPointerType.Float, 4);
				}
				blendWeightsVBO.Bind(source.BlendWeights, (dm & GeometryBuffer.Attributes.BlendWeights) != 0);
			}
			source.DirtyAttributes = GeometryBuffer.Attributes.None;
		}

		public void Dispose()
		{
			DisposeVertexBuffer(ref verticesVBO);
			DisposeVertexBuffer(ref colorsVBO);
			DisposeVertexBuffer(ref uv1VBO);
			DisposeVertexBuffer(ref uv2VBO);
			DisposeVertexBuffer(ref uv3VBO);
			DisposeVertexBuffer(ref uv4VBO);
			DisposeVertexBuffer(ref blendIndicesVBO);
			DisposeVertexBuffer(ref blendWeightsVBO);
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