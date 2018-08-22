using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

		private int currentLayer;
		private List<Item> currentList;
			
		public static readonly Rectangle DefaultClipRegion = new Rectangle(-float.MaxValue, -float.MaxValue, float.MaxValue, float.MaxValue);

		/// <summary>
		/// The clip region. Usually represents viewport bounds in the coordinate space of the root widget.
		/// </summary>
		public Rectangle ClipRegion = DefaultClipRegion;

		//public Rectangle ClipRegion
		//{
		//	get { return DefaultClipRegion; }
		//	set { }
		//}

		public List<Item>[] Layers { get; private set; } = new List<Item>[LayerCount];

		public int CurrentLayer
		{
			get { return currentLayer; }
			set
			{
				currentLayer = value;
				currentList = Layers[CurrentLayer] ?? (Layers[CurrentLayer] = new List<Item>());
			}
		}

		public RenderChain()
		{
			CurrentLayer = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(Node node, IPresenter presenter)
		{
			currentList.Add(new Item { Node = node, Presenter = presenter });
		}

		public void GetRenderObjects(RenderObjectList list)
		{
			for (var i = 0; i < Layers.Length; i++) {
				var layer = Layers[i];
				if (layer == null) {
					continue;
				}
				for (var j = layer.Count - 1; j >= 0; j--) {
					var item = layer[j];
					var ro = item.Presenter.GetRenderObject(item.Node);
					if (ro != null) {
						list.Add(ro);
					}
				}
			}
		}

		private RenderObjectList renderObjects = new RenderObjectList();

		[System.Obsolete]
		public void Render()
		{
			try {
				renderObjects.Clear();
				GetRenderObjects(renderObjects);
				renderObjects.Render();
			} finally {
				renderObjects.Clear();
			}
		}

		[System.Obsolete]
		public void RenderAndClear()
		{
			Render();
			Clear();
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
