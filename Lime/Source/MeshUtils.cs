using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Lime
{
	public partial class Mesh : IMesh, IDisposable, IGLObject
	{
		public static IMesh Combine<T>(params IMesh[] meshes) where T : struct
		{
			if (meshes.Any(m => m.VertexBuffers.Length != 1 || !(m.VertexBuffers[0] is VertexBuffer<T>))) {
				throw new InvalidOperationException();
			}
			int numVertices = meshes.Sum(m => ((VertexBuffer<T>)m.VertexBuffers[0]).Data.Length);
			int numIndices = meshes.Sum(m => m.IndexBuffer.Data.Length);
			var outVertices = new VertexBuffer<T> { Data = new T[numVertices] };
			var outIndices = new IndexBuffer { Data = new ushort[numIndices] };
			int currentVertex = 0;
			int currentIndex = 0;
			foreach (var m in meshes) {
				var indices = m.IndexBuffer.Data;
				for (int i = currentIndex; i < currentIndex + indices.Length; i++) {
					outIndices.Data[i] = (ushort)(indices[i - currentIndex] + currentVertex);
				}
				currentIndex += indices.Length;
				var vertices = ((VertexBuffer<T>)m.VertexBuffers[0]).Data;
				vertices.CopyTo(outVertices.Data, currentVertex);
				currentVertex += vertices.Length;
			}
			return new Mesh {
				VertexBuffers = new [] { outVertices },
				IndexBuffer = outIndices,
				Attributes = new[] { (int[])meshes[0].Attributes[0].Clone() }
			};
		}

		public delegate void VertexProcessor<T>(ref T vertex) where T : struct;

		public static void TransformVertices<T>(IMesh mesh, VertexProcessor<T> processor) where T : struct
		{
			if (mesh.VertexBuffers.Length != 1 || !(mesh.VertexBuffers[0] is VertexBuffer<T>)) {
				throw new InvalidOperationException();
			}
			var vertices  = ((VertexBuffer<T>)mesh.VertexBuffers[0]).Data;
			for (int i = 0; i < vertices.Length; i++) {
				processor(ref vertices[i]);
			}
			mesh.VertexBuffers[0].Dirty = true;
		}

		/// <summary>
		/// Creates a cone standing on the X-Z plane.
		/// </summary>
		public static IMesh CreateCone(float height, float radius, int tesselation, Color4 color)
		{
			var vb = new VertexBuffer<PositionColor>();
			var ib = new IndexBuffer();
			ib.Data = new ushort[tesselation * 6];
			vb.Data = new PositionColor[tesselation + 2];
			vb.Data[0] = new PositionColor { Position = new Vector3(0, height, 0), Color = color };
			vb.Data[1] = new PositionColor { Position = Vector3.Zero, Color = color };
			int j = 0;
			for (int i = 0; i < tesselation; i++) {
				vb.Data[i + 2] = new PositionColor {
					Position = new Vector3(
						Mathf.Cos(i * Mathf.TwoPi / tesselation) * radius,
						0,
						Mathf.Sin(i * Mathf.TwoPi / tesselation) * radius),
					Color = color
				};
				ib.Data[j++] = 0;
				ib.Data[j++] = (ushort)((i + 1) % tesselation + 2);
				ib.Data[j++] = (ushort)(i + 2);
				ib.Data[j++] = 1;
				ib.Data[j++] = (ushort)(i + 2);
				ib.Data[j++] = (ushort)((i + 1) % tesselation + 2);
			}
			var mesh = new Mesh();
			mesh.VertexBuffers = new[] { vb };
			mesh.Attributes = new[] { new[] { ShaderPrograms.Attributes.Pos1, ShaderPrograms.Attributes.Color1 } };
			mesh.IndexBuffer = ib;
			return mesh;
		}

		/// <summary>
		/// Creates a frustum standing on the X-Z plane.
		/// </summary>
		public static IMesh CreateFrustum(float height, float radius1, float radius2, int tesselation, Color4 color)
		{
			var vb = new VertexBuffer<PositionColor>();
			var ib = new IndexBuffer();
			ib.Data = new ushort[tesselation * 12];
			vb.Data = new PositionColor[tesselation * 2 + 2];
			vb.Data[0] = new PositionColor { Position = new Vector3(0, height, 0), Color = color };
			vb.Data[1] = new PositionColor { Position = Vector3.Zero, Color = color };
			int j = 0;
			for (int i = 0; i < tesselation; i++) {
				var x =	Mathf.Cos(i * Mathf.TwoPi / tesselation);
				var z = Mathf.Sin(i * Mathf.TwoPi / tesselation); 
				vb.Data[i * 2 + 2] = new PositionColor { Position = new Vector3(x * radius2, height, z * radius2), Color = color };
				vb.Data[i * 2 + 3] = new PositionColor { Position = new Vector3(x * radius1, 0, z * radius1), Color = color };
				var t1 = (ushort)(i * 2 + 2);
				var t2 = (ushort)((i + 1) % tesselation * 2 + 2);
				var b1 = (ushort)(i * 2 + 3);
				var b2 = (ushort)((i + 1) % tesselation * 2 + 3);
				// Top
				ib.Data[j++] = 0;
				ib.Data[j++] = t2;
				ib.Data[j++] = t1;
				// Side
				ib.Data[j++] = t1;
				ib.Data[j++] = t2;
				ib.Data[j++] = b2;
				ib.Data[j++] = b1;
				ib.Data[j++] = t1;
				ib.Data[j++] = b2;
				// Bottom
				ib.Data[j++] = 1;
				ib.Data[j++] = b1;
				ib.Data[j++] = b2;
			}
			var mesh = new Mesh();
			mesh.VertexBuffers = new[] { vb };
			mesh.Attributes = new[] { new[] { ShaderPrograms.Attributes.Pos1, ShaderPrograms.Attributes.Color1 } };
			mesh.IndexBuffer = ib;
			return mesh;
		}

		/// <summary>
		/// Creates a cylinder standing on the X-Z plane.
		/// </summary>
		public static IMesh CreateCylinder(float height, float radius, int tesselation, Color4 color)
		{
			return CreateFrustum(height, radius, radius, tesselation, color);
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 32)]
		public struct PositionColor
		{
			public Vector3 Position;
			public Color4 Color;
		}
	}
}
