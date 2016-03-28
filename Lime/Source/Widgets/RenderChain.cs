using System.Collections.Generic;

namespace Lime
{
	/// <summary>
	/// Очередь отрисовки. Представляет собой коллекцию, которая умеет сортировать объекты, учитывая их иерархию и Z-Order
	/// </summary>
	public class RenderChain
	{
		private int currentLayer;
		private IHitProcessor hitProcessor;

		public int MaxUsedLayer { get; private set; }

		public readonly Node[] Layers = new Node[Widget.MaxLayer + 1];

		public RenderChain(IHitProcessor hitProcessor = null)
		{
			if (hitProcessor == null) {
				hitProcessor = DefaultHitProcessor.Instance;
			}
			this.hitProcessor = hitProcessor;
		}
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
				node.NextToRender = Layers[currentLayer];
				Layers[currentLayer] = node;
			}
		}

		public int SetCurrentLayer(int layer)
		{
			if (layer > MaxUsedLayer) {
				MaxUsedLayer = layer;
			}
			int oldLayer = currentLayer;
			currentLayer = layer;
			return oldLayer;
		}

		public void RenderAndClear()
		{
			for (int i = 0; i <= MaxUsedLayer; i++) {
				Node node = Layers[i];
				while (node != null) {
					hitProcessor.PerformHitTest(node);
					node.Render();
					Node next = node.NextToRender;
					node.NextToRender = null;
					node = next;
				}
				Layers[i] = null;
			}
			MaxUsedLayer = 0;
		}

		public void Clear()
		{
			for (int i = 0; i <= MaxUsedLayer; i++) {
				Node node = Layers[i];
				while (node != null) {
					Node next = node.NextToRender;
					node.NextToRender = null;
					node = next;
				}
				Layers[i] = null;
			}
			MaxUsedLayer = 0;
		}

		/// <summary>
		/// Перечисляет все объекты в том порядке, в каком они должны отрисоваться
		/// </summary>
		public IEnumerable<Node> Enumerate()
		{
			for (int i = 0; i <= MaxUsedLayer; i++) {
				for (var node = Layers[i]; node != null; node = node.NextToRender) {
					yield return node;
				}
			}
		}
	}
}