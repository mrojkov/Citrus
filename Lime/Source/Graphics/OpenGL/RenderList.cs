using System.Collections.Generic;

namespace Lime
{
	public unsafe class RenderList
	{
		public readonly List<RenderBatch> Batches = new List<RenderBatch>();
		public readonly List<VertexBuffer> Buffers = new List<VertexBuffer>();
		private VertexBuffer lastBuffer;
		private RenderBatch lastBatch;

		public bool Empty { get { return lastBuffer == null; } }
		
		public RenderBatch RequestForBatch(ITexture texture1, ITexture texture2, Blending blending, ShaderId shader, ShaderProgram customShaderProgram, int numVertices, int numIndices)
		{
			if (lastBuffer == null || lastBuffer.VertexCount + numVertices > lastBuffer.Capacity) {
				lastBuffer = VertexBufferPool.Acquire();
				Buffers.Add(lastBuffer);
			} else if ((GetTextureHandle(lastBatch.Texture1) == GetTextureHandle(texture1)) &&
				(GetTextureHandle(lastBatch.Texture2) == GetTextureHandle(texture2)) &&
				lastBatch.IndexBuffer.IndexCount + numIndices <= lastBatch.IndexBuffer.Capacity &&
				lastBatch.Blending == blending &&
				lastBatch.Shader == shader &&
				lastBatch.CustomShaderProgram == customShaderProgram) 
			{
				return lastBatch;
			}
			lastBatch = RenderBatchPool.Acquire();
			lastBatch.IndexBuffer = IndexBufferPool.Acquire();
			lastBatch.VertexBuffer = lastBuffer;
			lastBatch.Texture1 = texture1;
			lastBatch.Texture2 = texture2;
			lastBatch.Blending = blending;
			lastBatch.Shader = shader;
			lastBatch.CustomShaderProgram = customShaderProgram;
			Batches.Add(lastBatch);
			return lastBatch;
		}

		private uint GetTextureHandle(ITexture texture)
		{
			return texture == null ? 0 : texture.GetHandle();
		}

		public void Render()
		{
			VertexBuffer buffer = null;
			foreach (var batch in Batches) {
				if (buffer != batch.VertexBuffer) {
					buffer = batch.VertexBuffer;
					buffer.Bind();
				}
				batch.Render();
			}
			PlatformRenderer.CheckErrors();
		}

		public void Clear()
		{
			if (lastBuffer == null) {
				return;
			}
			foreach (var i in Batches) {
				RenderBatchPool.Release(i);
			}
			Batches.Clear();
			foreach (var i in Buffers) {
				VertexBufferPool.Release(i);
			}
			Buffers.Clear();
			lastBuffer = null;
			lastBatch = null;
		}

		public void Flush()
		{
			if (lastBuffer != null) {
				Render();
				Clear();
			}
		}
	}
}
