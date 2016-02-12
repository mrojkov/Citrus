using System;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	public class VSplitter : Widget
	{
		private const float activeAreaHeight = 10;

		public VSplitter()
		{
			Layout = new VBoxLayout { Spacing = 4 };
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
			var initialHeights = widgets.Select(i => i.Height).ToList();
			Input.CaptureMouse();
			while (Input.IsMousePressed()) {
				var dragDelta = Input.MousePosition.Y - initialMousePosition.Y;
				AdjustStretchDelta(initialHeights[index], widgets[index], ref dragDelta);
				dragDelta = -dragDelta;
				AdjustStretchDelta(initialHeights[index + 1], widgets[index + 1], ref dragDelta);
				dragDelta = -dragDelta;
				for (int i = 0; i < widgets.Count; i++) {
					GetLayoutCell(widgets[i]).StretchY = initialHeights[i];
				}
				GetLayoutCell(widgets[index]).StretchY += dragDelta;
				GetLayoutCell(widgets[index + 1]).StretchY -= dragDelta;
				Layout.InvalidateConstraintsAndArrangement(this);
				yield return null;
			}
			Input.ReleaseMouse();
		}

		private void AdjustStretchDelta(float initialHeight, Widget widget, ref float delta)
		{
			if (initialHeight + delta <= widget.MinHeight) {
				delta = widget.MinHeight - initialHeight;
			}
			if (initialHeight + delta >= widget.MaxHeight) {
				delta = widget.MaxHeight - initialHeight;
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
				var y = (a.Bottom + b.Top) / 2;
				if (Mathf.Abs(Input.MousePosition.Y - y) < activeAreaHeight / 2) {
					if (Input.MousePosition.X >= a.Left && Input.MousePosition.X <= a.Right) {
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