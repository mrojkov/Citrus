using System.Collections.Generic;

namespace Lime
{
	public class RenderList
	{
		public readonly List<RenderBatch> Batches = new List<RenderBatch>();
		private RenderBatch lastBatch;

		public bool Empty { get { return lastBatch == null; } }

		public RenderBatch GetBatch(IMaterial material, int vertexCount, int indexCount)
		{
			bool needMesh = lastBatch == null || 
				lastBatch.LastVertex + vertexCount > RenderBatch.VertexBufferCapacity ||
				lastBatch.LastIndex + indexCount > RenderBatch.IndexBufferCapacity;
			if (!needMesh && lastBatch.Material == material && material.PassCount == 1) {
				return lastBatch;
			}
			var batch = RenderBatch.Acquire(needMesh ? null : lastBatch);
			batch.Material = material;
			Batches.Add(batch);
			lastBatch = batch;
			return batch;
		}

		public void Render()
		{
			foreach (var batch in Batches) {
				batch.Render();
			}
		}

		public void Clear()
		{
			if (lastBatch == null) {
				return;
			}
			foreach (var i in Batches) {
				i.Release();
			}
			Batches.Clear();
			lastBatch = null;
		}

		public void Flush()
		{
			if (lastBatch != null) {
				Render();
				Clear();
			}
		}
	}
}
