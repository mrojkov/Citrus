using System.Collections.Generic;

namespace Lime
{
	public class RenderList
	{
		public readonly List<IRenderBatch> Batches = new List<IRenderBatch>();
		private IRenderBatch lastBatch;

		public bool Empty { get { return lastBatch == null; } }

		public RenderBatch<TVertex> GetBatch<TVertex>(ITexture texture1, ITexture texture2, IMaterial material, int vertexCount, int indexCount)
			where TVertex : struct
		{
			var atlas1 = texture1?.AtlasTexture;
			var atlas2 = texture2?.AtlasTexture;
			var typedLastBatch = lastBatch as RenderBatch<TVertex>;
			var needMesh = typedLastBatch == null ||
				typedLastBatch.LastVertex + vertexCount > RenderBatchLimits.MaxVertices ||
				typedLastBatch.LastIndex + indexCount > RenderBatchLimits.MaxIndices;
			if (needMesh ||
				typedLastBatch.Texture1 != atlas1 ||
				typedLastBatch.Texture2 != atlas2 ||
				typedLastBatch.Material != material ||
				typedLastBatch.Material.PassCount != 1
			) {
				typedLastBatch = RenderBatch<TVertex>.Acquire(needMesh ? null : typedLastBatch);
				typedLastBatch.Texture1 = atlas1;
				typedLastBatch.Texture2 = atlas2;
				typedLastBatch.Material = material;
				Batches.Add(typedLastBatch);
				lastBatch = typedLastBatch;
			}
			return typedLastBatch;
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
