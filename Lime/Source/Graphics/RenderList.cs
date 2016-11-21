using System.Collections.Generic;

namespace Lime
{
	public class RenderList
	{
		private readonly List<RenderBatch> Batches = new List<RenderBatch>();
		private RenderBatch lastBatch;
		public bool Empty => lastBatch == null;
		public int LastFrameBatchCount { get; private set; }
		private bool wasEndFrame;

		public RenderBatch GetBatch(ITexture texture1, ITexture texture2, Blending blending, ShaderId shader, ShaderProgram customShaderProgram, int vertexCount, int indexCount)
		{
			bool needMesh = lastBatch == null ||
				lastBatch.LastVertex + vertexCount > lastBatch.Geometry.Vertices.Length ||
				lastBatch.LastIndex + indexCount > lastBatch.Geometry.Indices.Length;
			if (!needMesh &&
				(GetTextureHandle(lastBatch.Texture1) == GetTextureHandle(texture1)) &&
				(GetTextureHandle(lastBatch.Texture2) == GetTextureHandle(texture2)) &&
				lastBatch.Blending == blending &&
				lastBatch.Shader == shader &&
				lastBatch.CustomShaderProgram == customShaderProgram) {
				return lastBatch;
			}
			var batch = RenderBatchPool.Acquire();
			if (needMesh) {
				batch.Geometry = GeometryBufferPool.Acquire();
				batch.OwnsMesh = true;
				lastBatch = batch;
			}
			batch.StartIndex = lastBatch.LastIndex;
			batch.LastVertex = lastBatch.LastVertex;
			batch.LastIndex = lastBatch.LastIndex;
			batch.Geometry = lastBatch.Geometry;
			batch.Texture1 = texture1;
			batch.Texture2 = texture2;
			batch.Blending = blending;
			batch.Shader = shader;
			batch.CustomShaderProgram = customShaderProgram;
			Batches.Add(batch);
			lastBatch = batch;
			return batch;
		}

		private static uint GetTextureHandle(ITexture texture)
		{
			return texture?.GetHandle() ?? 0;
		}

		public void Render()
		{
			foreach (var batch in Batches) {
				batch.Render();
			}
		}

		private void Clear()
		{
			if (lastBatch == null) {
				return;
			}
			foreach (var i in Batches) {
				RenderBatchPool.Release(i);
			}
			Batches.Clear();
			lastBatch = null;
		}

		public void Flush()
		{
			if (lastBatch != null) {
				Render();
				if (wasEndFrame) {
					LastFrameBatchCount = 0;
					wasEndFrame = false;
				}
				LastFrameBatchCount += Batches.Count;
				Clear();
			}
		}

		public void EndFrame()
		{
			wasEndFrame = true;
		}
	}
}
