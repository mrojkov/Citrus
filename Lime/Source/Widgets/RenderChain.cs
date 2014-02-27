using System.Collections.Generic;

namespace Lime
{
	public class RenderChain
	{
		const int MaxLayers = 100;
		int currentLayer;
		int maxUsedLayer;

		readonly Node[] layers = new Node[MaxLayers];

		public void Add(Node node, int layer = 0)
		{
			if (layer != 0) {
				int oldLayer = SetLayer(layer);
				Add(node, 0);
				SetLayer(oldLayer);
			} else {
				node.NextToRender = layers[currentLayer];
				layers[currentLayer] = node;
			}
		}

		public int SetLayer(int layer)
		{
			if (layer > maxUsedLayer) {
				maxUsedLayer = layer;
			}
			int oldLayer = currentLayer;
			currentLayer = layer;
			return oldLayer;
		}

		public void RenderAndClear()
		{
			for (int i = 0; i <= maxUsedLayer; i++) {
				Node node = layers[i];
				while (node != null) {
					node.Render();
					Node next = node.NextToRender;
					node.NextToRender = null;
					node = next;
				}
				layers[i] = null;
			}
			maxUsedLayer = 0;
		}
	}
}
