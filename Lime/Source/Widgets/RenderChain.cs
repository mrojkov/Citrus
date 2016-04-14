using System.Collections.Generic;

namespace Lime
{
	public class RenderChain
	{
		private IHitProcessor hitProcessor;

		public const int LayerCount = 100;
		public int CurrentLayer;
		public readonly List<Item>[] Layers = new List<Item>[LayerCount];

		public RenderChain(IHitProcessor hitProcessor = null)
		{
			if (hitProcessor == null) {
				hitProcessor = DefaultHitProcessor.Instance;
			}
			this.hitProcessor = hitProcessor;
		}

		public void Add(Node node, IPresenter presenter)
		{
			var list = Layers[CurrentLayer];
			if (list == null) {
				list = Layers[CurrentLayer] = new List<Item>();
			}
			list.Add(new Item { Node = node, Presenter = presenter });
		}

		public void RenderAndClear()
		{
			for (int i = 0; i < Layers.Length; i++) {
				var list = Layers[i];
				if (list == null || list.Count == 0) {
					continue;
				}
				for (int j = list.Count - 1; j >= 0; j--) {
					var t = list[j];
					hitProcessor.PerformHitTest(t.Node, t.Presenter);
					t.Presenter.Render(t.Node);
				}
				list.Clear();
			}
		}

		public void Clear()
		{
			for (int i = 0; i < Layers.Length; i++) {
				var list = Layers[i];
				if (list != null && list.Count > 0) {
					list.Clear();
				}
			}
		}

		public struct Item
		{
			public Node Node;
			public IPresenter Presenter;
		}
	}
}