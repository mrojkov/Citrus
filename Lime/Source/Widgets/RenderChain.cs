using System.Collections.Generic;

namespace Lime
{
	/// <summary>
	/// Очередь отрисовки. Представляет собой коллекцию, которая умеет сортировать объекты, учитывая их иерархию и Z-Order
	/// </summary>
	public class RenderChain
	{
		int currentLayer;
		int maxUsedLayer;

		readonly Node[] layers = new Node[Widget.MaxLayer + 1];

		/// <summary>
		/// Добавляет объект и все его дочерние объекты в очередь отрисовки
		/// </summary>
		/// <param name="node">Добавляемый объект</param>
		/// <param name="layer">Слой. Задает порядок отрисовки. Чем больше значение, тем позднее будет отрисован объект. 0 - значение по умолчанию. От 0 до 99</param>
		public void Add(Node node, int layer = 0)
		{
			if (layer != 0) {
				int oldLayer = SetCurrentLayer(layer);
				Add(node, 0);
				SetCurrentLayer(oldLayer);
			} else {
				node.NextToRender = layers[currentLayer];
				layers[currentLayer] = node;
			}
		}

		public int SetCurrentLayer(int layer)
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
        
		/// <summary>
		/// Перечисляет все объекты в том порядке, в каком они должны отрисоваться
		/// </summary>
		public IEnumerable<Node> Enumerate()
		{
			for (int i = 0; i <= maxUsedLayer; i++) {
				for (var node = layers[i]; node != null; node = node.NextToRender) {
					yield return node;
				}
			}
		}
	}
}