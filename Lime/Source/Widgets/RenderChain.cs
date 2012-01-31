using System.Collections.Generic;

namespace Lime
{
	public class RenderChain
	{
		const int MaxLayers = 100;
		int currentLayer;
		int maxUsedLayer;

		Stack<int> layerStack = new Stack<int>();
		Node[] layers = new Node[MaxLayers];

		public void Add(Node node)
		{
			node.NextToRender = layers[currentLayer];
			layers[currentLayer] = node;
		}

		public void Add(Node node, int layer)
		{
			if (layer != 0) {
				PushLayer(layer);
				Add(node);
				PopLayer();
			} else {
				Add(node);
			}
		}

		public void PushLayer(int layer)
		{
			if (layer > maxUsedLayer) {
				maxUsedLayer = layer;
			}
			currentLayer = layer;
			layerStack.Push(layer);
		}

		public void PopLayer()
		{
			currentLayer = layerStack.Peek();
			layerStack.Pop();
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
