using System;
using System.Collections.Generic;

namespace Lime
{
	public class RenderBatch
	{
		public const int VertexBufferCapacity = 400;
		public const int IndexBufferCapacity = 600;

		private static Stack<RenderBatch> batchPool = new Stack<RenderBatch>();
		private static Stack<IMesh> meshPool = new Stack<IMesh>();
		private IMesh mesh;
		private bool ownsMesh;

		public Blending Blending;
		public ShaderId Shader;
		public ShaderProgram CustomShaderProgram;
		public ITexture Texture1;
		public ITexture Texture2;
		public int LastVertex;
		public int StartIndex;
		public int LastIndex;

		public IVertexBuffer<Vertex> VertexBuffer { get; private set; }
		public IIndexBuffer IndexBuffer { get; private set; }

		private void Clear()
		{
			Texture1 = Texture2 = null;
			Blending = Blending.None;
			Shader = ShaderId.None;
			CustomShaderProgram = null;
			StartIndex = LastIndex = LastVertex = 0;
			if (mesh != null) {
				if (ownsMesh) {
					ReleaseMesh(mesh);
				}
				mesh = null;
			}
			ownsMesh = false;
		}

		public void Render()
		{
			PlatformRenderer.SetTexture(Texture1, 0);
			PlatformRenderer.SetTexture(Texture2, 1);
			PlatformRenderer.SetBlending(Blending);
			PlatformRenderer.SetShader(Shader, CustomShaderProgram);
			PlatformRenderer.DrawTriangles(mesh, StartIndex, LastIndex - StartIndex);
		}

		public static RenderBatch Acquire(RenderBatch origin)
		{
			var batch = batchPool.Count == 0 ? new RenderBatch() : batchPool.Pop();
			if (origin != null) {
				batch.mesh = origin.mesh;
				batch.StartIndex = origin.LastIndex;
				batch.LastVertex = origin.LastVertex;
				batch.LastIndex = origin.LastIndex;
			} else {
				batch.ownsMesh = true;
				batch.mesh = AcquireMesh();
			}
			batch.VertexBuffer = (IVertexBuffer<Vertex>)batch.mesh.VertexBuffers[0];
			batch.IndexBuffer = batch.mesh.IndexBuffer;
			return batch;
		}

		public void Release()
		{
			Clear();
			batchPool.Push(this);
		}

		private static IMesh AcquireMesh()
		{
			if (meshPool.Count == 0) {
				var vao = new Mesh {
					IndexBuffer = new IndexBuffer { Data = new ushort[IndexBufferCapacity], Dynamic = true },
					VertexBuffers = new[] {
						new VertexBuffer<Vertex> { Data = new Vertex[VertexBufferCapacity], Dynamic = true }
					},
					Attributes = new[] { 
						new int[] {
							ShaderPrograms.Attributes.Pos1,
							ShaderPrograms.Attributes.Color1,
							ShaderPrograms.Attributes.UV1,
							ShaderPrograms.Attributes.UV2
						}
					}
				};
				return vao;
			}
			return meshPool.Pop();
		}

		private static void ReleaseMesh(IMesh item)
		{
			meshPool.Push(item);
		}
	}
}