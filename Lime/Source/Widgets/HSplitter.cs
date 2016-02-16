using System;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	public class HSplitter : Widget
	{
		private const float activeAreaWidth = 10;

		public HSplitter()
		{
			Layout = new HBoxLayout { Spacing = 4 };
			Tasks.Add(MainTask());
		}

		private IEnumerator<object> MainTask()
		{
			while (true) {
				if (Input.WasMousePressed()) {
					int splitterIndex;
					if (FindSplitterUnderMouse(out splitterIndex)) {
						yield return DragSplitterTask(splitterIndex);
					}
				}
				yield return null;
			}
		}

		private IEnumerator<object> DragSplitterTask(int index)
		{
			var widgets = GetWidgets();
			var initialMousePosition = Input.MousePosition;
			var initialWidths = widgets.Select(i => i.Width).ToList();
			Input.CaptureMouse();
			while (Input.IsMousePressed()) {
				var dragDelta = Input.MousePosition.X - initialMousePosition.X;
				AdjustStretchDelta(initialWidths[index], widgets[index], ref dragDelta);
				dragDelta = -dragDelta;
				AdjustStretchDelta(initialWidths[index + 1], widgets[index + 1], ref dragDelta);
				dragDelta = -dragDelta;
				for (int i = 0; i < widgets.Count; i++) {
					GetLayoutCell(widgets[i]).StretchX = initialWidths[i];
				}
				GetLayoutCell(widgets[index]).StretchX += dragDelta;
				GetLayoutCell(widgets[index + 1]).StretchX -= dragDelta;
				Layout.InvalidateConstraintsAndArrangement(this);
				yield return null;
			}
			Input.ReleaseMouse();
		}

		private void AdjustStretchDelta(float initialWidth, Widget widget, ref float delta)
		{
			if (initialWidth + delta <= widget.MinWidth) {
				delta = widget.MinWidth - initialWidth;
			}
			if (initialWidth + delta >= widget.MaxWidth) {
				delta = widget.MaxWidth - initialWidth;
			}
		}

		private LayoutCell GetLayoutCell(Widget widget)
		{
			return widget.LayoutCell ?? (widget.LayoutCell = new LayoutCell());
		}

		private bool FindSplitterUnderMouse(out int splitterIndex)
		{
			var widgets = GetWidgets();
			for (int i = 0; i < widgets.Count - 1; i++) {
				var a = widgets[i].CalcAABBInWindowSpace();
				var b = widgets[i + 1].CalcAABBInWindowSpace();
				var x = (a.Right + b.Left) / 2;
				if (Mathf.Abs(Input.MousePosition.X - x) < activeAreaWidth / 2) {
					if (Input.MousePosition.Y >= a.Top && Input.MousePosition.Y <= a.Bottom) {
						splitterIndex = i;
						return true;
					}
				}
			}
			splitterIndex = -1;
			return false;
		}

		private List<Widget> GetWidgets()
		{
			return Nodes.OfType<Widget>().ToList();
		}
	}
}