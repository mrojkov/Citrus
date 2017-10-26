using System.Collections.Generic;

namespace Lime
{
	public struct HitTestArgs
	{
		public Ray Ray;
		public Vector2 Point;
		public float Distance;
		public Node Node;

		public HitTestArgs(Ray ray) : this()
		{
			Ray = ray;
			Distance = float.MaxValue;
		}

		public HitTestArgs(Vector2 point) : this()
		{
			Point = point;
		}
	}

	public class RenderChain
	{
		public const int LayerCount = 100;
		public int CurrentLayer;
		public readonly List<Item>[] Layers = new List<Item>[LayerCount];

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
			Render();
			Clear();
		}

		public void Render()
		{
			for (int i = 0; i < Layers.Length; i++) {
				var list = Layers[i];
				if (list == null || list.Count == 0) {
					continue;
				}
				for (int j = list.Count - 1; j >= 0; j--) {
					var t = list[j];
					t.Presenter.Render(t.Node);
				}
			}
		}

		public bool HitTest(ref HitTestArgs args)
		{
			for (int i = Layers.Length - 1; i >= 0; i--) {
				var list = Layers[i];
				if (list == null || list.Count == 0) {
					continue;
				}
				for (int j = 0; j < list.Count; j++) {
					var t = list[j];
					if (t.Presenter.PartialHitTest(t.Node, ref args)) {
						return true;
					}
				}
			}
			return false;
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