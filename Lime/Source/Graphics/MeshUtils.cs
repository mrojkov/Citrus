using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Lime
{
	public static class MeshUtils
	{
		public static Mesh<T> Combine<T>(params Mesh<T>[] meshes) where T : unmanaged
		{
			int numVertices = meshes.Sum(m => m.Vertices.Length);
			int numIndices = meshes.Sum(m => m.Indices.Length);
			var outVertices = new T[numVertices];
			var outIndices = new ushort[numIndices];
			int currentVertex = 0;
			int currentIndex = 0;
			foreach (var m in meshes) {
				var indices = m.Indices;
				for (int i = currentIndex; i < currentIndex + indices.Length; i++) {
					outIndices[i] = (ushort)(indices[i - currentIndex] + currentVertex);
				}
				currentIndex += indices.Length;
				var vertices = m.Vertices;
				vertices.CopyTo(outVertices, currentVertex);
				currentVertex += vertices.Length;
			}
			return new Mesh<T> {
				Vertices = outVertices,
				Indices = outIndices,
				AttributeLocations = meshes[0].AttributeLocations,
				DirtyFlags = MeshDirtyFlags.All
			};
		}

		public delegate void VertexProcessor<T>(ref T vertex) where T : unmanaged;

		public static void TransformVertices<T>(Mesh<T> mesh, VertexProcessor<T> processor) where T : unmanaged
		{
			var vertices = mesh.Vertices;
			for (int i = 0; i < vertices.Length; i++) {
				processor(ref vertices[i]);
			}
			mesh.DirtyFlags |= MeshDirtyFlags.Vertices;
		}

		/// <summary>
		/// Creates a cone standing on the X-Z plane.
		/// </summary>
		public static Mesh<VertexPositionColor> CreateCone(float height, float radius, int tesselation, Color4 color)
		{
			var indices = new ushort[tesselation * 6];
			var vertices = new VertexPositionColor[tesselation + 2];
			vertices[0] = new VertexPositionColor { Position = new Vector3(0, height, 0), Color = color };
			vertices[1] = new VertexPositionColor { Position = Vector3.Zero, Color = color };
			int j = 0;
			for (int i = 0; i < tesselation; i++) {
				vertices[i + 2] = new VertexPositionColor {
					Position = new Vector3(
						Mathf.Cos(i * Mathf.TwoPi / tesselation) * radius,
						0,
						Mathf.Sin(i * Mathf.TwoPi / tesselation) * radius),
					Color = color
				};
				indices[j++] = 0;
				indices[j++] = (ushort)((i + 1) % tesselation + 2);
				indices[j++] = (ushort)(i + 2);
				indices[j++] = 1;
				indices[j++] = (ushort)(i + 2);
				indices[j++] = (ushort)((i + 1) % tesselation + 2);
			}
			return new Mesh<VertexPositionColor> {
				Vertices = vertices,
				Indices = indices,
				AttributeLocations = new[] { ShaderPrograms.Attributes.Pos1, ShaderPrograms.Attributes.Color1 },
				DirtyFlags = MeshDirtyFlags.All
			};
		}

		/// <summary>
		/// Creates a frustum standing on the X-Z plane.
		/// </summary>
		public static Mesh<VertexPositionColor> CreateFrustum(float height, float radius1, float radius2, int tesselation, Color4 color)
		{
			var indices = new ushort[tesselation * 12];
			var vertices = new VertexPositionColor[tesselation * 2 + 2];
			vertices[0] = new VertexPositionColor { Position = new Vector3(0, height, 0), Color = color };
			vertices[1] = new VertexPositionColor { Position = Vector3.Zero, Color = color };
			int j = 0;
			for (int i = 0; i < tesselation; i++) {
				var x =	Mathf.Cos(i * Mathf.TwoPi / tesselation);
				var z = Mathf.Sin(i * Mathf.TwoPi / tesselation); 
				vertices[i * 2 + 2] = new VertexPositionColor { Position = new Vector3(x * radius2, height, z * radius2), Color = color };
				vertices[i * 2 + 3] = new VertexPositionColor { Position = new Vector3(x * radius1, 0, z * radius1), Color = color };
				var t1 = (ushort)(i * 2 + 2);
				var t2 = (ushort)((i + 1) % tesselation * 2 + 2);
				var b1 = (ushort)(i * 2 + 3);
				var b2 = (ushort)((i + 1) % tesselation * 2 + 3);
				// Top
				indices[j++] = 0;
				indices[j++] = t2;
				indices[j++] = t1;
				// Side
				indices[j++] = t1;
				indices[j++] = t2;
				indices[j++] = b2;
				indices[j++] = b1;
				indices[j++] = t1;
				indices[j++] = b2;
				// Bottom
				indices[j++] = 1;
				indices[j++] = b1;
				indices[j++] = b2;
			}
			return new Mesh<VertexPositionColor> {
				Vertices = vertices,
				Indices = indices,
				AttributeLocations = new[] { ShaderPrograms.Attributes.Pos1, ShaderPrograms.Attributes.Color1 },
				DirtyFlags = MeshDirtyFlags.All
			};
		}

		/// <summary>
		/// Creates a cylinder standing on the X-Z plane.
		/// </summary>
		public static Mesh<VertexPositionColor> CreateCylinder(float height, float radius, int tesselation, Color4 color)
		{
			return CreateFrustum(height, radius, radius, tesselation, color);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 32)]
	public struct VertexPositionColor
	{
		public Vector3 Position;
		public Color4 Color;
	}
}
