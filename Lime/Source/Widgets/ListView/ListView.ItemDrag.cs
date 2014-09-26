using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public partial class ListView : ScrollViewWithSlider, ICollection<Widget>
	{
		public delegate bool CanDragItemToDelegate(Widget itemBeingDragged, Widget destinationItem);
		public bool CanDragItems;
		public CanDragItemToDelegate CanDragItemTo;
		public event Action<Widget> ItemDragging;
		public event Action<Widget> ItemDragged;

		private const float MouseAccidentialMoveThreshold = 20;
		private const float DelayBeforeItemDrag = 0.3f;
		private bool dragInProgress;

		private IEnumerator<object> DragItemMainTask()
		{
			while (true) {
				yield return 0;
				if (!CanDragItems) {
					continue;
				}
				while (!Frame.Input.WasMousePressed()) {
					yield return 0;
				}
				var item = this.FirstOrDefault(i => i.IsMouseOver());
				if (item == null) {
					continue;
				}
				var prevMousePosition = Input.MousePosition;
				yield return DelayBeforeItemDrag;
				if ((Input.MousePosition - prevMousePosition).Length < MouseAccidentialMoveThreshold) {
					yield return DragItemTask(item);
				}
				while (Input.IsMousePressed()) {
					yield return 0;
				}
			}
		}

		private IEnumerator<object> DragItemTask(Widget item)
		{
			if (ItemDragging != null) {
				ItemDragging(item);
			}
			dragInProgress = true;
			try {
				while (item.Input.IsMousePressed() || Frame.Input.IsMousePressed()) {
					var bounds = item.CalcAABBInSpaceOf(World.Instance);
					var i = this.IndexOf(item);
					if (Input.MousePosition.Y < bounds.Top && i > 0) {
						if (CanDragItemTo(item, this[i - 1])) {
							yield return SwapItemsTask(item, this[i - 1]);
						}
					}
					if (Input.MousePosition.Y > bounds.Bottom && i < Count - 1) {
						if (CanDragItemTo(item, this[i + 1])) {
							yield return SwapItemsTask(item, this[i + 1]);
						}
					}
					yield return 0;
				}
			} finally {
				dragInProgress = false;
				if (ItemDragged != null) {
					ItemDragged(item);
				}
			}
		}

		private IEnumerator<object> SwapItemsTask(Widget item1, Widget item2)
		{
			if (item1.Y > item2.Y) {
				Lime.Toolbox.Swap(ref item1, ref item2);
			}
			var a = item1.Position;
			var b = item2.Position;
			foreach (float t in TaskList.SinMotion(0.15f, 0, 1)) {
				item1.Position = Vector2.Lerp(t, a, a + item2.Height * Vector2.Down);
				item2.Position = Vector2.Lerp(t, b, a);
				yield return 0;
			}
			SwapWidgets(item1, item2);
		}

		private void SwapWidgets(Widget item1, Widget item2)
		{
			var i1 = this.IndexOf(item1);
			var i2 = this.IndexOf(item2);
			this[i2] = new Widget();
			this[i1] = item2;
			this[i2] = item1;
			Refresh();
		}

		protected override bool IsItemDragInProgress()
		{
			return dragInProgress;
		}
	}
}
